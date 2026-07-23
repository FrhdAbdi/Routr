namespace Routr.Tests.Integration.TestTypes;


public record PingRequest : IRequest<string>;
public record PongNotification : INotification;