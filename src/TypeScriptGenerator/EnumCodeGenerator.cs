using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TypeScriptGenerator
{
    public class EnumCodeGenerator
    {
        public void Generate(
            string targetPath,
            Compilation compilation,
            List<NamedTypeSymbolData> enumSymbols)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            if (enumSymbols is null)
            {
                throw new ArgumentNullException(nameof(enumSymbols));
            }

            var enumAsStringAttributeSymbol = compilation!.GetTypeByMetadataName(CodeGenerator.EnumAsStringAttributeFullName);
            if (enumAsStringAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var enumLabelAttributeSymbol = compilation!.GetTypeByMetadataName(CodeGenerator.EnumLabelAttributeFullName);
            if (enumLabelAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var flagsAttributeSymbol = compilation!.GetTypeByMetadataName(CodeGenerator.FlagsAttributeFullName);
            if (flagsAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            foreach (var namedTypeSymbolData in enumSymbols)
            {
                if (namedTypeSymbolData.NamedTypeSymbol.GetAttributes().Any(
                    ad =>
                        ad.AttributeClass?.Equals(enumAsStringAttributeSymbol, SymbolEqualityComparer.Default) == true))
                {
                    GenerateStringEnum(
                       namedTypeSymbolData,
                       flagsAttributeSymbol,
                       enumLabelAttributeSymbol,
                       targetPath);
                }
                else
                {
                    GenerateNormalEnum(
                        namedTypeSymbolData,
                        flagsAttributeSymbol,
                        enumLabelAttributeSymbol,
                        targetPath);
                }
            }
        }

        internal void GenerateNormalEnum(
            NamedTypeSymbolData namedTypeSymbolData,
            INamedTypeSymbol flagsAttributeSymbol,
            INamedTypeSymbol enumLabelAttributeSymbol,
            string targetPath)
        {
            var typeName = namedTypeSymbolData.Name;
            var targetFile = GetTargetFile(targetPath, typeName);
            var fields = GetFieldSymbols(namedTypeSymbolData);

            var sb = new StringBuilder();
            sb.AppendLine(CodeGenerator.Header).AppendLine();
            sb.AppendLine($"export enum {typeName} {{");
            foreach (var fieldSymbol in fields)
            {
                sb.AppendLine($"  {fieldSymbol.Name} = {fieldSymbol.ConstantValue?.ToString()},");
            }

            sb.AppendLine("}").AppendLine();

            sb.AppendLine($"export namespace {typeName} {{").AppendLine();

            GenerateGetLabel(
                sb,
                enumLabelAttributeSymbol,
                typeName,
                fields);

            sb.AppendLine(
                $@"  export function getKeys(): string[] {{
    const keys: string[] = [];
    for (let enumMember in {typeName}) {{
      if(!{typeName}.hasOwnProperty(enumMember)) {{
        continue;
      }}
      if(isValidMember(enumMember)) {{
        keys.push({typeName}[enumMember]);
      }}
    }}
    return keys;  
  }}").AppendLine();

            sb.AppendLine(
                $@"  export function getValues(): {typeName}[] {{
    const values: {typeName}[] = [];
    for (let enumMember in {typeName}) {{
      if(!{typeName}.hasOwnProperty(enumMember)) {{
        continue;
      }}
      if(isValidMember(enumMember)) {{
        values.push(enumMember);
      }}        
    }}
    return values;  
  }}").AppendLine();

            GenerateHasFlag(namedTypeSymbolData, flagsAttributeSymbol, sb);

            sb.AppendLine(
                @"  function isValidMember(value: any): value is number {
    return !isNaN(parseInt(value));
  }");

            sb.AppendLine().AppendLine("}").AppendLine();

#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            File.WriteAllText(targetFile, sb.ToString(), CodeGenerator.Uft8WithoutBomEncoding);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        }

        internal void GenerateStringEnum(
            NamedTypeSymbolData namedTypeSymbolData,
            INamedTypeSymbol flagsAttributeSymbol,
            INamedTypeSymbol enumLabelAttributeSymbol,
            string targetPath)
        {
            var flagsAttributeData = namedTypeSymbolData.NamedTypeSymbol.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.Equals(flagsAttributeSymbol, SymbolEqualityComparer.Default) == true);

            if (flagsAttributeData != null)
            {
                throw new StringEnumsCantBeUsedWithFlagsAttributeException();
            }

            var typeName = namedTypeSymbolData.Name;

            var targetFile = GetTargetFile(targetPath, typeName);
            var fields = GetFieldSymbols(namedTypeSymbolData);

            var sb = new StringBuilder();

            sb.AppendLine(CodeGenerator.Header).AppendLine();
            sb.AppendLine($"export enum {typeName} {{");
            foreach (var fieldSymbol in fields)
            {
                sb.AppendLine($"  {fieldSymbol.Name} = '{fieldSymbol.Name}',");
            }

            sb.AppendLine("}").AppendLine();

            sb.AppendLine($"export namespace {typeName} {{").AppendLine();

            GenerateGetLabel(
                sb,
                enumLabelAttributeSymbol,
                typeName,
                fields);

            sb.AppendLine(
                $@"  export function getKeys(): string[] {{
    const keys: string[] = [];
    for(let enumMember in {typeName}) {{
      if(!{typeName}.hasOwnProperty(enumMember)) {{
        continue;
      }}
      if(!isValidMember(enumMember)){{
        continue;
      }}
      const member = {typeName}[enumMember];
      if(typeof member === 'function'){{
        continue;
      }}
      keys.push(enumMember);
    }} 
    return keys;
  }}").AppendLine();

            sb.AppendLine(
                $@"  export function getValues(): {typeName}[] {{
    const values: {typeName}[] = [];
    for (let enumMember in {typeName}) {{
      if(!{typeName}.hasOwnProperty(enumMember)) {{
        continue;
      }}
      if (!isValidMember(enumMember)) {{
        continue;
      }}
      const member = {typeName}[enumMember];
      if(typeof member === 'function'){{
        continue;
      }}
      values.push(member);
    }}   
    return values;
  }}").AppendLine();

            sb.AppendLine(
                $@"  function isValidMember(value: string): value is keyof typeof {typeName} {{
    return value in {typeName};
  }}");

            sb.AppendLine().AppendLine("}").AppendLine();

