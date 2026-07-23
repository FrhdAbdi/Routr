namespace Routr.Sample.WebApi.Features;


public record CreateUserCommand(string Name) : IRequest<Guid>;


public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    public Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}