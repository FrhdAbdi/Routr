using Microsoft.Extensions.DependencyInjection;
using Routr.Tests.Integration.TestTypes;
using Xunit;

namespace Routr.Tests.Integration;


public class MediatorIntegrationTests
{
    [Fact]
    public async Task Send_Request_ReturnsHandlerResponse()
    {
        var services = new ServiceCollection();
        services.AddRoutrHandlers();
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.Send(new PingRequest());

        Assert.Equal("pong", result);
    }

    [Fact]
    public async Task Send_WithPipeline_ExecutesBehavior()
    {
        var services = new ServiceCollection();
        var behavior = new LoggingBehavior<PingRequest, string>();
        services.AddSingleton<IPipelineBehavior<PingRequest, string>>(behavior);
        services.AddRoutrHandlers();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.Send(new PingRequest());

        Assert.Equal("pong", result);
        Assert.True(behavior.BeforeCalled);
        Assert.True(behavior.AfterCalled);
    }

    [Fact]
    public async Task Send_PassesCancellationToken()
    {
        var handler = new CancellationAwareHandler();
        var services = new ServiceCollection();
        services.AddRoutrHandlers();
        services.AddSingleton<IRequestHandler<CancellationRequest, string>>(handler);
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        using var cts = new CancellationTokenSource();
        var result = await sender.Send(new CancellationRequest(), cts.Token);

        Assert.Equal("ok", result);
        Assert.True(handler.TokenWasPassed);
    }

    [Fact]
    public async Task Send_WhenHandlerThrows_ExceptionPropagates()
    {
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<FailingRequest, string>, FailingHandler>();
        services.AddRoutrHandlers();
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.Send(new FailingRequest()));
    }

    [Fact]
    public async Task Publish_Notification_CallsAllHandlers()
    {
        var services = new ServiceCollection();
        services.AddRoutrHandlers();
        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        PongNotificationHandler.CallCount = 0;

        await publisher.Publish(new PongNotification());

        Assert.Equal(1, PongNotificationHandler.CallCount);
    }

    public record CancellationRequest : IRequest<string>;
    public class CancellationAwareHandler : IRequestHandler<CancellationRequest, string>
    {
        public bool TokenWasPassed { get; private set; }
        public Task<string> Handle(CancellationRequest request, CancellationToken ct)
        {
            TokenWasPassed = ct.CanBeCanceled;
            return Task.FromResult("ok");
        }
    }

    public record FailingRequest : IRequest<string>;
    public class FailingHandler : IRequestHandler<FailingRequest, string>
    {
        public Task<string> Handle(FailingRequest request, CancellationToken ct)
            => throw new InvalidOperationException("Test exception");
    }
}