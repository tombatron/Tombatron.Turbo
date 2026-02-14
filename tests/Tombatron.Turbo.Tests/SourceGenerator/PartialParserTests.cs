using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Xunit;

namespace Tombatron.Turbo.Tests.SourceGenerator;

public class PartialParserTests
{
    #region ExtractModelType

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ExtractModelType_WithNullOrEmptyContent_ReturnsNull(string? content)
    {
        var result = PartialParser.ExtractModelType(content!);

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractModelType_WithNoModelDirective_ReturnsNull()
    {
        var result = PartialParser.ExtractModelType("<div>Hello</div>");

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractModelType_WithSimpleType_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model string");

        result.Should().Be("string");
    }

    [Fact]
    public void ExtractModelType_WithQualifiedType_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model MyApp.Models.Product");

        result.Should().Be("MyApp.Models.Product");
    }

    [Fact]
    public void ExtractModelType_WithGenericType_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model List<string>");

        result.Should().Be("List<string>");
    }

    [Fact]
    public void ExtractModelType_WithNestedGenericType_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model Dictionary<string, List<int>>");

        result.Should().Be("Dictionary<string, List<int>>");
    }

    [Fact]
    public void ExtractModelType_WithValueTuple_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model (string RoomId, ChatRoom Room)");

        result.Should().Be("(string RoomId, ChatRoom Room)");
    }

    [Fact]
    public void ExtractModelType_WithTupleWithQualifiedTypes_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model (string RoomId, Tombatron.Turbo.Chat.ChatRoom Room)");

        result.Should().Be("(string RoomId, Tombatron.Turbo.Chat.ChatRoom Room)");
    }

    [Fact]
    public void ExtractModelType_WithNullableType_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model string?");

        result.Should().Be("string?");
    }

    [Fact]
    public void ExtractModelType_WithNullableTuple_ReturnsType()
    {
        var result = PartialParser.ExtractModelType("@model (string Name, int Age)?");

        result.Should().Be("(string Name, int Age)?");
    }

    [Fact]
    public void ExtractModelType_WithTrailingWhitespace_ReturnsTrimmedType()
    {
        var result = PartialParser.ExtractModelType("@model string   ");

        result.Should().Be("string");
    }

    [Fact]
    public void ExtractModelType_WithTrailingCarriageReturn_ReturnsTrimmedType()
    {
        var result = PartialParser.ExtractModelType("@model string\r");

        result.Should().Be("string");
    }

    [Fact]
    public void ExtractModelType_WithModelDirectiveAfterOtherContent_ReturnsType()
    {
        var content = "@using MyApp\n@model Product\n<div>Content</div>";

        var result = PartialParser.ExtractModelType(content);

        result.Should().Be("Product");
    }

    #endregion

    #region IsPartialFile

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsPartialFile_WithNullOrEmpty_ReturnsFalse(string? filePath)
    {
        var result = PartialParser.IsPartialFile(filePath!);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsPartialFile_WithNonCshtmlFile_ReturnsFalse()
    {
        var result = PartialParser.IsPartialFile("/Pages/_Something.html");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsPartialFile_WithNoUnderscorePrefix_ReturnsFalse()
    {
        var result = PartialParser.IsPartialFile("/Pages/Something.cshtml");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/Pages/_Layout.cshtml")]
    [InlineData("/Pages/_ViewStart.cshtml")]
    [InlineData("/Pages/_ViewImports.cshtml")]
    public void IsPartialFile_WithExcludedFile_ReturnsFalse(string filePath)
    {
        var result = PartialParser.IsPartialFile(filePath);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/Pages/Shared/_Message.cshtml")]
    [InlineData("/Views/Shared/_RoomEntry.cshtml")]
    [InlineData("/Pages/_MyPartial.cshtml")]
    public void IsPartialFile_WithValidPartial_ReturnsTrue(string filePath)
    {
        var result = PartialParser.IsPartialFile(filePath);

        result.Should().BeTrue();
    }

    #endregion

    #region GetViewPath

    [Fact]
    public void GetViewPath_WithEmptyPath_ReturnsEmpty()
    {
        var result = PartialParser.GetViewPath("");

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetViewPath_WithPagesPath_ReturnsFromPages()
    {
        var result = PartialParser.GetViewPath("/project/Pages/Shared/_Message.cshtml");

        result.Should().Be("/Pages/Shared/_Message.cshtml");
    }

    [Fact]
    public void GetViewPath_WithViewsPath_ReturnsFromViews()
    {
        var result = PartialParser.GetViewPath("/project/Views/Shared/_Message.cshtml");

        result.Should().Be("/Views/Shared/_Message.cshtml");
    }

    [Fact]
    public void GetViewPath_WithBackslashes_NormalizesToForwardSlashes()
    {
        var result = PartialParser.GetViewPath("C:\\project\\Pages\\Shared\\_Message.cshtml");

        result.Should().Be("/Pages/Shared/_Message.cshtml");
    }

    [Fact]
    public void GetViewPath_WithNoPagesOrViewsSegment_ReturnsFileNameWithoutExtension()
    {
        var result = PartialParser.GetViewPath("/some/other/_Message.cshtml");

        result.Should().Be("_Message");
    }

    #endregion

    #region GetPartialName

    [Fact]
    public void GetPartialName_WithEmptyPath_ReturnsEmpty()
    {
        var result = PartialParser.GetPartialName("");

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPartialName_WithUnderscorePrefix_ReturnsNameWithoutUnderscore()
    {
        var result = PartialParser.GetPartialName("/Pages/Shared/_Message.cshtml");

        result.Should().Be("Message");
    }

    [Fact]
    public void GetPartialName_WithoutUnderscorePrefix_ReturnsFullName()
    {
        var result = PartialParser.GetPartialName("/Pages/Shared/Message.cshtml");

        result.Should().Be("Message");
    }

    #endregion

    #region Parse

    [Fact]
    public void Parse_WithNonPartialFile_ReturnsNull()
    {
        var result = PartialParser.Parse("/Pages/Index.cshtml", "@model string");

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_WithPartialWithoutModel_ReturnsInfoWithNullModelType()
    {
        var result = PartialParser.Parse(
            "/Pages/Shared/_Message.cshtml",
            "<div>Hello</div>");

        result.Should().NotBeNull();
        result!.PartialName.Should().Be("Message");
        result.ModelTypeName.Should().BeNull();
    }

    [Fact]
    public void Parse_WithPartialWithSimpleModel_ReturnsInfoWithModelType()
    {
        var result = PartialParser.Parse(
            "/Pages/Shared/_Message.cshtml",
            "@model string\n<div>@Model</div>");

        result.Should().NotBeNull();
        result!.PartialName.Should().Be("Message");
        result.ModelTypeName.Should().Be("string");
    }

    [Fact]
    public void Parse_WithPartialWithTupleModel_ReturnsInfoWithTupleModelType()
    {
        var result = PartialParser.Parse(
            "/Pages/Shared/_RoomEntry.cshtml",
            "@model (string RoomId, Tombatron.Turbo.Chat.ChatRoom Room)\n<div>@Model.RoomId</div>");

        result.Should().NotBeNull();
        result!.PartialName.Should().Be("RoomEntry");
        result.ModelTypeName.Should().Be("(string RoomId, Tombatron.Turbo.Chat.ChatRoom Room)");
    }

    #endregion
}
