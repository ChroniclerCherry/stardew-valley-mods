using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace StardewAquarium.src;
internal static class Extensions
{

    /// <summary>
    /// Faster replacement for str.Split()[index];.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="deliminator">deliminator to use.</param>
    /// <param name="index">index of the chunk to get.</param>
    /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ReadOnlySpan<char> GetNthChunk(this string str, char deliminator, int index = 0)
    {
        if (index < 0 || index > str.Length + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        int start = 0;
        int ind = 0;
        while (index-- >= 0)
        {
            ind = str.IndexOf(deliminator, start);
            if (ind == -1)
            {
                // since we've previously decremented index, check against -1;
                // this means we're done.
                if (index == -1)
                {
                    return str.AsSpan()[start..];
                }

                // else, we've run out of entries
                // and return an empty span to mark as failure.
                return [];
            }

            if (index > -1)
            {
                start = ind + 1;
            }
        }
        return str.AsSpan()[start..ind];
    }

    /// <summary>
    /// Gets the index of the next whitespace character.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetIndexOfWhiteSpace(this string str)
    {
        return str.AsSpan().GetIndexOfWhiteSpace();
    }

    /// <summary>
    /// Gets the index of the next whitespace character.
    /// </summary>
    /// <param name="chars">ReadOnlySpan to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetIndexOfWhiteSpace(this ReadOnlySpan<char> chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsWhiteSpace(chars[i]))
            {
                return i;
            }
        }
        return -1;
    }
}
