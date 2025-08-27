using Etl.Business.Authentication.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Etl.Business.Authentication;

public class SsoLoginUrlProvider : ISsoLoginUrlProvider
{
	private readonly IConfiguration _configuration;
	public SsoLoginUrlProvider(IConfiguration configuratin)
	{
		_configuration = configuratin ?? throw new ArgumentNullException(nameof(configuratin));
	}
	public Uri GetSsoLoginUrl()
	{
		return new Uri(_configuration.GetValue("SsoLoginUrl", "http://my-keycloak.com"));
	}
}
