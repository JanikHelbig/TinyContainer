using System;
using System.Runtime.Serialization;

namespace Jnk.TinyContainer
{
    [Serializable]
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException()
        {
        }

        public TypeNotFoundException(string message) : base(message)
        {
        }

        public TypeNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TypeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}