using System;

namespace ThatCore.Config.Toml.Parsers;

internal interface IValueParser
{
    void Parse(ITomlSetting entry, TomlLine line);
}

internal abstract class ValueParser<T> : IValueParser
{
    protected Type ParserType { get; }
    private Type ConfigEntryParserType { get; } 

    protected ValueParser()
    {
        ParserType = typeof(T);

        ConfigEntryParserType = typeof(ITomlSetting<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(ParserType);
    }

    public void Parse(ITomlSetting entry, TomlLine line)
    {
        if (entry is ITomlSetting<T> supportedEntry)
        {
            if (string.IsNullOrWhiteSpace(line.Value))
            {
                supportedEntry.IsSet = true;
            }
            else
            {
                ParseInternal(supportedEntry, line);
            }
        }
        /*
        else if (Nullable.GetUnderlyingType(entry.SettingType) == ParserType)
        {
            if (string.IsNullOrWhiteSpace(line.Value))
            {
                entry.IsSet = true;
            }
            else
            {
                var convertedEntry = (ITomlConfigEntry<T>)Convert.ChangeType(entry, ConfigEntryParserType);

                ParseInternal(convertedEntry, line);
            }
        }
        */
        else
        {
            throw new InvalidOperationException($"Unable to parse config entry with type {typeof(T).Name} using parser {GetType().Name}.");
        }
    }

    protected abstract void ParseInternal(ITomlSetting<T> entry, TomlLine line);
}
