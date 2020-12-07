using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MessageBufferModel.Persistance;
using System.Diagnostics;

namespace MessageBufferModel.Hubs
{
	public class MainHub : Hub
	{
		private readonly ReceiverPersistance receiverPersistance;
		//private readonly MessageBuffer messageQueue;

		public MainHub(IReceiverPersistance recPersistance)
		{
			receiverPersistance = (ReceiverPersistance)recPersistance;
			//messageQueue = receiverPersistance.MsgBuffer;

		}

		public override async Task OnConnectedAsync()
		{
			log("Client connected.");
			await base.OnConnectedAsync();
		}

		public async Task RegisterReceiver()
		{
			receiverPersistance.RegisterReceiverInstance(Context.ConnectionId);

		}

		public async Task ReceiveMessage(string msg)
		{
			log("Message sent to queue.");
			receiverPersistance.msgQueue.Enqueue(msg);

			string destination = receiverPersistance.GetFreeReceiverInstanceConnectionID();
			if (!string.IsNullOrEmpty(destination))
			{
				bool res = receiverPersistance.msgQueue.TryDequeue(out string msgToSent);
				await Clients.Client(destination).SendAsync("ReceiveMessage", msgToSent);
				receiverPersistance.DisposeReceiverInstance(destination);
				log("Message get from queue (sent to receiver).");
			}
			else
			{
				log("No receiver at this moment");
			}
		}

		public void Finished()
		{
			log("beckend instance finished");
			receiverPersistance.RegisterReceiverInstance(Context.ConnectionId);
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			log("Client disconnected.");
			receiverPersistance.DisposeReceiverInstance(Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}
		
		private void log(string msg)
		{
			Debug.WriteLine($"main hub >>> {msg}");
		}
	}
}
