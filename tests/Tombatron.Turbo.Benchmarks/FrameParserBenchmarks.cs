using BenchmarkDotNet.Attributes;
using Tombatron.Turbo.SourceGenerator;

namespace Tombatron.Turbo.Benchmarks;

/// <summary>
/// Benchmarks for FrameParser performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class FrameParserBenchmarks
{
    private string _singleFrameHtml = null!;
    private string _multipleFramesHtml = null!;
    private string _nestedFramesHtml = null!;
    private string _largeDocumentHtml = null!;
    private string _noFramesHtml = null!;

    [GlobalSetup]
    public void Setup()
    {
        _singleFrameHtml = """
            @page
            @model IndexModel
            <h1>Hello</h1>
            <turbo-frame id="cart-items">
                <div>Item 1</div>
                <div>Item 2</div>
            </turbo-frame>
            """;

        _multipleFramesHtml = """
            @page
            @model IndexModel
            <h1>Dashboard</h1>
            <turbo-frame id="notifications">
                <div>Notification 1</div>
            </turbo-frame>
            <turbo-frame id="recent-activity">
                <div>Activity 1</div>
            </turbo-frame>
            <turbo-frame id="stats">
                <div>Stats</div>
            </turbo-frame>
            <turbo-frame id="user-profile">
                <div>Profile</div>
            </turbo-frame>
            <turbo-frame id="settings">
                <div>Settings</div>
            </turbo-frame>
            """;

        _nestedFramesHtml = """
            @page
            @model IndexModel
            <turbo-frame id="outer">
                <div>Outer content</div>
                <turbo-frame id="middle">
                    <div>Middle content</div>
                    <turbo-frame id="inner">
                        <div>Inner content</div>
                    </turbo-frame>
                </turbo-frame>
            </turbo-frame>
            """;

        // Generate a large document (~50KB)
        var frames = string.Join("\n", Enumerable.Range(1, 50)
            .Select(i => $"""
                <turbo-frame id="item_{i}">
                    <div class="card">
                        <h3>Item {i}</h3>
                        <p>Description for item {i} with some additional text to make it more realistic.</p>
                        <button>Action</button>
                    </div>
                </turbo-frame>
                """));
        _largeDocumentHtml = $"@page\n@model ListModel\n<h1>Items</h1>\n{frames}";

        _noFramesHtml = """
            @page
            @model IndexModel
            <h1>Hello World</h1>
            <p>This is a page without any turbo-frames.</p>
            <div class="content">
                <p>Just regular HTML content here.</p>
            </div>
            """;
    }

    [Benchmark(Baseline = true)]
    public int Parse_SingleFrame()
    {
        var frames = FrameParser.Parse(_singleFrameHtml);
        return frames.Length;
    }

    [Benchmark]
    public int Parse_MultipleFrames()
    {
        var frames = FrameParser.Parse(_multipleFramesHtml);
        return frames.Length;
    }

    [Benchmark]
    public int Parse_NestedFrames()
    {
        var frames = FrameParser.Parse(_nestedFramesHtml);
        return frames.Length;
    }

    [Benchmark]
    public int Parse_LargeDocument_50Frames()
    {
        var frames = FrameParser.Parse(_largeDocumentHtml);
        return frames.Length;
    }

    [Benchmark]
    public int Parse_NoFrames()
    {
        var frames = FrameParser.Parse(_noFramesHtml);
        return frames.Length;
    }

    [Benchmark]
    public bool ContainsRazorExpression_Static()
    {
        return FrameParser.ContainsRazorExpression("cart-items");
    }

    [Benchmark]
    public bool ContainsRazorExpression_Dynamic()
    {
        return FrameParser.ContainsRazorExpression("item_@Model.Id");
    }

    [Benchmark]
    public string GetStaticIdPortion_Static()
    {
        return FrameParser.GetStaticIdPortion("cart-items");
    }

    [Benchmark]
    public string GetStaticIdPortion_Dynamic()
    {
        return FrameParser.GetStaticIdPortion("item_@Model.Id");
    }
}
