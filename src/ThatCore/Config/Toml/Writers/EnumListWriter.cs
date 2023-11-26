﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ThatCore.Config.Toml.Writers;

internal class EnumListWriter<T> : ValueWriter<List<T>>
    where T : struct, Enum
{
    protected override string WriteInternal(ITomlSetting<List<T>> entry)
    {
        if (entry.IsSet && entry.Value is not null)
        {
            return entry.Value.Select(x => x.ToString()).Join();
        }
        else
        {
            return string.Empty;
        }
    }
}
