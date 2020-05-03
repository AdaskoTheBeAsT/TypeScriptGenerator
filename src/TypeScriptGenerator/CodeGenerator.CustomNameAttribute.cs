namespace TypeScriptGenerator
{
    public partial class CodeGenerator
    {
        private const string CustomNameAttributeName = "CustomNameAttribute";
        private static readonly string CustomNameAttributeFullName = $"{CustomNamespace}.{CustomNameAttributeName}";
        private static readonly string CustomNameAttributeText =
            $@"using System;

namespace {CustomNamespace}
{{
    [AttributeUsage(AttributeTargets.Method)]
    public class {CustomNameAttributeName}
       : Attribute
    {{
        public {CustomNameAttributeName}(string? name)
        {{
            Name = name;
        }}

        public string? Name {{ get; }}
    }}
}}";
    }
}