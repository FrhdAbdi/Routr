namespace Routr.Sample.WebApi.Features;


public record GetUserQuery(Guid Id) : IRequest<string>;


public class GetUserHandler : IRequestHandler<GetUserQuery, string>
{
    public Task<string> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"User-{request.Id}");
    }
}