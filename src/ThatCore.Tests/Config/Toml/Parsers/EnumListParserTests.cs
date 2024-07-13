using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThatCore.Logging;

namespace ThatCore.Config.Toml.Parsers;

[TestClass]
public class EnumListParserTests
{
    [DataTestMethod]
    [DataRow("One", 1)]
    [DataRow("One,", 1)]
    [DataRow("One, ", 1)]
    [DataRow("One,\u00A0", 1)]
    [DataRow(",One", 1)]
    [DataRow("One, Two", 2)]
    public void TrimsEmptySplits(string input, int expectedCount)
    {
        // Arrange
        var logger = new TestLogger();
        Log.SetLogger(logger);

        var parser = new EnumListParser<TestEnum>();

        var tomlEntry = new TomlSetting<List<TestEnum>>("Test", default);
        var tomlLine = new TomlLine()
        {
            Value = input,
        };

        // Act
        parser.Parse(tomlEntry, tomlLine);

        // Assert
        tomlEntry.Value.Count.Should().Be(expectedCount);

        logger.Logs
            .Where(x => x.LogLevel < LogLevel.Info)
            .Should()
            .BeEmpty();
    }

    private enum TestEnum
    {
        One,
        Two,
        Three
    }

}
