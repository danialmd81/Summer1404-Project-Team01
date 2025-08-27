using Etl.Business.Authentication;
using Etl.Business.Authentication.Abstractions;

namespace Etl.UnitTests.Business.Authentication;

internal class SsoLoginUrlProviderTests
{
	private readonly ISsoLoginUrlProvider _sut;

	public SsoLoginUrlProviderTests()
	{
		_sut = new SsoLoginUrlProvider();
	}
}