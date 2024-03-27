using System;

namespace Morpeh.SourceGeneration.Common;

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1),
        };
    }
    
    public static string FirstCharToLower(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToLower() + input.Substring(1),
        };
    }

    public static string UpperFirstCharOrAddUnderline(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0] == '_' && input.Length > 1 && !char.IsUpper(input[1])
                ? UpperFirstCharOrAddUnderline(input.Substring(1))
                : char.IsUpper(input[0])
                    ? "_" + input
                    : FirstCharToUpper(input),
        };
    }
    
    public static string AddAttributePostfix(this string value) => value + "Attribute";
}