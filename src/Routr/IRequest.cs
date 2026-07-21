namespace Routr;


/// <summary>
/// Marker interface for a request that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Marker interface for a request that does not return a value.
/// </summary>
public interface IRequest : IRequest<Unit>
{
}