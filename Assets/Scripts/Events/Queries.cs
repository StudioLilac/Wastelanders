using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public interface IQuery<TResult> { }
public static class QueryExtensions
{
    private static readonly Dictionary<Type, MethodInfo> _methodCache = new();
    private static readonly MethodInfo _baseQueryMethod;

    static QueryExtensions()
    {
        _baseQueryMethod = typeof(QueryBus).GetMethods()
            .First(m => m.Name == nameof(QueryBus.Query) && m.GetGenericArguments().Length == 2);
    }

    // This could have been clean but WebGL doesn't support the dynamic keyword:(.
    public static TResult Query<TResult>(this IQuery<TResult> query)
    {
        Type concreteType = query.GetType();
        if (!_methodCache.TryGetValue(concreteType, out MethodInfo methodToCall))
        {
            methodToCall = _baseQueryMethod.MakeGenericMethod(concreteType, typeof(TResult));
            _methodCache[concreteType] = methodToCall;
        }

        return (TResult)methodToCall.Invoke(null, new object[] { query });
    }

    public static void Answer<TQuery, TResult>(this MonoBehaviour caller, Func<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
        if (!caller.TryGetComponent(out QueryLifecycleHost host))
        {
            host = caller.gameObject.AddComponent<QueryLifecycleHost>();
        }

        host.AddRegistration(handler);
    }

    public static void UnAnswer<TQuery, TResult>(this MonoBehaviour caller, Func<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
        if (caller.TryGetComponent(out QueryLifecycleHost host))
        {
            host.RemoveRegistration(handler);
        }
    }
}