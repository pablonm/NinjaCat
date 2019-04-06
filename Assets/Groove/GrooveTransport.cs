using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Groove
{
	public class GrooveTransport : Transport
	{
		public int Port = 7777;

		[SerializeField]
		private bool SecureServer = false;

		[SerializeField]
		private string PathToCertificate = "/../certificate.pfx";

		[SerializeField]
		private string CertificatePassword = "FillMeOut";

		public WebSocketClientContainer Client = new WebSocketClientContainer();

#if !UNITY_WEBGL || UNITY_EDITOR
		public WebSocketServerContainer Server = new WebSocketServerContainer();

#endif

		private const string WebGLServerErrorMessage = "The server can not be started on a WebGL build. This is an inherent limitation of the WebGL target and will not be supported.";

		public override bool ClientConnected()
		{
			return Client.SocketConnected;
		}

		public override void ClientDisconnect()
		{
			Client.Disconnect();
		}

		public override bool ClientGetNextMessage(out TransportEvent transportEvent, out byte[] data)
		{
			WebSocketMessage msg;
			bool GotMessage = Client.GetNextMessage(out msg);
			if (GotMessage)
			{
				transportEvent = msg.Type;
				data = msg.Data;
			}
			else
			{
				transportEvent = TransportEvent.Disconnected;
				data = null;
			}
			return GotMessage;
		}

		public override bool ClientSend(int channelId, byte[] data)
		{
			Client.ClientInterface.Send(data);
			return true;
		}

		public override bool GetConnectionInfo(int connectionId, out string address)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			return Server.GetConnectionInfo(connectionId, out address);
#else
			address = "";
			return false;
#endif
		}

		public override bool ServerActive()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			return Server.ServerActive;
#else
			return false;
#endif
		}

		public override bool ServerDisconnect(int connectionId)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			return Server.RemoveConnectionId(connectionId);
#else
			return false;
#endif
		}

		public override bool ServerGetNextMessage(out int connectionId, out TransportEvent transportEvent, out byte[] data)
		{

			transportEvent = Mirror.TransportEvent.Disconnected;
			data = null;
			connectionId = 0;
#if !UNITY_WEBGL || UNITY_EDITOR

			WebSocketMessage message = Server.GetNextMessage();
			if (message == null)
				return false;
			connectionId = message.connectionId;

			switch (message.Type)
			{
				case TransportEvent.Connected:
					transportEvent = TransportEvent.Connected;
					break;
				case TransportEvent.Data:
					transportEvent = TransportEvent.Data;
					data = message.Data;
					break;
				case TransportEvent.Disconnected:
					transportEvent = TransportEvent.Disconnected;
					break;
				default:
					break;
			}
			return true;

#else
			Debug.LogError(WebGLServerErrorMessage);
			return false;
#endif
		}

		public override bool ServerSend(int connectionId, int channelId, byte[] data)
		{
#if !UNITY_WEBGL || UNITY_EDITOR

			return Server.Send(connectionId, data);
#else
			return false;
#endif
		}

		public override void ServerStop()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			Server.StopServer();
#endif
		}

		public override void Shutdown()
		{
			if (Client != null)
			{
				if (Client.SocketConnected)
				{
					Client.Disconnect();
				}
			}
#if !UNITY_WEBGL || UNITY_EDITOR
			if (Server != null)
			{
				if (Server.ServerActive)
				{
					Server.StopServer();
				}
			}
#endif
		}

		public override int GetMaxPacketSize(int channelId = 0)
		{
			return int.MaxValue;
		}

		public override void ClientConnect(string address)
		{
			Client.Connect(address, Port, SecureServer);
		}

		public override void ServerStart()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (SecureServer)
			{
				Server.StartServer(Port, PathToCertificate, CertificatePassword);
			}
			else
			{
				Server.StartServer(Port);
			}

#else
			Debug.LogError(WebGLServerErrorMessage);
#endif
		}
	}
}
