namespace Routr.Benchmarks;


public record BenchRequest(string Message) : IRequest<string>;

public class BenchHandler : IRequestHandler<BenchRequest, string>
{
    public Task<string> Handle(BenchRequest request, CancellationToken ct)
        => Task.FromResult(request.Message);
}