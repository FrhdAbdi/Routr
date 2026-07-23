namespace Routr.Tests.Integration.TestTypes;


public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public bool BeforeCalled { get; private set; }
    public bool AfterCalled { get; private set; }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        BeforeCalled = true;
        var response = await next();
        AfterCalled = true;
        return response;
    }
}