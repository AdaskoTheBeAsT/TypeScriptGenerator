using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
#pragma warning disable 162

namespace TypeScriptGenerator
{
    [Generator]
    public partial class CodeGenerator
        : ISourceGenerator
    {
        public const string Header = "// Generated by TypeScriptGenerator";
        public const string ModelsPath = "models";
        public const string EnumsPath = "enums";
        public const string ServicesPath = "services";
        public static readonly Encoding Uft8WithoutBomEncoding = new UTF8Encoding(false, true);
        private const string CustomNamespace = "TypeScriptGenerator";
        private const string TargetPath = "./ClientApp/src/api";
        private const string FlagsAttributeFullName = "System.FlagsAttribute";

        public void Execute(SourceGeneratorContext context)
        {
            if (!RegisterAttributes(context, out var receiver, out var compilation))
            {
                return;
            }

            return;

            // get the newly bound attributes
            var includeAttributeSymbol = compilation!.GetTypeByMetadataName(IncludeAttributeFullName);
            if (includeAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var enumAsStringAttributeSymbol = compilation!.GetTypeByMetadataName(EnumAsStringAttributeFullName);
            if (enumAsStringAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var enumLabelAttributeSymbol = compilation!.GetTypeByMetadataName(EnumLabelAttributeFullName);
            if (enumLabelAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var flagsAttributeSymbol = compilation!.GetTypeByMetadataName(FlagsAttributeFullName);
            if (flagsAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var customNameAttributeSymbol = compilation!.GetTypeByMetadataName(CustomNameAttributeFullName);
            if (customNameAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            // loop over the candidate fields, and keep the ones that are actually annotated
#pragma warning disable S1481 // Unused local variables should be removed
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var classSymbols = FilterClassSymbols(receiver!, compilation, includeAttributeSymbol!);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore S1481 // Unused local variables should be removed

            var enumSymbols = FilterEnumSymbols(receiver!, compilation, includeAttributeSymbol!);

            ////CleanOutputFolders(TargetPath);
            var enumCodeGenerator = new EnumCodeGenerator();
            enumCodeGenerator.Generate(
                TargetPath,
                flagsAttributeSymbol,
                enumAsStringAttributeSymbol,
                enumLabelAttributeSymbol,
                enumSymbols);
        }

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CandidateReceiver());
        }

        internal bool RegisterAttributes(SourceGeneratorContext context, out CandidateReceiver? typescriptCandidateReceiver, out Compilation? compilation)
        {
            // add the attribute text
            context.AddSource(CustomNameAttributeName, SourceText.From(CustomNameAttributeText, Encoding.UTF8));
            context.AddSource(EnumAsStringAttributeName, SourceText.From(EnumAsStringAttributeText, Encoding.UTF8));
            context.AddSource(EnumLabelAttributeName, SourceText.From(EnumLabelAttributeText, Encoding.UTF8));
            context.AddSource(IncludeAttributeName, SourceText.From(IncludeAttributeText, Encoding.UTF8));

            // retrieve the populated receiver
            if (!(context.SyntaxReceiver is CandidateReceiver receiver))
            {
                typescriptCandidateReceiver = null;
                compilation = null;
                return false;
            }

            typescriptCandidateReceiver = receiver;

            // we're going to create a new compilation that contains the attribute.
            // TODO: we should allow source generators to provide source during initialize, so that this step isn't required.
            var csharpCompilation = context.Compilation as CSharpCompilation;
            if (csharpCompilation is null)
            {
                compilation = null;
                return false;
            }

            var options = csharpCompilation.SyntaxTrees[0].Options as CSharpParseOptions;
            compilation = context.Compilation.AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(SourceText.From(CustomNameAttributeText, Encoding.UTF8), options),
                CSharpSyntaxTree.ParseText(SourceText.From(EnumAsStringAttributeText, Encoding.UTF8), options),
                CSharpSyntaxTree.ParseText(SourceText.From(EnumLabelAttributeText, Encoding.UTF8), options),
                CSharpSyntaxTree.ParseText(SourceText.From(IncludeAttributeText, Encoding.UTF8), options));

            return true;
        }

        internal List<NamedTypeSymbolData> FilterClassSymbols(CandidateReceiver receiver, Compilation compilation, INamedTypeSymbol attributeSymbol)
        {
            var classSymbols = new List<NamedTypeSymbolData>();
            foreach (var classDeclarationSyntax in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                var namedTypeSymbol = model.GetDeclaredSymbol(classDeclarationSyntax);

                var attributeClass = namedTypeSymbol?.GetAttributes().FirstOrDefault(
                    a => a.AttributeClass?.Equals(
                        attributeSymbol,
                        SymbolEqualityComparer.Default) == true);
                if (attributeClass != null)
                {
                    var name = attributeClass.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? null;
                    classSymbols.Add(new NamedTypeSymbolData(namedTypeSymbol!, name));
                }
            }

            return classSymbols;
        }

        internal List<NamedTypeSymbolData> FilterEnumSymbols(
            CandidateReceiver receiver,
            Compilation compilation,
            INamedTypeSymbol attributeSymbol)
        {
            var enumSymbols = new List<NamedTypeSymbolData>();
            foreach (var enumDeclarationSyntax in receiver.CandidateEnums)
            {
                var model = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);
                var namedTypeSymbol = model.GetDeclaredSymbol(enumDeclarationSyntax);
                var attributeClass = namedTypeSymbol?.GetAttributes().FirstOrDefault(
                    a =>
                        a.AttributeClass?.Equals(
                            attributeSymbol,
                            SymbolEqualityComparer.Default) == true);
                if (attributeClass != null)
                {
                    var name = attributeClass.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? null;
                    enumSymbols.Add(new NamedTypeSymbolData(namedTypeSymbol!, name));
                }
            }

            return enumSymbols;
        }

        internal void CleanOutputFolders(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var rootDirectoryInfo = new DirectoryInfo(path);
            foreach (var file in rootDirectoryInfo.GetFiles())
            {
                file.Delete();
            }

            var modelsPath = $"{path}/{ModelsPath}";
            var enumsPath = $"{path}/{EnumsPath}";
            var servicesPath = $"{path}/{ServicesPath}";
            CleanFilesAndFolders(modelsPath, enumsPath, servicesPath);
        }

        internal void CleanFilesAndFolders(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var directoryInfo = new DirectoryInfo(path);

                foreach (var file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }

                foreach (var dir in directoryInfo.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
        }
    }
}
