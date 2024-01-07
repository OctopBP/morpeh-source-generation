namespace MorpehAttributes.Common;

public static class StringExt
{
    public static string WithUnderscore(this string value) => "_" + value;
    
    public static string LowerFirstLatter(this string value)
    {
        if(string.IsNullOrEmpty(value))
        {
            return value;
        }
        return char.ToLower(value[0]) + value.Substring(1);
    }
}