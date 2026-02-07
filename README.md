# MiniECS - Lightweight Entity Component System for Unity

## 📖 Visão Geral

**MiniECS** é um framework ECS (Entity Component System) leve, rápido e extremamente simples, projetado especificamente para jogos multiplayer em Unity com **Netcode for GameObjects**. É uma alternativa muito mais acessível e performática comparada ao complexo e pesado **Unity ECS + DOTS**, mantendo toda a elegância da arquitetura baseada em componentes.

### O que é ECS?

Entity Component System é um padrão arquitetural que separa dados (Components) da lógica (Systems):
- **Entities**: Identificadores únicos para objetos do jogo
- **Components**: Dados puros (structs) sem lógica
- **Systems**: Lógica que processa componentes de múltiplas entities

Isso resulta em código mais limpo, reutilizável, testável e infinitamente mais performático.

---

## 🎯 O que MiniECS faz

MiniECS permite que você:

1. **Organize sua lógica de jogo** em Systems independentes que operam sobre componentes de dados puros
2. **Sincronize estado entre servidor e cliente** de forma simples com Netcode for GameObjects
3. **Maximize performance** eliminando overhead de MonoBehaviours e OOP tradicional
4. **Evite o overhead** do complexo Unity ECS/DOTS enquanto mantém padrões modernos
5. **Implemente networking** com código mínimo - sem necessidade de serializadores complexos

### Casos de Uso Ideais

- Jogos multiplayer competitivos (Battle Royale, MOBA, FPS)
- MMOs e mundos persistentes
- Simulações de muitos atores (NPCs, partículas, física)
- Qualquer jogo que precise de sincronização precisa entre servidor e cliente

---

## ⚡ Vantagens de Performance

### Benefícios Técnicos da Arquitetura ECS

1. **Zero Reflection em Runtime** - Acesso direto aos componentes via queries compiladas, sem GetComponent
2. **Cache-Friendly Memory** - Componentes organizados em arrays contíguos para melhor utilização de cache
3. **Zero Allocations em Queries** - Iteração eficiente sem criar garbage collections
4. **Código Simples e Direto** - Sem overhead de MonoBehaviour ou Game Objects complexos
5. **Minimal Footprint** - Framework extremamente leve, apenas o essencial para ECS funcionar

### Comparado com Abordagens Tradicionais

- **Vs MonoBehaviour**: Elimina o overhead de GetComponent em loops, mantém dados organizados na memória
- **Vs Pooling Manual**: Gerenciamento automático de entidades sem necessidade de listas manuais
- **Vs Unity ECS+DOTS**: Mesma eficiência sem a complexidade e peso do framework completo
- **Vs Network Manual**: Integração nativa com Netcode for GameObjects sem serialização complexa

---

## 🔄 Sincronização com Netcode for GameObjects

MiniECS integra-se perfeitamente com Netcode for GameObjects, sendo **muito mais simples** que abordagens tradicionais.

### Arquitetura Cliente-Servidor

```
┌─────────────────┐                      ┌─────────────────┐
│   CLIENT        │                      │     SERVER      │
├─────────────────┤                      ├─────────────────┤
│ Input System    │                      │ Movement System │
│ Animation System│ ◄─── RPC/NetworkVar─►│ Position System │
│ Sync System     │                      │ AI System       │
└─────────────────┘                      └─────────────────┘
```

### Exemplo Prático: Sistema de Movimento Sincronizado

#### 1. **Definir Componentes (Dados Puros)**

```csharp
// Position.cs
namespace Game.Components
{
    public struct Position
    {
        public Vector2 Value;
    }
}

// Velocity.cs
namespace Game.Components
{
    public struct Velocity
    {
        public Vector2 Value;
    }
}

// InputData.cs - Apenas no servidor
namespace Game.Components
{
    public struct InputData
    {
        public Vector2 Move;
    }
}

// AnimatorRef.cs - Apenas no cliente
namespace Game.Components
{
    public struct AnimatorRef
    {
        public Animator Value;
    }
}
```

#### 2. **Sistema de Movimento (Servidor)**

```csharp
// PlayerMovementSystem.cs
using MiniECS;
using Game.Components;
using UnityEngine;

namespace Game.Systems
{
    public struct PlayerMovementSystem : ISystem, IServerSystem
    {
        private const float SPEED = 5f;

        public void OnCreate(ref SystemContext ctx) { }

        public void OnUpdate(ref SystemContext ctx)
        {
            // Etapa 1: Atualizar velocidade baseado no input
            foreach (var (e, input, vel) in 
                EntityManager.Query<InputData, ValueRW<Velocity>>().ForEach())
            {
                vel.Value.Value = input.Move * SPEED;
            }

            // Etapa 2: Atualizar posição baseado na velocidade
            foreach (var (e, pos, vel) in 
                EntityManager.Query<ValueRW<Position>, Velocity>().ForEach())
            {
                pos.Value.Value += vel.Value * ctx.DeltaTime;
            }
        }

        public void OnDestroy(ref SystemContext ctx) { }
    }
}
```

