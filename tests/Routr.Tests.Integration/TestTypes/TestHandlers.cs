namespace Routr.Tests.Integration.TestTypes;


public class PingHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken ct)
        => Task.FromResult("pong");
}

public class PongNotificationHandler : INotificationHandler<PongNotification>
{
    public static int CallCount;

    public Task Handle(PongNotification notification, CancellationToken ct)
    {
        Interlocked.Increment(ref CallCount);
        return Task.CompletedTask;
    }
}