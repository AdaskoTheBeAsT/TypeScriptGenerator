namespace TypeScriptGenerator
{
    public partial class CodeGenerator
    {
        private const string IncludeAttributeName = "IncludeAttribute";
        private static readonly string IncludeAttributeFullName = $"{CustomNamespace}.{IncludeAttributeName}";
        private static readonly string IncludeAttributeText =
            $@"using System;

namespace {CustomNamespace}
{{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public class {IncludeAttributeName}
       : Attribute
    {{
        public {IncludeAttributeName}()
        {{
        }}

        public {IncludeAttributeName}(string name)
        {{
            if (string.IsNullOrEmpty(name))
            {{
                throw new ArgumentNullException(nameof(name));
            }}

            Name = name;
        }}

        public string? Name {{ get; }}
    }}
}}";
    }
}