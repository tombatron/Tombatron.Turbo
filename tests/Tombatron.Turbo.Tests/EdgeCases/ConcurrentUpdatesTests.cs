using FluentAssertions;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.EdgeCases;

/// <summary>
/// Tests for concurrent usage of stream builder.
/// </summary>
public class ConcurrentUpdatesTests
{
    [Fact]
    public async Task StreamBuilder_ParallelBuilds_ProduceCorrectOutput()
    {
        // Each task creates its own builder - they should not interfere
        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            await Task.Yield(); // Force async execution
            var builder = new TurboStreamBuilder();
            builder.Append($"target_{i}", $"<div>Content {i}</div>");
            return builder.Build();
        });

        var results = await Task.WhenAll(tasks);

        for (int i = 1; i <= 10; i++)
        {
            results[i - 1].Should().Contain($"target_{i}");
            results[i - 1].Should().Contain($"Content {i}");
        }
    }

    [Fact]
    public void StreamBuilder_SequentialBuilds_AreIndependent()
    {
        var results = new List<string>();

        for (int i = 0; i < 5; i++)
        {
            var builder = new TurboStreamBuilder();
            builder.Append($"target_{i}", $"<div>Iteration {i}</div>");
            results.Add(builder.Build());
        }

        // Each result should only contain its own iteration
        for (int i = 0; i < 5; i++)
        {
            results[i].Should().Contain($"target_{i}");
            results[i].Should().NotContain($"target_{(i + 1) % 5}");
        }
    }

    [Fact]
    public void StreamBuilder_MultipleBuilds_SameInstance_AreIdentical()
    {
        var builder = new TurboStreamBuilder();
        builder.Append("target", "<div>content</div>");

        var result1 = builder.Build();
        var result2 = builder.Build();
        var result3 = builder.Build();

        result1.Should().Be(result2);
        result2.Should().Be(result3);
    }

    [Fact]
    public void StreamBuilder_AddAfterBuild_ExtendsOutput()
    {
        var builder = new TurboStreamBuilder();
        builder.Append("target1", "<div>First</div>");

        var firstResult = builder.Build();
        firstResult.Should().Contain("target1");
        firstResult.Should().NotContain("target2");

        builder.Append("target2", "<div>Second</div>");
        var secondResult = builder.Build();

        secondResult.Should().Contain("target1");
        secondResult.Should().Contain("target2");
    }

    [Fact]
    public async Task StreamBuilder_ConcurrentBuildCalls_AreSafe()
    {
        var builder = new TurboStreamBuilder();
        for (int i = 0; i < 100; i++)
        {
            builder.Append($"target_{i}", $"<div>Content {i}</div>");
        }

        // Call Build() concurrently from multiple threads
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() => builder.Build()));
        var results = await Task.WhenAll(tasks);

        // All results should be identical
        var first = results[0];
        foreach (var result in results)
        {
            result.Should().Be(first);
        }
    }

    [Fact]
    public void StreamBuilder_ReuseForDifferentResponses_WorksCorrectly()
    {
        // Simulate using different builders for different HTTP responses
        var responses = new List<string>();

        // Response 1: append notification
        var builder1 = new TurboStreamBuilder();
        builder1.Append("notifications", "<div>New message</div>");
        responses.Add(builder1.Build());

        // Response 2: update cart
        var builder2 = new TurboStreamBuilder();
        builder2.Replace("cart-count", "<span>3</span>");
        responses.Add(builder2.Build());

        // Response 3: multiple updates
        var builder3 = new TurboStreamBuilder();
        builder3
            .Remove("flash-message")
            .Append("items", "<li>New item</li>");
        responses.Add(builder3.Build());

        responses[0].Should().Contain("notifications");
        responses[0].Should().NotContain("cart-count");

        responses[1].Should().Contain("cart-count");
        responses[1].Should().NotContain("notifications");

        responses[2].Should().Contain("flash-message");
        responses[2].Should().Contain("items");
    }
}
