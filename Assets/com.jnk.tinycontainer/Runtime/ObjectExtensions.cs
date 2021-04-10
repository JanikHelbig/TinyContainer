using UnityEngine;

namespace Jnk.TinyContainer
{
    internal static class ObjectExtensions
    {
        public static T IsNull<T>(this T obj) where T : Object
        {
            return obj == null ? null : obj;
        }
    }
}
