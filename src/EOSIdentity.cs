using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Epic.OnlineServices;

namespace Backdash.EOS;

public struct EOSIdentity(ProductUserId id)
{
	public ProductUserId Id = id;
}

public static class EOSIdentityExtensions
{
	public static SocketAddress ToSocketAddress(this in EOSIdentity identity)
	{
		var address = new SocketAddress(AddressFamily.Unspecified, Unsafe.SizeOf<EOSIdentity>());
		var identityBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in identity, 1));
		identityBytes.CopyTo(address.Buffer.Span);

		return address;
	}

	public static ref EOSIdentity FromSocketAddress(SocketAddress socketAddress)
	{
		return ref MemoryMarshal.AsRef<EOSIdentity>(socketAddress.Buffer.Span);
	}
}
