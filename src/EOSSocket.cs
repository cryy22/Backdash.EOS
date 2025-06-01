using System.Net;
using System.Net.Sockets;
using Backdash.Network.Client;

namespace Backdash.EOS;

public class EOSSocket : IPeerSocket
{
	public AddressFamily AddressFamily => AddressFamily.Unspecified;
	public int Port { get; }

	public ValueTask<int> ReceiveFromAsync(
		Memory<byte> buffer,
		SocketAddress address,
		CancellationToken cancellationToken
	)
	{
		return default;
	}

	public ValueTask<SocketReceiveFromResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
	{
		return default;
	}

	public ValueTask<int> SendToAsync(
		ReadOnlyMemory<byte> buffer,
		SocketAddress socketAddress,
		CancellationToken cancellationToken
	)
	{
		return default;
	}

	public ValueTask<int> SendToAsync(
		ReadOnlyMemory<byte> buffer,
		EndPoint remoteEndPoint,
		CancellationToken cancellationToken
	)
	{
		return default;
	}

	public void Close()
	{ }

	public void Dispose()
	{ }
}
