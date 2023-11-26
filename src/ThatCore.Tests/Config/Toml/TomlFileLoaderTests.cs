using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThatCore.Config.Toml;

[TestClass]
public class TomlFileLoaderTests
{
    [TestMethod]
    public void CanLoadSchema()
    {
        Test<FakeModel2, string>(x => x.Prop1.Prop);
    }

    private void Test<T, K>(Expression<Func<T, K>> propertySelector)
    {
        var memberExpression = propertySelector.Body as MemberExpression;

        var member = memberExpression.Member;

        var prop = member as PropertyInfo;

        Debug.Write(prop.GetValue(new FakeModel()));

        propertySelector.Compile();
    }

    private class FakeModel
    {
        public string Prop { get; set; } = "Test";
    }

    private class FakeModel2
    {
        public FakeModel Prop1 { get; set; } = new();
    }
}
