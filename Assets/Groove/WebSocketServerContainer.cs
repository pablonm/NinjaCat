using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
#if !UNITY_WEBGL || UNITY_EDITOR
using WebSocketSharp;
using WebSocketSharp.Net.WebSockets;
using WebSocketSharp.Server;
#endif

namespace Mirror.Groove
{
	public class WebSocketMessage
	{
		public int connectionId;
		public TransportEvent Type;
		public byte[] Data;
	}


	public class WebSocketServerContainer
	{
#if !UNITY_WEBGL || UNITY_EDITOR
		WebSocketServer WebsocketServer;

		readonly Dictionary<int, IWebSocketSession> WebsocketSessions = new Dictionary<int, IWebSocketSession>();
		public int MaxConnections { get; private set; }

		private readonly Queue<WebSocketMessage> MessageQueue = new Queue<WebSocketMessage>();

		internal void AddMessage(WebSocketMessage webSocketMessage)
		{
			lock (MessageQueue)
			{
				MessageQueue.Enqueue(webSocketMessage);
			}
		}

		int connectionIdCounter = 1;
		

		public WebSocketMessage GetNextMessage()
		{
			lock (MessageQueue)
			{
				if (MessageQueue.Count > 0)
				{
					return MessageQueue.Dequeue();
				}
				return null;
			}
		}

		public bool GetNextMessage(out WebSocketMessage message)
		{
			lock (MessageQueue)
			{
				if (MessageQueue.Count > 0)
				{
					message =  MessageQueue.Dequeue();
					return true;
				}
				message = null;
				return false;
			}
		}

		internal int NextId()
		{
			return Interlocked.Increment(ref connectionIdCounter);
		}

		internal void OnConnect(int connectionId, IWebSocketSession socketBehavior)
		{
			lock (WebsocketSessions)
			{
				WebsocketSessions[connectionId] = socketBehavior;
			}
			var message = new WebSocketMessage { connectionId = connectionId, Type = TransportEvent.Connected };
			AddMessage(message);
		}

		internal void OnMessage(int connectionId, byte[] data)
		{
			var message = new WebSocketMessage { connectionId = connectionId, Type = TransportEvent.Data, Data = data };
			AddMessage(message);
		}

		internal void OnDisconnect(int connectionId)
		{
			lock (WebsocketSessions)
			{
				WebsocketSessions.Remove(connectionId);
			}
			var message = new WebSocketMessage { connectionId = connectionId, Type = TransportEvent.Disconnected };
			AddMessage(message);
		}

		public bool ServerActive { get { return WebsocketServer != null && WebsocketServer.IsListening; } }

		public bool RemoveConnectionId(int connectionId)
		{
			lock (WebsocketSessions)
			{
				IWebSocketSession session;

				if (WebsocketSessions.TryGetValue(connectionId, out session))
				{
					session.Context.WebSocket.Close();
					return true;
				}
			}
			return false;
		}

		public void StopServer()
		{
			WebsocketServer.Stop();
			lock (WebsocketSessions)
			{
				WebsocketSessions.Clear();
			}
		}


		public void StartServer(int Port = 7777, string PathToCert = "", string CertPassword = "")
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			bool SecureServer = false;
			if (PathToCert != "" && CertPassword != "")
			{
				SecureServer = true;
			}
            Debug.Log("Starting websocker server");
			WebsocketServer = new WebSocketServer(Port, SecureServer);
			WebsocketServer.AddWebSocketService<MirrorWebSocketBehavior>("/game", (behaviour) =>
			{
				behaviour.Server = this;
				behaviour.connectionId = NextId();
			});

			if (SecureServer)
			{
				WebsocketServer.SslConfiguration.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(Application.dataPath + PathToCert, CertPassword);
			}
			WebsocketServer.Start();
#else
			Debug.Log("don't start the server on webgl please");
#endif
		}

		public bool GetConnectionInfo(int connectionId, out string address)
		{
			lock (WebsocketSessions)
			{
				IWebSocketSession session;

				if (WebsocketSessions.TryGetValue(connectionId, out session))
				{
					address = session.Context.UserEndPoint.Address.ToString();
					return true;
				}
			}
			address = null;
			return false;
		}

		public bool Send(int connectionId, byte[] data)
		{
			lock (WebsocketSessions)
			{
				IWebSocketSession session;

				if (WebsocketSessions.TryGetValue(connectionId, out session))
				{
					session.Context.WebSocket.Send(data);
					return true;
				}
			}
			return false;
		}

		public bool Disconnect(int connectionId)
		{
			lock (WebsocketSessions)
			{
				IWebSocketSession session;
				if(WebsocketSessions.TryGetValue(connectionId, out session))
				{
					WebsocketServer.WebSocketServices["/game"].Sessions.CloseSession(session.ID);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

#else
		public void StartServer(string address, int port, int maxConnections){
			Debug.LogError("can't start server in WebGL");
		}
#endif


	}
}
