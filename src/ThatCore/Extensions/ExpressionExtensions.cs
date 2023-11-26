using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ThatCore.Extensions;

public static class ExpressionExtensions
{
    public static PropertyInfo GetPropertyInfo<T, K>(this Expression<Func<T, K>> propertySelector)
    {
        var memberExpression = propertySelector.Body as MemberExpression;

        var property = memberExpression?.Member as PropertyInfo;

        return property;
    }
}
