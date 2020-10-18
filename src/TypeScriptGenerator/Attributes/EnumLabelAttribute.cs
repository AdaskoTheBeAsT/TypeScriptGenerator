using System;

namespace TypeScriptGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumLabelAttribute
        : Attribute
    {
        public EnumLabelAttribute(string? text)
        {
            Text = text;
        }

        public string? Text { get; }
    }
}
