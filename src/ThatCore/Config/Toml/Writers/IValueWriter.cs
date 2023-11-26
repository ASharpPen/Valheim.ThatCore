using System;

namespace ThatCore.Config.Toml.Writers;

internal interface IValueWriter
{
    string Write(ITomlSetting entry);
}

internal abstract class ValueWriter<T> : IValueWriter
{
    protected Type WriterType { get; }

    protected ValueWriter()
    {
        WriterType = typeof(T);
    }

    public string Write(ITomlSetting entry)
    {
        if (entry is ITomlSetting<T> supportedEntry)
        {
            return WriteInternal(supportedEntry);
        }
        else
        {
            throw new InvalidOperationException($"Unable to write config entry with type {typeof(T).Name} using writer {GetType().Name}.");
        }
    }

    protected abstract string WriteInternal(ITomlSetting<T> entry);
}
