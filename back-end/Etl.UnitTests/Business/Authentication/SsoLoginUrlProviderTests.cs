using Etl.Business.Authentication;
using Etl.Business.Authentication.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

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
		var ssoUrl = "https://fakeurl.com";
		_configuration["SsoLoginUrl"] = ssoUrl;
		var expected = new Uri(ssoUrl);

		// Act
		var actual = _sut.GetSsoLoginUrl();

		// Assert
		actual.Should().Be(expected);
	}
}