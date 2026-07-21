using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Routr.SourceGenerator.Tests;


public class RouterGeneratorTests
{
    private static (Compilation, GeneratorDriver) RunGenerator(string source)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ISender).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        IIncrementalGenerator generator = new RouterGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .RunGenerators(compilation);

        return (compilation, driver);
    }

    [Fact]
    public void Generator_ProducesGeneratedSender_WhenHandlerExists()
    {
        var source = """
            using Routr;
            using System.Threading;
            using System.Threading.Tasks;

            public record MyRequest(string Name) : IRequest<string>;

            public class MyHandler : IRequestHandler<MyRequest, string>
            {
                public Task<string> Handle(MyRequest request, CancellationToken ct)
                    => Task.FromResult(request.Name);
            }
            """;

        var (_, driver) = RunGenerator(source);
        var result = driver.GetRunResult();

        var generatedFile = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.EndsWith("GeneratedSender.g.cs"));

        Assert.NotNull(generatedFile);
        var text = generatedFile!.GetText().ToString();
        Assert.Contains("GeneratedSender", text);
        Assert.Contains("MyRequest", text);
        Assert.Contains("MyHandler", text);
    }

    [Fact]
    public void Generator_ProducesNoOutput_WhenNoHandlerExists()
    {
        var (_, driver) = RunGenerator("public class Empty {}");
        var result = driver.GetRunResult();
        Assert.Empty(result.GeneratedTrees);
    }
}