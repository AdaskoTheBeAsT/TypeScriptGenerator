using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#pragma warning disable 162

namespace TypeScriptGenerator
{
    [Generator]
    public partial class CodeGenerator
        : ISourceGenerator
    {
        public const string Header = "// Generated by https://github.com/AdaskoTheBeAsT/TypeScriptGenerator";
        public const string ModelsPath = "models";
        public const string EnumsPath = "enums";
        public const string ServicesPath = "services";
        public const string FlagsAttributeFullName = "System.FlagsAttribute";
        public const string ControllerBaseTypeName = "Microsoft.AspNetCore.Mvc.ControllerBase";
        public const string ControllerTypeName = "Microsoft.AspNetCore.Mvc.Controller";
        public static readonly Encoding Uft8WithoutBomEncoding = new UTF8Encoding(false, true);
        private string? _typeScriptGeneratorOutputPath;

        public void Execute(GeneratorExecutionContext context)
        {
            if (!RegisterAttributes(context, out var receiver, out var compilation))
            {
                return;
            }

            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                "build_property.typescriptgeneratoroutputpath",
                out _typeScriptGeneratorOutputPath))
            {
                return;
            }

            if (!Directory.Exists(_typeScriptGeneratorOutputPath))
            {
                Directory.CreateDirectory(_typeScriptGeneratorOutputPath);
            }

            // get the newly bound attributes
            var includeAttributeSymbol = compilation!.GetTypeByMetadataName("TypeScriptGenerator.Attributes.IncludeAttribute");
            if (includeAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            var customNameAttributeSymbol = compilation!.GetTypeByMetadataName("TypeScriptGenerator.Attributes.CustomNameAttribute");
            if (customNameAttributeSymbol is null)
            {
                throw new SymbolNotFoundException();
            }

            ////CleanOutputFolders(TargetPath);
            // loop over the candidate fields, and keep the ones that are actually annotated
            var enumSymbols = FilterEnumSymbols(receiver!, compilation, includeAttributeSymbol!);
            if (enumSymbols?.Count > 0)
            {
                var enumHelperGenerator = new EnumHelperGenerator();
                enumHelperGenerator.Generate(_typeScriptGeneratorOutputPath);
            }
            
            var enumCodeGenerator = new EnumCodeGenerator();
            enumCodeGenerator.Generate(
                _typeScriptGeneratorOutputPath,
                compilation!,
                enumSymbols);
            var classSymbols = FilterClassSymbols(receiver!, compilation, includeAttributeSymbol!);
            var modelCodeGenerator = new ModelCodeGenerator();
            modelCodeGenerator.Generate(_typeScriptGeneratorOutputPath, compilation, classSymbols);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                ////Debugger.Launch();
            }
#endif 
            context.RegisterForSyntaxNotifications(() => new CandidateReceiver());
        }


        internal bool RegisterAttributes(GeneratorExecutionContext context, out CandidateReceiver? typescriptCandidateReceiver, out Compilation? compilation)
        {
            // add the attribute text
            ////context.AddSource(CustomNameAttributeName, SourceText.From(CustomNameAttributeText, Encoding.UTF8));
            ////context.AddSource(EnumAsStringAttributeName, SourceText.From(EnumAsStringAttributeText, Encoding.UTF8));
            ////context.AddSource(EnumLabelAttributeName, SourceText.From(EnumLabelAttributeText, Encoding.UTF8));
            ////context.AddSource(IncludeAttributeName, SourceText.From(IncludeAttributeText, Encoding.UTF8));

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
            if (!(context.Compilation is CSharpCompilation csharpCompilation))
            {
                compilation = null;
                return false;
            }

            ////var options = csharpCompilation.SyntaxTrees[0].Options as CSharpParseOptions;
            ////compilation = context.Compilation.AddSyntaxTrees(
            ////    CSharpSyntaxTree.ParseText(SourceText.From(CustomNameAttributeText, Encoding.UTF8), options),
            ////    CSharpSyntaxTree.ParseText(SourceText.From(EnumAsStringAttributeText, Encoding.UTF8), options),
            ////    CSharpSyntaxTree.ParseText(SourceText.From(EnumLabelAttributeText, Encoding.UTF8), options),
            ////    CSharpSyntaxTree.ParseText(SourceText.From(IncludeAttributeText, Encoding.UTF8), options));
            compilation = context.Compilation;

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
