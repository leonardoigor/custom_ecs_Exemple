
using UnityEngine;

public class ClientBootstrap : MonoBehaviour {
    SystemScheduler scheduler;
    void Awake() {
        SystemRegistry.RegisterAll();
        scheduler = new SystemScheduler(false, true);
    }
    void Update() {
        scheduler.Update(Time.deltaTime);
    }
}
