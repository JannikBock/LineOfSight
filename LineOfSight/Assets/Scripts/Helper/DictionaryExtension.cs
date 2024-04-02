using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class DictionaryExtension
{
    //TryGetValue function for a dictionary that returns the default and doesnt throw.
    public static Z Get<T, Z>(this Dictionary<T, Z> obj, T key)
    {
        Z result;
        if (!obj.TryGetValue(key, out result))
        {
            return default;
        }
        return result;
    }


}

