using System.Text;

namespace MyCleanTemplate.Domain.Common.Logs;

public static class LogHelper
{
    public static string GetFullName(Type t)
    {
        if (!t.IsGenericType)
        {
            return t.Name;
        }

        var sb = new StringBuilder();

        sb.Append(t.Name[..t.Name.LastIndexOf('`')]);
        sb.Append(t.GetGenericArguments().Aggregate("<",
            (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type)));
        sb.Append(value: '>');

        return sb.ToString();
    }
}

