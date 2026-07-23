using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Routr.SourceGenerator.Tests;


public class RouterGeneratorSnapshotTests
{
    static RouterGeneratorSnapshotTests() =>
        Verifier.DerivePathInfo(
            (sourceFile, projectDirectory, type, method) => new(
                directory: Path.Combine(projectDirectory, "Snapshots"),
                typeName: type.Name,
                methodName: method.Name));

    [Fact]
    public Task SingleRequestHandler_GeneratesCorrectCode()
    {
        var source = """
            using Routr;
            using System.Threading;
            using System.Threading.Tasks;

            public record MyRequest(string Name) : IRequest<string>;

            public class MyHandler : IRequestHandler<MyRequest, string>
            {
                public Task<string> Handle(MyRequest request, CancellationToken ct)
                    => Task.FromResult(request.Name);
            }
            """;

        var (_, diagnostics, generatedSources) = TestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var allGenerated = string.Join("\n\n", generatedSources);
        return Verifier.Verify(allGenerated);
    }

    [Fact]
    public Task MultipleRequestHandlers_GeneratesAllArms()
    {
        var source = """
        using Routr;
        using System.Threading;
        using System.Threading.Tasks;

        public record QueryA : IRequest<string>;
        public record QueryB : IRequest<int>;

        public class HandlerA : IRequestHandler<QueryA, string>
        {
            public Task<string> Handle(QueryA request, CancellationToken ct) => Task.FromResult("a");
        }

        public class HandlerB : IRequestHandler<QueryB, int>
        {
            public Task<int> Handle(QueryB request, CancellationToken ct) => Task.FromResult(1);
        }
        """;

        var (_, diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var allGenerated = string.Join("\n\n", generatedSources);
        return Verifier.Verify(allGenerated);
    }

    [Fact]
    public Task NotificationHandler_GeneratesPublisher()
    {
        var source = """
        using Routr;
        using System.Threading;
        using System.Threading.Tasks;

        public class MyNotification : INotification { }

        public class MyNotificationHandler : INotificationHandler<MyNotification>
        {
            public Task Handle(MyNotification notification, CancellationToken ct) => Task.CompletedTask;
        }
        """;

        var (_, diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var allGenerated = string.Join("\n\n", generatedSources);
        return Verifier.Verify(allGenerated);
    }

    [Fact]
    public Task VoidRequest_UsesUnit()
    {
        var source = """
        using Routr;
        using System.Threading;
        using System.Threading.Tasks;

        public record VoidCommand : IRequest;

        public class VoidHandler : IRequestHandler<VoidCommand>
        {
            public Task<Unit> Handle(VoidCommand request, CancellationToken ct) => Task.FromResult(Unit.Value);
        }
        """;

        var (_, diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var allGenerated = string.Join("\n\n", generatedSources);
        return Verifier.Verify(allGenerated);
    }

    [Fact]
    public Task RequestAndNotification_GeneratesAll()
    {
        var source = """
        using Routr;
        using System.Threading;
        using System.Threading.Tasks;

        public record Query : IRequest<string>;
        public record Event : INotification;

        public class QueryHandler : IRequestHandler<Query, string>
        {
            public Task<string> Handle(Query request, CancellationToken ct) => Task.FromResult("ok");
        }

        public class EventHandler : INotificationHandler<Event>
        {
            public Task Handle(Event notification, CancellationToken ct) => Task.CompletedTask;
        }
        """;

        var (_, diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var allGenerated = string.Join("\n\n", generatedSources);
        return Verifier.Verify(allGenerated);
    }
}