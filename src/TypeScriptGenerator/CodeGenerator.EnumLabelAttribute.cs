namespace TypeScriptGenerator
{
    public partial class CodeGenerator
    {
        public static readonly string EnumLabelAttributeFullName = $"{CustomNamespace}.{EnumLabelAttributeName}";
        private const string EnumLabelAttributeName = "EnumLabelAttribute";
        private static readonly string EnumLabelAttributeText =
            $@"using System;

namespace {CustomNamespace}
{{
    [AttributeUsage(AttributeTargets.Field)]
    public class {EnumLabelAttributeName}
       : Attribute
    {{
        public {EnumLabelAttributeName}(string? text)
        {{
            Text = text;
        }}

        public string? Text {{ get; }}
    }}
}}";
    }
}