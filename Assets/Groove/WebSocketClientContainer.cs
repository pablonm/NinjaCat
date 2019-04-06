using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Groove
{
	public class WebSocketClientContainer
	{
		public WebSocketClient ClientInterface;

		public bool SocketConnected { get { return ClientInterface != null && ClientInterface.Connected; } }

		private bool ClientConnectedLastFrame = false;

		public void Connect(string address, int port, bool UseSecureClient)
		{
			ConnectInternal(address, port, UseSecureClient);
			if (LogFilter.Debug)
			{
				Debug.Log("WebSocket client connected");
			}
		}

		private void ConnectInternal(string address, int port, bool UseSecureClient)
		{
			string scheme = UseSecureClient ? "wss://" : "ws://";

			var uri = new System.UriBuilder(scheme, address, port);
			uri.Path += "game";
			if (Mirror.LogFilter.Debug)
			{
				Debug.Log("attempting to start client on: " + uri.ToString());
			}
			ClientInterface = new WebSocketClient(uri.Uri);
			ClientInterface.Connect();
		}

		public void Disconnect()
		{
			ClientInterface.Close();
		}

		public bool GetNextMessage(out WebSocketMessage msg)
		{
			msg = new WebSocketMessage();
			msg.Type = TransportEvent.Disconnected;
			msg.Data = null;

			if (ClientInterface != null)
			{
				if (!ClientConnectedLastFrame)
				{
					ClientConnectedLastFrame = SocketConnected;
					if (ClientConnectedLastFrame)
					{
						msg.Type = TransportEvent.Connected;
						return true;
					}
				}
				try
				{
					var Rcvd = ClientInterface.Recv();
					if (Rcvd != null)
					{
						msg.Type = TransportEvent.Data;
						msg.Data = Rcvd;
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (System.Exception e)
				{
					Debug.LogError("Client Exception: " + e);
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}
}