using FluentAssertions;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.Security;

/// <summary>
/// Tests for XSS prevention in stream output.
/// </summary>
public class XssTests
{
    [Fact]
    public void StreamBuilder_TargetWithScriptTag_IsEscaped()
    {
        var builder = new TurboStreamBuilder();
        var maliciousTarget = "<script>alert('xss')</script>";

        builder.Append(maliciousTarget, "<div>content</div>");
        var result = builder.Build();

        result.Should().NotContain("<script>");
        result.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void StreamBuilder_TargetWithEventHandler_IsEscaped()
    {
        var builder = new TurboStreamBuilder();
        var maliciousTarget = "id\" onclick=\"alert('xss')";

        builder.Append(maliciousTarget, "<div>content</div>");
        var result = builder.Build();

        // The quote should be escaped, preventing attribute injection
        // The raw quote should not appear - it should be &quot;
        result.Should().NotContain("target=\"id\""); // This would allow breaking out of the attribute
        result.Should().Contain("&quot;"); // Quotes are properly escaped
    }

    [Fact]
    public void StreamBuilder_TargetWithJavascriptUrl_IsEscaped()
    {
        var builder = new TurboStreamBuilder();
        var maliciousTarget = "javascript:alert('xss')";

        builder.Append(maliciousTarget, "<div>content</div>");
        var result = builder.Build();

        // The target should be used as-is (it's a DOM ID, not a URL)
        // But special characters should still be escaped
        result.Should().Contain("target=\"javascript:alert(&#39;xss&#39;)\"");
    }

    [Fact]
    public void StreamBuilder_HtmlContent_IsNotEscaped()
    {
        // HTML content is intentionally not escaped - it's the caller's responsibility
        // to ensure safe HTML. This test documents the behavior.
        var builder = new TurboStreamBuilder();
        var htmlContent = "<div onclick=\"alert('xss')\">content</div>";

        builder.Append("target", htmlContent);
        var result = builder.Build();

        // Content is preserved as-is (by design)
        result.Should().Contain(htmlContent);
    }

    [Theory]
    [InlineData("<")]
    [InlineData(">")]
    [InlineData("\"")]
    [InlineData("'")]
    [InlineData("&")]
    public void StreamBuilder_SpecialCharsInTarget_AreEscaped(string specialChar)
    {
        var builder = new TurboStreamBuilder();
        var target = $"id{specialChar}test";

        builder.Append(target, "<div>content</div>");
        var result = builder.Build();

        // The raw special character should not appear in the target attribute
        result.Should().NotContain($"target=\"id{specialChar}test\"");
    }

    [Fact]
    public void StreamBuilder_RemoveAction_TargetIsEscaped()
    {
        var builder = new TurboStreamBuilder();
        var maliciousTarget = "id<script>";

        builder.Remove(maliciousTarget);
        var result = builder.Build();

        result.Should().NotContain("<script>");
        result.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void StreamBuilder_AllActions_EscapeTarget()
    {
        var maliciousTarget = "<img src=x onerror=alert(1)>";
        var safeContent = "<div>safe</div>";

        var actions = new (string name, Action<TurboStreamBuilder> action)[]
        {
            ("Append", b => b.Append(maliciousTarget, safeContent)),
            ("Prepend", b => b.Prepend(maliciousTarget, safeContent)),
            ("Replace", b => b.Replace(maliciousTarget, safeContent)),
            ("Update", b => b.Update(maliciousTarget, safeContent)),
            ("Before", b => b.Before(maliciousTarget, safeContent)),
            ("After", b => b.After(maliciousTarget, safeContent)),
            ("Remove", b => b.Remove(maliciousTarget)),
        };

        foreach (var (name, action) in actions)
        {
            var builder = new TurboStreamBuilder();
            action(builder);
            var result = builder.Build();

            result.Should().NotContain("<img", $"{name} should escape the target");
            result.Should().Contain("&lt;img", $"{name} should contain escaped target");
        }
    }
}
