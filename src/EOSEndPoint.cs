using System.Net;
using Epic.OnlineServices;

namespace Backdash.EOS;

public class EOSEndPoint : EndPoint
{
	public EOSIdentity Identity;

	public EOSEndPoint(EOSIdentity identity)
	{
		Identity = identity;
	}

	public override SocketAddress Serialize()
	{
		return Identity.ToSocketAddress();
	}
}