#### 3. **Sistema de Sincronização (Network)**

```csharp
// PlayerNetworkAdapter.cs
using Game.Components;
using MiniECS;
using Unity.Netcode;
using UnityEngine;

namespace Game.Network
{
    public class PlayerNetworkAdapter : NetworkBehaviour
    {
        [SerializeField] private Animator animator;
        private Entity entity;

        // Sincronizar velocidade do servidor para clientes (para animação)
        private NetworkVariable<Vector2> netVelocity = 
            new NetworkVariable<Vector2>(
                default, 
                NetworkVariableReadPermission.Everyone, 
                NetworkVariableWritePermission.Server
            );

        public override void OnNetworkSpawn()
        {
            animator = GetComponent<Animator>();
            entity = EntityManager.CreateEntity();

            if (IsServer)
            {
                // Servidor: gerencia toda a lógica e física
                EntityManager.Add(entity, new Position { Value = transform.position });
                EntityManager.Add(entity, new Velocity());
                EntityManager.Add(entity, new InputData());
                EntityManager.Add(entity, new TransformRef { Value = transform });
            }

            if (IsClient)
            {
                // Cliente: apenas visualiza (animação)
                // Position é controlada pelo NetworkTransform (authoridade do servidor)
                EntityManager.Add(entity, new Velocity());
                EntityManager.Add(entity, new AnimatorRef { Value = animator });
            }
        }

        private void Update()
        {
            if (entity.Id == 0) return;

            // CLIENTE: Coletar input e enviar ao servidor
            if (IsOwner && IsClient)
            {
                var x = Input.GetAxis("Horizontal");
                var y = Input.GetAxis("Vertical");
                var input = new Vector2(x, y);

                // Só enviar se houver input ou periodicamente
                if (input != Vector2.zero || Time.frameCount % 5 == 0)
                {
                    SubmitInputServerRpc(input);
                }
            }

            // SERVIDOR: Sincronizar velocidade para clientes
            if (IsServer && EntityManager.Has<Velocity>(entity.Id))
            {
                netVelocity.Value = EntityManager
                    .Pool<Velocity>()
                    .Get(entity.Id)
                    .Value;
            }

            // CLIENTE: Receber velocidade do servidor
            if (IsClient && EntityManager.Has<Velocity>(entity.Id))
            {
                EntityManager
                    .Pool<Velocity>()
                    .Get(entity.Id)
                    .Value = netVelocity.Value;
            }
        }

        [ServerRpc]
        private void SubmitInputServerRpc(Vector2 input)
        {
            if (EntityManager.Has<InputData>(entity.Id))
            {
                EntityManager
                    .Pool<InputData>()
                    .Get(entity.Id)
                    .Move = input;
            }
        }
    }
}
```

#### 4. **Sistema de Animação (Cliente)**

```csharp
// AnimationSystemClient.cs
using MiniECS;
using Game.Components;
using UnityEngine;

namespace Game.Systems
{
    public struct AnimationSystemClient : ISystem, IClientSystem
    {
        public void OnCreate(ref SystemContext ctx) { }

        public void OnUpdate(ref SystemContext ctx)
        {
            foreach (var (e, vel, animRef) in 
                EntityManager.Query<Velocity, AnimatorRef>().ForEach())
            {
                var speed = vel.Value.magnitude;
                animRef.Value.SetFloat("Speed", speed);
            }
        }

        public void OnDestroy(ref SystemContext ctx) { }
    }
}
```

---

## 🚀 Quick Start

### 1. **Inicializar ECS no Bootstrap**

```csharp
// ServerBootstrap.cs
using MiniECS;
using UnityEngine;

public class ServerBootstrap : MonoBehaviour
{
    private void Start()
    {
        // Registrar todos os systems
        EntityManager.RegisterSystem<PlayerMovementSystem>();
        EntityManager.RegisterSystem<ServerPositionSyncSystem>();
        EntityManager.RegisterSystem<WanderSystemServer>();
        
        // Inicializar
        EntityManager.Initialize();
    }

    private void Update()
    {
        EntityManager.Update();
    }

    private void OnDestroy()
    {
        EntityManager.Cleanup();
    }
}

// ClientBootstrap.cs
using MiniECS;
using UnityEngine;

public class ClientBootstrap : MonoBehaviour
{
    private void Start()
    {
        // Registrar systems do cliente
        EntityManager.RegisterSystem<PositionSystemClient>();
        EntityManager.RegisterSystem<AnimationSystemClient>();
        
        // Inicializar
        EntityManager.Initialize();
    }

    private void Update()
    {
        EntityManager.Update();
    }

    private void OnDestroy()
    {
        EntityManager.Cleanup();
    }
}
```

