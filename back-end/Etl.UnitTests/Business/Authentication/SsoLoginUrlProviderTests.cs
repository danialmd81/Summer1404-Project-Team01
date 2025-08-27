using Etl.Business.Authentication;
using Etl.Business.Authentication.Abstractions;

namespace Etl.UnitTests.Business.Authentication;

public class SsoLoginUrlProviderTests
{
    private readonly ConfigurationManager _configuration;
    private readonly ISsoLoginUrlProvider _sut;

	public SsoLoginUrlProviderTests()
	{
		_configuration = new ConfigurationManager();
		_sut = new SsoLoginUrlProvider();
	}

	[Fact]
	public void GetSsoLoginUrl_ShouldReturnConfigUrlConfiguredUrl_Whenever()
	{
        // Arrange
        var ssoUrl = "fakeurl.com";
        _configuration["SsoLoginUrl"] = ssoUrl;
		var expected = new Uri(ssoUrl);

		// Act
		var actual = _sut.GetSsoLoginUrl();

		// Assert
		actual.

	}
}