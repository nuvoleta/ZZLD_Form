using FluentAssertions;
using ZZLD_Form.Core.Services;

namespace ZZLD_Form.UnitTests.Services;

public class TemplateServiceTests
{
    private readonly TemplateService _templateService;

    public TemplateServiceTests()
    {
        _templateService = new TemplateService();
    }

    [Fact]
    public async Task GetTemplatePathAsync_ReturnsTemplatePath()
    {
        // Act
        var result = await _templateService.GetTemplatePathAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(".pdf");
    }

    [Fact]
    public async Task ValidateTemplateAsync_ReturnsTrue()
    {
        // Act
        var result = await _templateService.ValidateTemplateAsync();

        // Assert
        result.Should().BeTrue();
    }
}