### 2. **Criar Entidades com Authoring**

```csharp
// PlayerAuthoring.cs
using MiniECS;
using Game.Components;
using UnityEngine;

namespace Game.Authoring
{
    public class PlayerAuthoring : MonoBehaviour, IEntityAuthoring
    {
        [SerializeField] private float speed = 5f;

        public void Bake(ref Entity entity)
        {
            EntityManager.Add(entity, new Position 
            { 
                Value = transform.position 
            });
            
            EntityManager.Add(entity, new Velocity());
            
            EntityManager.Add(entity, new TransformRef 
            { 
                Value = transform 
            });
        }
    }
}
```

### 3. **Usar em Prefabs**

Adicione o `PlayerAuthoring` ao seu prefab. Quando instanciado:

```csharp
var prefab = Resources.Load<GameObject>("Player");
var instance = Instantiate(prefab);

// A entidade é criada automaticamente via IEntityAuthoring
if (instance.TryGetComponent<PlayerAuthoring>(out var authoring))
{
    var entity = EntityManager.CreateEntity();
    authoring.Bake(ref entity);
}
```

---

## 📊 Comparação: MiniECS vs Unity ECS+DOTS vs MonoBehaviour

| Característica | MonoBehaviour | Unity ECS+DOTS | MiniECS |
|---|---|---|---|
| **Curva de Aprendizado** | Fácil | Muito difícil | Fácil |
| **Setup Inicial** | Simples | Complexo | Simples |
| **Performance** | Tradicional | Otimizada | Otimizada |
| **Tamanho do Framework** | N/A | Pesado (~500MB) | Leve e direto |
| **Multiplayer** | Manual com RPC | Complexo | Integrado com Netcode |
| **Documentação** | Vasta | Mínima | Prática |
| **Escalabilidade** | Limitada | Muito alta | Alta |
| **Curva de Complexidade** | Baixa no início | Exponencial | Previsível |

---

## 🎮 Exemplo Completo: Jogo com Múltiplos Jogadores

```csharp
// ============ COMPONENTS ============

// Position.cs
public struct Position
{
    public Vector2 Value;
}

// Velocity.cs
public struct Velocity
{
    public Vector2 Value;
}

// Health.cs
public struct Health
{
    public int HP;
    public int MaxHP;
}

// InputData.cs (Servidor)
public struct InputData
{
    public Vector2 Move;
    public bool Attack;
}

// ============ SYSTEMS ============

// MovementSystem.cs (Servidor)
public struct MovementSystem : ISystem, IServerSystem
{
    public void OnCreate(ref SystemContext ctx) { }

    public void OnUpdate(ref SystemContext ctx)
    {
        foreach (var (e, input, vel, pos) in 
            EntityManager.Query<InputData, ValueRW<Velocity>, ValueRW<Position>>()
                .ForEach())
        {
            vel.Value.Value = input.Move * 5f;
            pos.Value.Value += vel.Value * ctx.DeltaTime;
        }
    }

    public void OnDestroy(ref SystemContext ctx) { }
}

// CombatSystem.cs (Servidor)
public struct CombatSystem : ISystem, IServerSystem
{
    public void OnCreate(ref SystemContext ctx) { }

    public void OnUpdate(ref SystemContext ctx)
    {
        foreach (var (e, input, health) in 
            EntityManager.Query<InputData, ValueRW<Health>>()
                .ForEach())
        {
            if (input.Attack)
            {
                health.Value.HP = Mathf.Max(0, health.Value.HP - 10);
            }
        }
    }

    public void OnDestroy(ref SystemContext ctx) { }
}

// AnimationSystem.cs (Cliente)
public struct AnimationSystem : ISystem, IClientSystem
{
    public void OnCreate(ref SystemContext ctx) { }

    public void OnUpdate(ref SystemContext ctx)
    {
        foreach (var (e, vel, animRef) in 
            EntityManager.Query<Velocity, AnimatorRef>().ForEach())
        {
            animRef.Value.SetFloat("Speed", vel.Value.magnitude);
        }
    }

    public void OnDestroy(ref SystemContext ctx) { }
}

// ============ USAGE ============

// Em um System ou prefab
var entity = EntityManager.CreateEntity();
EntityManager.Add(entity, new Position { Value = Vector2.zero });
EntityManager.Add(entity, new Velocity { Value = Vector2.zero });
EntityManager.Add(entity, new Health { HP = 100, MaxHP = 100 });
EntityManager.Add(entity, new InputData { Move = Vector2.zero });
```

