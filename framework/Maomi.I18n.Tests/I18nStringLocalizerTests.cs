using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Localization;
using Moq;
using System.Globalization;

namespace Maomi.I18n.Tests;

public class I18nStringLocalizerTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<I18nContext> _contextMock;
    private readonly Mock<I18nResourceFactory> _resourceFactoryMock;
    private readonly Mock<LocalizationOptions> _localizationOptionsMock;
    private readonly I18nStringLocalizer _localizer;

    public I18nStringLocalizerTests()
    {
        var fixture = new AutoFixture.Fixture();
        fixture.Customize(new AutoMoqCustomization());

        _serviceProviderMock = fixture.Freeze<Mock<IServiceProvider>>();
        _contextMock = fixture.Freeze<Mock<I18nContext>>();
        _resourceFactoryMock = fixture.Freeze<Mock<I18nResourceFactory>>();
        _localizationOptionsMock = fixture.Freeze<Mock<LocalizationOptions>>();

        _localizer = new I18nStringLocalizer(
            _contextMock.Object,
            _resourceFactoryMock.Object,
            _serviceProviderMock.Object,
            _localizationOptionsMock.Object
        );
    }

    [Theory]
    [InlineData("TestString", "测试字符串", "zh-CN")]
    [InlineData("TestString", "Test String", "en-US")]
    [InlineData("TestString", "Prueba de cadena", "es-ES")]
    public void Indexer_ReturnsLocalizedString_ForMultipleCultures(string name, string expectedValue, string culture)
    {
        var localizedString = new LocalizedString(name, expectedValue, false);

        _resourceFactoryMock.Setup(r => r.Resources).Returns(new List<I18nResource>()
        {
            new DictionaryResource(new CultureInfo(culture), new Dictionary<string, object> { { name, expectedValue } })
        });
        _contextMock.Setup(c => c.Culture).Returns(new CultureInfo(culture));

        var result = _localizer[name];

        Assert.Equal(localizedString.ToString(), result.ToString());
    }

    [Fact]
    public void GetAllStrings_ReturnsAllLocalizedStrings()
    {
        var localizedString = new LocalizedString("TestString", "测试字符串", false);
        var resourceMock = new Mock<I18nResource>();
        resourceMock.Setup(r => r.GetAllStrings(It.IsAny<bool>())).Returns(new List<LocalizedString> { localizedString });
        _resourceFactoryMock.Setup(r => r.Resources).Returns(new List<I18nResource> { resourceMock.Object });

        var result = _localizer.GetAllStrings(false);

        Assert.Contains(localizedString, result);
    }
}

public static class I18NStringLocalizerHelper
{
    public static void SetupFind(LocalizedString localizedString)
    {
    }
}
