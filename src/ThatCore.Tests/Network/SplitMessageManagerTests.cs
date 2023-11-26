using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThatCore.Network;

[TestClass]
public class SplitMessageManagerTests
{
    [TestMethod]
    public void CanPackage()
    {
        var message = new FakeMessage();

        Action act = () => SplitMessageManager.Pack(message);

        act.Should().NotThrow();
    }

    [TestMethod]
    public void CanSplitMessage()
    {
        var message = new FakeMessage();
        message.Text = File.ReadAllLines(Resources.Files.LoremIpsum).ToList();

        SplitMessageManager.MaxPackageSize = 1000;

        var splits = SplitMessageManager.Pack(message);

        splits.Should().HaveCountGreaterThan(1);
    }

    [TestMethod]
    public void CombineAndUnpackSplitsCanMergeMessages()
    {
        SerializerManager.GetSerializer<FakeMessage>();

        var message = new FakeMessage();
        message.SomeProp = 6;

        SplitMessageManager.MaxPackageSize = 10;

        var splits = SplitMessageManager.Pack(message);

        int index = 0;
        var splitDictionary = splits
            .ToDictionary(_ => index++);

        var combined = SplitMessageManager.CombineAndUnpackSplits(typeof(FakeMessage), splitDictionary);

        combined.Should().BeOfType<FakeMessage>();
        combined.As<FakeMessage>().SomeProp.Should().Be(6);
    }

    [TestMethod]
    public void CombineAndUnpackSplitsGenericCanMergeMessages()
    {
        SerializerManager.GetSerializer<FakeMessage>();

        var message = new FakeMessage();
        message.SomeProp = 6;

        SplitMessageManager.MaxPackageSize = 10;

        var splits = SplitMessageManager.Pack(message);

        int index = 0;
        var splitDictionary = splits
            .ToDictionary(_ => index++);

        var combined = SplitMessageManager.CombineAndUnpackSplits<FakeMessage>(splitDictionary);

        combined.SomeProp.Should().Be(6);
    }


    private class FakeMessage : IMessage
    {
        private readonly Action<FakeMessage> _callback;

        public FakeMessage()
        {
        }

        public FakeMessage(Action<FakeMessage> callback = null)
        {
            _callback = callback;
        }

        public int SomeProp { get; set; } = 5;

        public List<string> Text { get; set; } = new() { "Lorem ipsum", "dolor sit amet" };

        public void AfterUnpack()
        {
            _callback?.Invoke(this);
        }

        public void Initialize()
        {
        }
    }
}
