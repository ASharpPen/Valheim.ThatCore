using System;
using ThatCore.Logging;

namespace ThatCore.Extensions;

public static class EventExtensions
{
    public static void Raise<T>(this Action<T> events, T arg, string messageOnError = null)
    {
        if (events is null)
        {
            return;
        }

        foreach (Action<T> invocation in events.GetInvocationList())
        {
            try
            {
                invocation(arg);
            }
            catch (Exception e)
            {
                var target = $"{invocation.Method.DeclaringType.Assembly.GetName().Name}.{invocation.Method.DeclaringType.Name}.{invocation.Method.Name}";
                Log.Error?.Log($"[{target}]: {messageOnError ?? string.Empty}", e);
            }
        }
    }

    public static void Raise(this Action events, string messageOnError = null)
    {
        if (events is null)
        {
            return;
        }

        foreach (Action invocation in events.GetInvocationList())
        {
            try
            {
                invocation();
            }
            catch (Exception e)
            {
                var target = $"{invocation.Method.DeclaringType.Assembly.GetName().Name}.{invocation.Method.DeclaringType.Name}.{invocation.Method.Name}";
                Log.Error?.Log($"[{target}]: {messageOnError ?? string.Empty}", e);
            }
        }
    }
}
