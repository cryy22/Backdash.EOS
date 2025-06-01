using Backdash.Network.Client;
using Backdash.Options;

namespace Backdash.EOS;

public class EOSSocketFactory : IPeerSocketFactory
{
	public IPeerSocket Create(int port, NetcodeOptions options)
	{
		return new EOSSocket();
	}
}
