using System;

namespace TypeScriptGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomNameAttribute
        : Attribute
    {
        public CustomNameAttribute(string? name)
        {
            Name = name;
        }

        public string? Name { get; }
    }
}
