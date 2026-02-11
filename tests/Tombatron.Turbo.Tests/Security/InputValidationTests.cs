using FluentAssertions;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.Security;

/// <summary>
/// Tests for input validation in streams and frames.
/// </summary>
public class InputValidationTests
{
    #region Stream Builder Validation

    [Fact]
    public void StreamBuilder_NullTarget_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Append(null!, "<div>content</div>");

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("target");
    }

    [Fact]
    public void StreamBuilder_EmptyTarget_ThrowsArgumentException()
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Append("", "<div>content</div>");

        action.Should().Throw<ArgumentException>()
            .WithParameterName("target");
    }

    [Fact]
    public void StreamBuilder_WhitespaceTarget_ThrowsArgumentException()
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Append("   ", "<div>content</div>");

        action.Should().Throw<ArgumentException>()
            .WithParameterName("target");
    }

    [Fact]
    public void StreamBuilder_NullHtml_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Append("target", null!);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("html");
    }

    [Fact]
    public void StreamBuilder_EmptyHtml_IsAccepted()
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Append("target", "");

        action.Should().NotThrow();
    }

    [Fact]
    public void StreamBuilder_Remove_NullTarget_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Remove(null!);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("target");
    }

    [Fact]
    public void StreamBuilder_Remove_EmptyTarget_ThrowsArgumentException()
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Remove("");

        action.Should().Throw<ArgumentException>()
            .WithParameterName("target");
    }

    #endregion

    #region All Actions Validate Target

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StreamBuilder_AllActions_ValidateTarget(string? invalidTarget)
    {
        var builder = new TurboStreamBuilder();
        var content = "<div>content</div>";

        var actions = new (string name, Action action)[]
        {
            ("Append", () => builder.Append(invalidTarget!, content)),
            ("Prepend", () => builder.Prepend(invalidTarget!, content)),
            ("Replace", () => builder.Replace(invalidTarget!, content)),
            ("Update", () => builder.Update(invalidTarget!, content)),
            ("Before", () => builder.Before(invalidTarget!, content)),
            ("After", () => builder.After(invalidTarget!, content)),
            ("Remove", () => builder.Remove(invalidTarget!)),
        };

        foreach (var (name, action) in actions)
        {
            action.Should().Throw<ArgumentException>($"{name} should validate target");
        }
    }

    #endregion

    #region Content with Null vs Content Actions Validate Html

    [Fact]
    public void StreamBuilder_AppendWithNullHtml_Throws()
    {
        var builder = new TurboStreamBuilder();
        var action = () => builder.Append("target", null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StreamBuilder_PrependWithNullHtml_Throws()
    {
        var builder = new TurboStreamBuilder();
        var action = () => builder.Prepend("target", null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StreamBuilder_ReplaceWithNullHtml_Throws()
    {
        var builder = new TurboStreamBuilder();
        var action = () => builder.Replace("target", null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StreamBuilder_UpdateWithNullHtml_Throws()
    {
        var builder = new TurboStreamBuilder();
        var action = () => builder.Update("target", null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StreamBuilder_BeforeWithNullHtml_Throws()
    {
        var builder = new TurboStreamBuilder();
        var action = () => builder.Before("target", null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StreamBuilder_AfterWithNullHtml_Throws()
    {
        var builder = new TurboStreamBuilder();
        var action = () => builder.After("target", null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Very Long Inputs

    [Fact]
    public void StreamBuilder_VeryLongTarget_IsHandled()
    {
        var builder = new TurboStreamBuilder();
        var longTarget = new string('a', 10000);

        var action = () => builder.Append(longTarget, "<div>content</div>");

        action.Should().NotThrow();
        var result = builder.Build();
        result.Should().Contain(longTarget);
    }

    [Fact]
    public void StreamBuilder_VeryLongHtml_IsHandled()
    {
        var builder = new TurboStreamBuilder();
        var longHtml = $"<div>{new string('x', 100000)}</div>";

        var action = () => builder.Append("target", longHtml);

        action.Should().NotThrow();
        var result = builder.Build();
        result.Should().Contain(longHtml);
    }

    #endregion
}
