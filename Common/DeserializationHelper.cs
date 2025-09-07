#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ChroniclerCherry.Common;

/// <summary>Provides common methods for handling deserialized models.</summary>
internal static class DeserializationHelper
{
    /// <summary>Convert an unvalidated array into a non-null array containing non-null values.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="values">The values to convert.</param>
    public static T[] ToNonNullable<T>(T?[]? values)
        where T : class
    {
        if (values is null)
            return [];

        if (values.Contains(null))
            return values.Where(p => p is not null).ToArray()!;

        return values!;
    }


    /// <summary>Convert an unvalidated array into a non-null array containing non-null values.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="values">The values to convert.</param>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = SuppressReasons.ValidatesNullability)]
    public static List<T> ToNonNullable<T>(List<T>? values)
        where T : class
    {
        if (values is null)
            return [];

        if (values.Contains(null!))
            return values.Where(p => p is not null).ToList();

        return values;
    }
}