#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            File.WriteAllText(targetFile, sb.ToString(), CodeGenerator.Uft8WithoutBomEncoding);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        }

        internal string GetTargetFile(string targetPath, string typeName)
        {
            return $"{targetPath}/{CodeGenerator.EnumsPath}/{typeName}.ts";
        }

        internal List<IFieldSymbol> GetFieldSymbols(NamedTypeSymbolData namedTypeSymbolData)
        {
            var fields = namedTypeSymbolData.NamedTypeSymbol.GetMembers()
                .Where(s => s is IFieldSymbol)
                .Cast<IFieldSymbol>()
                .ToList();
            return fields;
        }

        internal void GenerateGetLabel(
            StringBuilder sb,
            INamedTypeSymbol enumLabelAttributeSymbol,
            string typeName,
            List<IFieldSymbol> fields)
        {
            sb.AppendLine($"  export function getLabel(value: {typeName}): string {{");
            sb.AppendLine("    switch(value) {");
            foreach (var fieldSymbol in fields)
            {
                var attributeClass = fieldSymbol.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.Equals(enumLabelAttributeSymbol, SymbolEqualityComparer.Default) == true);
                sb.AppendLine($"      case {typeName}.{fieldSymbol.Name}: return '{attributeClass?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? fieldSymbol.Name}';");
            }

            sb.AppendLine("      default: throw new Error('Invalid value=`${value}`');");
            sb.AppendLine("    }");
            sb.AppendLine("  }").AppendLine();
        }

        internal void GenerateHasFlag(NamedTypeSymbolData namedTypeSymbolData, INamedTypeSymbol flagsAttributeSymbol, StringBuilder sb)
        {
            if (!namedTypeSymbolData.NamedTypeSymbol.GetAttributes().Any(ad => ad.AttributeClass?.Equals(
                flagsAttributeSymbol,
                SymbolEqualityComparer.Default) == true))
            {
                return;
            }

            var typeName = namedTypeSymbolData.Name;
            sb.AppendLine(
                @$"  export function hasFlag(value: {typeName}, expected: {typeName}) {{
    return (value && expected) === expected;
  }}").AppendLine();
        }
    }
}
