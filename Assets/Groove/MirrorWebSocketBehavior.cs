#if !UNITY_WEBGL || UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Mirror.Groove
{
	public class MirrorWebSocketBehavior : WebSocketBehavior
	{
		internal WebSocketServerContainer Server;

		internal int connectionId = 0;

		protected override void OnOpen()
		{
			base.OnOpen();
			Server.OnConnect(connectionId, this);
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			Server.OnMessage(connectionId, e.RawData);
		}

		protected override void OnClose(CloseEventArgs e)
		{
			Server.OnDisconnect(connectionId);
			base.OnClose(e);
		}
	}

}
#endif