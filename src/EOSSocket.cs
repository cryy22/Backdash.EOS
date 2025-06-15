using System.Net;
using System.Net.Sockets;
using Backdash.Network.Client;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace Backdash.EOS;

public class EOSSocket(P2PInterface p2pInterface, SocketId socketId, EOSIdentity localIdentity) : IPeerSocket
{
	public AddressFamily AddressFamily => AddressFamily.Unspecified;
	public int Port { get; }

	public ValueTask<int> ReceiveFromAsync(
		Memory<byte> buffer,
		SocketAddress address,
		CancellationToken cancellationToken
	)
	{
		var receivePacketOptions = new ReceivePacketOptions
		{
			LocalUserId = localIdentity.Id,
			MaxDataSizeBytes = (uint) buffer.Length,
		};

		var result = p2pInterface.ReceivePacket(
			ref receivePacketOptions,
			out var senderId,
			out var _,
			out var _,
			buffer.Span,
			out var bytesWritten
		);

		if (result != Result.Success)
		{
			return ValueTask.FromException<int>(new Exception(result.ToString()));
		}
		ref var senderIdentity = ref EOSIdentityExtensions.FromSocketAddress(address);
		senderIdentity.Id = senderId;

		return ValueTask.FromResult((int) bytesWritten);
	}

	public ValueTask<SocketReceiveFromResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public unsafe ValueTask<int> SendToAsync(
		ReadOnlyMemory<byte> buffer,
		SocketAddress socketAddress,
		CancellationToken cancellationToken
	)
	{
		EOSIdentity recipientIdentity = EOSIdentityExtensions.FromSocketAddress(socketAddress);

		fixed (byte* bytePtr = buffer.Span)
		{
			var sendPacketOptions = new SendPacketOptions
			{
				LocalUserId = localIdentity.Id,
				RemoteUserId = recipientIdentity.Id,
				SocketId = socketId,
				Channel = 0,
				AllowDelayedDelivery = false,
				Reliability = PacketReliability.ReliableOrdered,
				DisableAutoAcceptConnection = false,
				Data = (nint) bytePtr,
				DataLengthInBytes = (uint) buffer.Length,
			};

			var result = p2pInterface.SendPacket(ref sendPacketOptions);
			if (result != Result.Success)
			{
				return ValueTask.FromException<int>(new Exception(result.ToString()));
			}

			return ValueTask.FromResult(buffer.Length);
		}
	}

	public ValueTask<int> SendToAsync(
		ReadOnlyMemory<byte> buffer,
		EndPoint remoteEndPoint,
		CancellationToken cancellationToken
	)
	{
		throw new NotImplementedException();
	}

	public void Close()
	{ }

	public void Dispose()
	{ }
}
