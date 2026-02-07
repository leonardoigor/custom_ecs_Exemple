
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class SystemRegistry {
    static readonly List<SystemWrapper> systems = new();
    public static IReadOnlyList<SystemWrapper> Systems => systems;

    public static void RegisterAll() {
        systems.Clear();
        foreach (var t in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!t.IsValueType) continue;
            if (!typeof(ISystem).IsAssignableFrom(t)) continue;
            var wt = typeof(SystemWrapper<>).MakeGenericType(t);
            systems.Add((SystemWrapper)Activator.CreateInstance(wt));
        }
    }
}
