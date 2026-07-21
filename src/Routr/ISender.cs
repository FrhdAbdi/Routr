namespace Routr;


/// <summary>
/// Sends requests to their corresponding handlers.
/// </summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}