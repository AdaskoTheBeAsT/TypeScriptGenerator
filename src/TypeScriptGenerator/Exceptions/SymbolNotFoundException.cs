using System;
using System.Runtime.Serialization;

namespace TypeScriptGenerator.Exceptions
{
    [Serializable]
    public class SymbolNotFoundException
        : Exception
    {
        public SymbolNotFoundException()
        {
        }

        public SymbolNotFoundException(string message)
            : base(message)
        {
        }

        public SymbolNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SymbolNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
