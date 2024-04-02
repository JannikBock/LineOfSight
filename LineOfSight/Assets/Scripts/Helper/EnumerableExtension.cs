using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class EnumerableExtension
{
    public static T MinOrDefault<T>(this IEnumerable<T> sequence)
    {
        if (sequence.Any())
        {
            return sequence.Min();
        }
        else
        {
            return default(T);
        }
    }

    public static T MaxOrDefault<T>(this IEnumerable<T> sequence)
    {
        if (sequence.Any())
        {
            return sequence.Max();
        }
        else
        {
            return default(T);
        }
    }
}

