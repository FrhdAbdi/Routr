namespace Routr.Tests;


public class UnitTests
{
    [Fact]
    public void Unit_Value_IsSingleton()
    {
        var a = Unit.Value;
        var b = Unit.Value;
        Assert.Equal(a, b);
    }

    [Fact]
    public void Unit_Equals_OtherUnit()
    {
        Assert.Equal(Unit.Value, new Unit());
    }

    [Fact]
    public void Unit_ToString_ReturnsParentheses()
    {
        Assert.Equal("()", Unit.Value.ToString());
    }
}

public class AbstractionCompilationTests
{
    private sealed class PingRequest : IRequest<string> { }

    private sealed class PingHandler : IRequestHandler<PingRequest, string>
    {
        public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
            => Task.FromResult("pong");
    }

    private sealed class VoidRequest : IRequest { }

    [Fact]
    public async Task Handler_ReturnsExpectedResponse()
    {
        var handler = new PingHandler();
        var result = await handler.Handle(new PingRequest(), CancellationToken.None);
        Assert.Equal("pong", result);
    }

    [Fact]
    public void IRequest_ImplementsIRequestOfUnit()
    {
        Assert.True(typeof(IRequest<Unit>).IsAssignableFrom(typeof(VoidRequest)));
    }
}