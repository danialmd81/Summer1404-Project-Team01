namespace Etl.Business.Authentication.Abstractions;

public interface ISsoLoginUrlProvider
{
	Uri GetSsoLoginUrl();
}