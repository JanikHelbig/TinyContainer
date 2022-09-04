using System;

namespace Jnk.TinyContainer
{
    [Flags]
    public enum EventFunction
    {
        None = 0,
        FixedUpdate = 1,
        Update = 2,
        LateUpdate = 4
    }
}