using MessageBufferModel.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MessageBufferModel.Persistance
{
	public class ReceiverPersistance : IReceiverPersistance
	{

		private List<string> freeReceiverList;
		private IHubContext<MainHub> hubsender;
		public ConcurrentQueue<string> msgQueue;

		public ReceiverPersistance(IHubContext<MainHub> hubContext)
		{
			msgQueue = new ConcurrentQueue<string>();
			hubsender = hubContext;
			freeReceiverList = new List<string>();
		}

		public Task DisposeReceiverInstance(string connID)
		{
			return Task.Run(() =>
			{
				bool res = freeReceiverList.Remove(connID);
				if (res)
				{
					log("Receiver client disposed.");
				}
				else
				{
					log("Sender client disposed.");
				}
			});
		}

		public string GetFreeReceiverInstanceConnectionID()
		{
			log("Fetching free receiver client connectionID...");
			if (freeReceiverList.Count == 0)
			{
				return string.Empty;
			}
			else
			{
				return freeReceiverList[0];
			}
		}

		public Task RegisterReceiverInstance(string connectionId)
		{
			return Task.Run(() =>
			{
				log("New receiver client registered");
				freeReceiverList.Add(connectionId);
				if (freeReceiverList.Count == 1 && msgQueue.Count != 0)
				{
								//OnFreeReceiver(connectionId);
								msgQueue.TryDequeue(out string msgToSent);
					hubsender.Clients.Client(connectionId).SendAsync("ReceiveMessage", msgToSent);
					freeReceiverList.Remove(connectionId);
					log($"message sent from buffer, buffer count {msgQueue.Count}");
				}
			});
		}

		private void log(string msg)
		{
			Debug.WriteLine($"receiver persistance >>> {msg}");
		}

	}
}
