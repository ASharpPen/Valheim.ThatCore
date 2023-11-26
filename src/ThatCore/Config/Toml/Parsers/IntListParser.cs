﻿using System.Collections.Generic;
using System.Linq;
using ThatCore.Extensions;
using ThatCore.Logging;

namespace ThatCore.Config.Toml.Parsers;

internal class IntListParser : ValueParser<List<int>>
{
    protected override void ParseInternal(ITomlSetting<List<int>> entry, TomlLine line)
    {
        var values = line.Value.SplitByComma();

        var results = values.Select<string, int?>(
            x =>
            {
                if (int.TryParse(x, out var result))
                {
                    return result;
                }
                else
                {
                    Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{x}'. Expected integer number. Eg., 0, 1 or 3.");
                    return null;
                }
            })
            .Where(x => x is not null)
            .Select(x => x.Value)
            .ToList();

        if (values.Count > 0 &&
            results.Count == 0)
        {
            // Something went wrong and all entries failed parsing.
            entry.IsSet = false;
            return;
        }

        entry.Value = results;
        entry.IsSet = true;
    }
}
