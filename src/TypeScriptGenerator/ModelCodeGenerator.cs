using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
#pragma warning disable 162

namespace TypeScriptGenerator
{
    public class ModelCodeGenerator
    {
        public void Generate(string targetPath, Compilation compilation, List<NamedTypeSymbolData> classSymbols)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentNullException(nameof(targetPath));
            }

            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            if (classSymbols is null)
            {
                throw new ArgumentNullException(nameof(classSymbols));
            }

            var controllerBaseSymbol = compilation.GetTypeByMetadataName(CodeGenerator.ControllerBaseTypeName);
            if (controllerBaseSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var controllerSymbol = compilation!.GetTypeByMetadataName(CodeGenerator.ControllerTypeName);
            if (controllerSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            foreach (var namedTypeSymbolData in classSymbols)
            {
                if (SkipClass(
                    namedTypeSymbolData,
                    controllerBaseSymbol,
                    controllerSymbol))
                {
                    continue;
                }

                var targetFile = GetTargetFile(
                    targetPath,
                    namedTypeSymbolData.Name);
                var sb = new StringBuilder();
                GenerateHeader(sb);
                var publicProperties = GetProperties(namedTypeSymbolData);
                GenerateImports(sb, publicProperties);
                GenerateClass(sb, namedTypeSymbolData, publicProperties);

#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
                File.WriteAllText(targetFile, sb.ToString(), CodeGenerator.Uft8WithoutBomEncoding);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            }
        }

        internal bool SkipClass(NamedTypeSymbolData namedTypeSymbolData, INamedTypeSymbol controllerBaseSymbol, INamedTypeSymbol controllerSymbol)
        {
            var baseType = namedTypeSymbolData.NamedTypeSymbol.BaseType;
            if (baseType != null && (baseType.Equals(controllerBaseSymbol, SymbolEqualityComparer.Default)
                || baseType.Equals(controllerSymbol, SymbolEqualityComparer.Default)))
            {
                return true;
            }

            return false;
        }

        internal void GenerateHeader(StringBuilder sb)
        {
            sb.AppendLine(CodeGenerator.Header).AppendLine();
        }

        internal IEnumerable<IPropertySymbol>? GetProperties(NamedTypeSymbolData namedTypeSymbolData)
        {
            return namedTypeSymbolData.NamedTypeSymbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public && m.Kind == SymbolKind.Property && m is IPropertySymbol)
                .Cast<IPropertySymbol>();
        }

        internal void GenerateImports(StringBuilder sb, IEnumerable<IPropertySymbol>? publicProperties)
        {
            if (publicProperties is null)
            {
                return;
            }

            var atLeastOneImportGenerated = false;
            foreach (var publicProperty in publicProperties)
            {
                var propertyType = publicProperty.Type;
                if (propertyType is null)
                {
                    continue;
                }

                if (propertyType.TypeKind == TypeKind.Class
                    && propertyType.SpecialType == SpecialType.None)
                {
                    sb.AppendLine($"import {{ {propertyType.Name} }} from './{propertyType.Name}';");
                    atLeastOneImportGenerated = true;
                }
                else if (propertyType.TypeKind == TypeKind.Enum)
                {
                    sb.AppendLine($"import {{ {propertyType.Name} }} from 'src/api/enums/{propertyType.Name}';");
                    atLeastOneImportGenerated = true;
                }

                if (atLeastOneImportGenerated)
                {
                    sb.AppendLine();
                }
            }
        }

        internal void GenerateClass(StringBuilder sb, NamedTypeSymbolData namedTypeSymbolData, IEnumerable<IPropertySymbol>? publicProperties)
        {
            sb.AppendLine($"export class {namedTypeSymbolData.Name} {{");
            if (publicProperties != null)
            {
                foreach (var publicProperty in publicProperties)
                {
                    GenerateProperty(sb, publicProperty);
                }
            }

            sb.AppendLine("}");
        }

        internal void GenerateProperty(StringBuilder sb, IPropertySymbol publicProperty)
        {
            var translatedType = TranslateTypeToTypescript(publicProperty.Type);
            sb.AppendLine($"  public {publicProperty.Name.ToCamelCase()}: {translatedType};");
        }

        internal string TranslateTypeToTypescript(ITypeSymbol publicPropertyType)
        {
            return publicPropertyType.SpecialType switch
            {
                SpecialType.System_String => "string",
                SpecialType.System_Char => "string",
                SpecialType.System_Boolean => "boolean",
                SpecialType.System_Object => "object",
                SpecialType.System_DateTime => "Date",
                SpecialType.System_Decimal => "number",
                SpecialType.System_Double => "number",
                SpecialType.System_Int16 => "number",
                SpecialType.System_Int32 => "number",
                SpecialType.System_Int64 => "number",
                SpecialType.System_Single => "number",
                SpecialType.System_Byte => "number",
                SpecialType.System_SByte => "number",
                SpecialType.System_UInt16 => "number",
                SpecialType.System_UInt32 => "number",
                SpecialType.System_UInt64 => "number",
                _ => "any",
            };
        }

        internal string GetTargetFile(string targetPath, string typeName)
        {
            return $"{targetPath}/{CodeGenerator.ModelsPath}/{typeName}.ts";
        }
    }
}