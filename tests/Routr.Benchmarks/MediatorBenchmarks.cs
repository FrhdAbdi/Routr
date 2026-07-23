using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Routr.Benchmarks;


[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private ISender _reflectionSender = null!;
    private ISender _generatedSender = null!;
    private readonly BenchRequest _request = new("hello");

    [GlobalSetup]
    public void Setup()
    {
        // ----- Reflection-based sender (manual Sender class) -----
        var reflectionServices = new ServiceCollection();
        reflectionServices.AddTransient<IRequestHandler<BenchRequest, string>, BenchHandler>();
        reflectionServices.AddTransient<ISender, Sender>();   // the existing reflection-based Sender
        var reflectionProvider = reflectionServices.BuildServiceProvider();
        _reflectionSender = reflectionProvider.GetRequiredService<ISender>();

        // ----- Source-generated sender -----
        var generatedServices = new ServiceCollection();
        // AddRoutrHandlers is generated here. It registers GeneratedSender + BenchHandler
        generatedServices.AddRoutrHandlers();
        var generatedProvider = generatedServices.BuildServiceProvider();
        _generatedSender = generatedProvider.GetRequiredService<ISender>();
    }

    [Benchmark(Baseline = true)]
    public Task<string> ReflectionBasedSender()
        => _reflectionSender.Send(_request);

    [Benchmark]
    public Task<string> SourceGeneratedSender()
        => _generatedSender.Send(_request);
}