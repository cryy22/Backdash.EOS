using Backdash.Network.Client;
using Backdash.Options;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace Backdash.EOS;

public class EOSSocketFactory(P2PInterface p2pInterface, SocketId socketId, ProductUserId[] productUserIds, EOSIdentity localIdentity) : IPeerSocketFactory
{
	public IPeerSocket Create(int port, NetcodeOptions options)
	{
		return new EOSSocket(p2pInterface, socketId, productUserIds, localIdentity);
	}
}
