﻿namespace TypeScriptGenerator
{
    public partial class CodeGenerator
    {
        public static readonly string EnumAsStringAttributeFullName = $"{CustomNamespace}.{EnumAsStringAttributeName}";
        private const string EnumAsStringAttributeName = "EnumAsStringAttribute";
        private static readonly string EnumAsStringAttributeText =
            $@"using System;

namespace {CustomNamespace}
{{
    [AttributeUsage(AttributeTargets.Enum)]
    public class {EnumAsStringAttributeName}
       : Attribute
    {{
        public {EnumAsStringAttributeName}()
        {{
        }}
    }}
}}";
    }
}