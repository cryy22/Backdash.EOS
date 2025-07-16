using System.Net;
using System.Net.Sockets;
using Backdash.Network.Client;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace Backdash.EOS;

public class EOSSocket(P2PInterface p2pInterface, SocketId socketId, ProductUserId[] productUserIds, EOSIdentity localIdentity) : IPeerSocket
{
	public AddressFamily AddressFamily => AddressFamily.Unspecified;
	public int Port { get; }

	private byte[] _receiveAry = new byte[256];
	private byte[] _sendAry = new byte[256];
	private SocketId _dummySocketId = new();

	public ValueTask<int> ReceiveFromAsync(
		Memory<byte> buffer,
		SocketAddress address,
		CancellationToken cancellationToken
	)
	{
		var localUserId = productUserIds[localIdentity.IdIndex];

		var sizeOptions = new GetNextReceivedPacketSizeOptions
		{
			LocalUserId = localUserId,
		};
		var result = p2pInterface.GetNextReceivedPacketSize(ref sizeOptions, out uint nextPacketSize);
		if (result == Result.NotFound)
		{
			return ValueTask.FromResult(0);
		}
		if (result != Result.Success)
		{
			return ValueTask.FromException<int>(new Exception(result.ToString()));
		}

		Console.WriteLine("RECEIVE start");
		if (_receiveAry.Length < nextPacketSize)
		{
			Console.WriteLine($"resizing rcv array. initial length: {_receiveAry.Length}");
			var targetLength = _receiveAry.Length;
			while (targetLength < nextPacketSize)
			{
				targetLength *= 2;
			}

			Array.Resize(ref _receiveAry, targetLength);
			Console.WriteLine($"array resized. new length: {_receiveAry.Length}");
		}
		var receiveSeg = new ArraySegment<byte>(_receiveAry, 0, (int) nextPacketSize);

		var receivePacketOptions = new ReceivePacketOptions
		{
			LocalUserId = localUserId,
			MaxDataSizeBytes = nextPacketSize,
		};

		ProductUserId senderId = new ProductUserId();
		result = p2pInterface.ReceivePacket(
			ref receivePacketOptions,
			ref senderId,
			ref _dummySocketId,
			out byte _,
			receiveSeg,
			out uint bytesWritten
		);
		receiveSeg.AsSpan().CopyTo(buffer.Span);

		if (result == Result.NotFound)
		{
			return ValueTask.FromResult(0);
		}

		if (result != Result.Success)
		{
			return ValueTask.FromException<int>(new Exception(result.ToString()));
		}
		Console.WriteLine("RECEIVE successful");

		ref EOSIdentity senderIdentity = ref EOSIdentityExtensions.FromSocketAddress(address);
		for (var i = 0; i < productUserIds.Length; i++)
		{
			if (senderId == productUserIds[i])
			{
				senderIdentity.IdIndex = i;
				break;
			}
		}

		return ValueTask.FromResult((int) bytesWritten);
	}

	public ValueTask<SocketReceiveFromResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public ValueTask<int> SendToAsync(
		ReadOnlyMemory<byte> buffer,
		SocketAddress socketAddress,
		CancellationToken cancellationToken
	)
	{
		Console.WriteLine("SEND start");
		if (_sendAry.Length < buffer.Length)
		{
			Console.WriteLine($"resizing send array. initial length: {_sendAry.Length}");
			var targetLength = _sendAry.Length;
			while (targetLength < buffer.Length)
			{
				targetLength *= 2;
			}

			Array.Resize(ref _sendAry, targetLength);
			Console.WriteLine($"array resized. new length: {_sendAry.Length}");
		}
		var sendSeg = new ArraySegment<byte>(_sendAry, 0, buffer.Length);
		buffer.Span.CopyTo(sendSeg.AsSpan());

		EOSIdentity recipientIdentity = EOSIdentityExtensions.FromSocketAddress(socketAddress);
		var sendPacketOptions = new SendPacketOptions
		{
			LocalUserId = productUserIds[localIdentity.IdIndex],
			RemoteUserId = productUserIds[recipientIdentity.IdIndex],
			SocketId = socketId,
			Channel = 0,
			AllowDelayedDelivery = true,
			Reliability = PacketReliability.ReliableOrdered,
			DisableAutoAcceptConnection = false,
			Data = sendSeg,
		};

		var result = p2pInterface.SendPacket(ref sendPacketOptions);
		if (result != Result.Success)
		{
			return ValueTask.FromException<int>(new Exception(result.ToString()));
		}

		Console.WriteLine("SEND successful");
		return ValueTask.FromResult(buffer.Length);
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
