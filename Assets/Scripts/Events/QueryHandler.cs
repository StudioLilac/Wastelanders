using System;
using System.Collections.Generic;
using UnityEngine;

public class QueryLifecycleHost : MonoBehaviour
{
    private readonly Dictionary<Delegate, IRegistration> activeRegistrations = new();

    private void OnEnable()
    {
        foreach (var reg in activeRegistrations.Values) reg.Enable();
    }

    private void OnDisable()
    {
        foreach (var reg in activeRegistrations.Values) reg.Disable();
    }

    public void AddRegistration<TQuery, TResult>(Func<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
        if (activeRegistrations.ContainsKey(handler)) return;

        QueryRegistration<TQuery, TResult> registration = new(handler);

        activeRegistrations.Add(handler, registration);

        if (enabled && gameObject.activeInHierarchy)
            registration.Enable();
    }

    public void RemoveRegistration<TQuery, TResult>(Func<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
        if (activeRegistrations.TryGetValue(handler, out var registration))
        {
            if (enabled && gameObject.activeInHierarchy)
                registration.Disable();

            activeRegistrations.Remove(handler);
        }
    }

    private void OnDestroy()
    {
        foreach (var reg in activeRegistrations.Values) reg.Disable();
        activeRegistrations.Clear();
    }

    private interface IRegistration
    {
        void Enable();
        void Disable();
    }

    private record QueryRegistration<TQuery, TResult>(Func<TQuery, TResult> Handler) : IRegistration
        where TQuery : IQuery<TResult>
    {
        public void Enable() => QueryBus.Register(Handler);
        public void Disable() => QueryBus.Unregister(Handler);
    }
}