using Microsoft.CodeAnalysis;

namespace TypeScriptGenerator
{
    public class NamedTypeSymbolData
    {
        public NamedTypeSymbolData(INamedTypeSymbol namedTypeSymbol)
            : this(namedTypeSymbol, namedTypeSymbol?.Name)
        {
        }

        public NamedTypeSymbolData(INamedTypeSymbol namedTypeSymbol, string? name)
        {
            NamedTypeSymbol = namedTypeSymbol;
            Name = (string.IsNullOrEmpty(name) ? namedTypeSymbol?.Name : name)!;
            AssemblyName = namedTypeSymbol?.ContainingAssembly?.Name ?? string.Empty;
        }

        public INamedTypeSymbol NamedTypeSymbol { get; }

        public string Name { get; }

        public string AssemblyName { get; }
    }
}
