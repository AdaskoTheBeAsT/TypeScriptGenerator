using System;

namespace TypeScriptGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public class IncludeAttribute
    : Attribute
    {
        public IncludeAttribute()
        {
        }

        public IncludeAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public string? Name { get; }
    }
}