---

## 🔧 Recursos Avançados

### Queries Eficientes

```csharp
// Simples - sem filtro
foreach (var (e, pos) in EntityManager.Query<Position>().ForEach())
{
    // ...
}

// Múltiplos componentes
foreach (var (e, pos, vel, health) in 
    EntityManager.Query<Position, Velocity, Health>().ForEach())
{
    // ...
}

// Com modificação (ValueRW)
foreach (var (e, pos, vel) in 
    EntityManager.Query<ValueRW<Position>, Velocity>().ForEach())
{
    pos.Value.Value += vel.Value;
}

// Sem entidade
foreach (var (pos, vel) in 
    EntityManager.Query<Position, Velocity>().ForEach())
{
    // Acesso rápido aos dados
}
```

### Pool Direto para Performance Ultra-Alta

```csharp
// Para loops muito quentes, acesso direto ao pool
var posPool = EntityManager.Pool<Position>();
var velPool = EntityManager.Pool<Velocity>();

for (int i = 0; i < posPool.Count; i++)
{
    var pos = posPool.Get(i);
    var vel = velPool.Get(i);
    posPool.Get(i).Value += vel.Value * deltaTime;
}
```

---

## 📝 Estrutura de Pastas Recomendada

```
Assets/
├── MiniECS/
│   ├── Core/                    # Motor ECS (EntityManager, etc)
│   ├── Bootstrap/               # Scripts de inicialização
│   └── SystemApi/               # Interfaces e base classes
│
├── Game/
│   ├── Components/              # Structs de dados
│   │   ├── Position.cs
│   │   ├── Velocity.cs
│   │   └── Health.cs
│   │
│   ├── Systems/                 # Lógica de jogo
│   │   ├── MovementSystem.cs
│   │   ├── CombatSystem.cs
│   │   └── AnimationSystem.cs
│   │
│   ├── Network/                 # Sincronização
│   │   └── PlayerNetworkAdapter.cs
│   │
│   └── Authoring/               # Configuração de entities
│       ├── PlayerAuthoring.cs
│       └── EnemyAuthoring.cs
│
└── Resources/                   # Prefabs e assets
    ├── Prefabs/
    └── Materials/
```

---

## ✅ Checklist de Implementação

- [ ] Criar estrutura de pastas
- [ ] Definir todos os componentes necessários
- [ ] Implementar systems do servidor
- [ ] Implementar systems do cliente
- [ ] Criar adaptadores de network (PlayerNetworkAdapter)
- [ ] Configurar bootstraps (ServerBootstrap, ClientBootstrap)
- [ ] Testar sincronização local (Play + Play as Client)
- [ ] Otimizar hot loops com Pool direto se necessário
- [ ] Implementar sistemas avançados (AI, física, etc)
- [ ] Profile com Profiler do Unity para identificar gargalos

---

## 🐛 Troubleshooting

### Problema: Entities não estão sincronizando

**Solução**: Verifique se os NetworkVariable estão com permissões corretas:
```csharp
// ❌ Errado
private NetworkVariable<Vector2> velocity = new();

// ✅ Correto
private NetworkVariable<Vector2> velocity = new(
    default,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);
```

### Problema: Performance ruim com muitas entities

**Solução**: Use Pool direto para loops quentes:
```csharp
// ❌ Lento
foreach (var (e, pos, vel) in EntityManager.Query<Position, Velocity>())
{
    // ...
}

// ✅ Rápido
var posPool = EntityManager.Pool<Position>();
var velPool = EntityManager.Pool<Velocity>();
for (int i = 0; i < posPool.Count; i++)
{
    // ...
}
```

### Problema: Componentes não encontrados em queries

**Solução**: Certifique-se de que foi adicionado na entidade:
```csharp
// Sempre verificar antes de usar
if (EntityManager.Has<MyComponent>(entity.Id))
{
    var comp = EntityManager.Pool<MyComponent>().Get(entity.Id);
}
```

---

## 🚀 Próximos Passos

1. Clone/explore o repositório
2. Execute o exemplo de multiplayer no `Assets/MiniECS/Game/`
3. Adapte para seu próprio jogo
4. Compartilhe seu feedback!

---

## 📄 Licença

MiniECS é código aberto e livre para uso em projetos comerciais e pessoais.

---

## 🤝 Contribuindo

Encontrou um bug ou tem uma sugestão? Abra uma issue ou pull request!

**Desenvolvido com ❤️ para a comunidade Unity**
