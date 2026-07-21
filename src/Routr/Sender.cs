namespace Routr;


/// <summary>
/// Default implementation of <see cref="ISender"/>.
/// This manual (reflection-based) version exists for validation only
/// and will be superseded by Source-Generator-produced dispatch logic.
/// </summary>
public sealed class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for request type '{requestType.Name}'.");

        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;
        return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
    }
}