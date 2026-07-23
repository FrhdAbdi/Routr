using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Routr.SourceGenerator.Tests;


public static class TestHelper
{
    private static readonly ImmutableArray<MetadataReference> MetadataReferences;

    static TestHelper()
    {
        MetadataReferences = ImmutableArray.Create<MetadataReference>(
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(typeof(ISender).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location)
        );
    }

    public static (Compilation outputCompilation, ImmutableArray<Diagnostic> diagnostics, string[] generatedSources)
        RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            MetadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        IIncrementalGenerator generator = new RouterGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics);

        var generatedSources = outputCompilation.SyntaxTrees
            .Skip(1)
            .Select(tree => tree.ToString())
            .ToArray();

        return (outputCompilation, diagnostics, generatedSources);
    }
}