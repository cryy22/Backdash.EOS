using System.Net;
using Epic.OnlineServices;

namespace Backdash.EOS;

public class EOSEndPoint : EndPoint
{
	public ProductUserId Id;

	public EOSEndPoint(ProductUserId id)
	{
		Id = id;
	}
}
