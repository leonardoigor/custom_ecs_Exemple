
using UnityEngine;

public class ServerBootstrap : MonoBehaviour {
    SystemScheduler scheduler;
    void Awake() {
        SystemRegistry.RegisterAll();
        scheduler = new SystemScheduler(true, false);
    }
    void FixedUpdate() {
        scheduler.Update(Time.fixedDeltaTime);
    }
}
