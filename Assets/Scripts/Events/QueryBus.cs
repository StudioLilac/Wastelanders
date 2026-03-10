using System;
using UnityEngine;

#nullable enable
public static class QueryBus
{
    private static class Endpoint<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        public static Func<TQuery, TResult>? Handler;
    }

    public static void Register<TQuery, TResult>(Func<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
#if UNITY_EDITOR
        if (Endpoint<TQuery, TResult>.Handler != null)
        {
            Debug.LogWarning($"[QueryBus] Handler for {typeof(TQuery).Name} is being overwritten.");
        }
#endif
        Endpoint<TQuery, TResult>.Handler = handler;
    }

    public static void Unregister<TQuery, TResult>(Func<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
        if (Endpoint<TQuery, TResult>.Handler == handler)
        {
            Endpoint<TQuery, TResult>.Handler = null;
        }
    }

    public static TResult? Query<TQuery, TResult>(TQuery query)
        where TQuery : IQuery<TResult>
    {
        var handler = Endpoint<TQuery, TResult>.Handler;

        if (handler == null)
        {
            Debug.LogWarning("No handler found for query: " + typeof(TQuery));
            return default;
        }

        return handler(query);
    }
}