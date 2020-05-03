using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeScriptGenerator
{
    internal class CandidateReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public List<EnumDeclarationSyntax> CandidateEnums { get; } = new List<EnumDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds
                && cds.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(cds);
                return;
            }

            if (syntaxNode is EnumDeclarationSyntax eds
                && eds.AttributeLists.Count > 0)
            {
                CandidateEnums.Add(eds);
            }
        }
    }
}
