using System.Net;
using System.Net.Sockets;
using Backdash.Network.Client;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace Backdash.EOS;

public class EOSSocket(P2PInterface p2pInterface, SocketId socketId, ProductUserId[] productUserIds, EOSIdentity localIdentity) : IPeerSocket
{
	private const int _maxConsecutiveReceiveAttempts = 100;
	// TODO: nasty. configure as fraction of framerate or exponential backoff or something
	private static TimeSpan _timePerAttempt = TimeSpan.FromMilliseconds(50f);

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
			LocalUserId = productUserIds[localIdentity.IdIndex],
			MaxDataSizeBytes = (uint) buffer.Length,
		};

		var result = Result.NotFound;

		ProductUserId senderId = null;
		uint bytesWritten = 0;

		for (var i = 0; i < _maxConsecutiveReceiveAttempts; i++)
		{
			result = p2pInterface.ReceivePacket(
				ref receivePacketOptions,
				out senderId,
				out _,
				out _,
				buffer.Span,
				out bytesWritten
			);

			if (result == Result.Success)
			{
				break;
			}

			Console.WriteLine($"Failed receive: {result} (attempt #{i+1})");
			Thread.Sleep(_timePerAttempt);
		}

		if (result != Result.Success)
		{
			return ValueTask.FromException<int>(new Exception(result.ToString()));
		}

		ref var senderIdentity = ref EOSIdentityExtensions.FromSocketAddress(address);
		for (var i = 0; i < productUserIds.Length; i++)
		{
			if (senderId == productUserIds[i])
			{
				senderIdentity.IdIndex = i;
				break;
			}
		}

		Console.WriteLine("RECEIVE successful");
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
				LocalUserId = productUserIds[localIdentity.IdIndex],
				RemoteUserId = productUserIds[recipientIdentity.IdIndex],
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

			Console.WriteLine("SEND successful");
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
