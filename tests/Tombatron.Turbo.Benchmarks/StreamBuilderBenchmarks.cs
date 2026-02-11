using BenchmarkDotNet.Attributes;
using Tombatron.Turbo.Streams;

namespace Tombatron.Turbo.Benchmarks;

/// <summary>
/// Benchmarks for TurboStreamBuilder performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class StreamBuilderBenchmarks
{
    private const string SmallHtml = "<div>Hello</div>";
    private const string MediumHtml = "<div class=\"card\"><h2>Title</h2><p>Some content here with more text to make it a bit longer.</p></div>";
    private string _largeHtml = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Generate a large HTML string (~10KB)
        var items = Enumerable.Range(1, 100)
            .Select(i => $"<li id=\"item-{i}\">Item {i} with some description text</li>");
        _largeHtml = $"<ul>{string.Join("", items)}</ul>";
    }

    [Benchmark(Baseline = true)]
    public string SingleAction_SmallHtml()
    {
        var builder = new TurboStreamBuilder();
        builder.Append("target", SmallHtml);
        return builder.Build();
    }

    [Benchmark]
    public string SingleAction_MediumHtml()
    {
        var builder = new TurboStreamBuilder();
        builder.Append("target", MediumHtml);
        return builder.Build();
    }

    [Benchmark]
    public string SingleAction_LargeHtml()
    {
        var builder = new TurboStreamBuilder();
        builder.Append("target", _largeHtml);
        return builder.Build();
    }

    [Benchmark]
    public string MultipleActions_3()
    {
        var builder = new TurboStreamBuilder();
        builder.Append("list", SmallHtml);
        builder.Update("count", "42");
        builder.Remove("old-item");
        return builder.Build();
    }

    [Benchmark]
    public string MultipleActions_10()
    {
        var builder = new TurboStreamBuilder();
        for (int i = 0; i < 10; i++)
        {
            builder.Append($"target-{i}", SmallHtml);
        }
        return builder.Build();
    }

    [Benchmark]
    public string MultipleActions_50()
    {
        var builder = new TurboStreamBuilder();
        for (int i = 0; i < 50; i++)
        {
            builder.Append($"target-{i}", SmallHtml);
        }
        return builder.Build();
    }

    [Benchmark]
    public string AllActionTypes()
    {
        var builder = new TurboStreamBuilder();
        builder.Append("append-target", SmallHtml);
        builder.Prepend("prepend-target", SmallHtml);
        builder.Replace("replace-target", SmallHtml);
        builder.Update("update-target", SmallHtml);
        builder.Remove("remove-target");
        builder.Before("before-target", SmallHtml);
        builder.After("after-target", SmallHtml);
        return builder.Build();
    }

    [Benchmark]
    public string EscapeAttribute_Clean()
    {
        return TurboStreamBuilder.EscapeAttribute("simple-target-id");
    }

    [Benchmark]
    public string EscapeAttribute_NeedsEscaping()
    {
        return TurboStreamBuilder.EscapeAttribute("target<with\"special&chars>");
    }
}
