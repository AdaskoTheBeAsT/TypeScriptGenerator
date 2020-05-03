using System;
using System.Runtime.Serialization;

namespace TypeScriptGenerator
{
    [Serializable]
    public class StringEnumsCantBeUsedWithFlagsAttributeException
        : Exception
    {
        public StringEnumsCantBeUsedWithFlagsAttributeException()
        {
        }

        public StringEnumsCantBeUsedWithFlagsAttributeException(string message)
            : base(message)
        {
        }

        public StringEnumsCantBeUsedWithFlagsAttributeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected StringEnumsCantBeUsedWithFlagsAttributeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
