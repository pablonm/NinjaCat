﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

public class WebSocketClient
{
	private Uri mUrl;

	public WebSocketClient(Uri url)
	{
		mUrl = url;
		Debug.Log(mUrl);

		string protocol = mUrl.Scheme;
		if (!protocol.Equals("ws") && !protocol.Equals("wss"))
			throw new ArgumentException("Unsupported protocol: " + protocol);
	}

	public void SendString(string str)
	{
		Send(Encoding.UTF8.GetBytes (str));
	}

	public string RecvString()
	{
		byte[] retval = Recv();
		if (retval == null)
			return null;
		return Encoding.UTF8.GetString (retval);
	}

	public void ManuallyAddMessage(byte[] Msg)
	{

	}

#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern int SocketCreate (string url);

	[DllImport("__Internal")]
	private static extern int SocketState (int socketInstance);

	[DllImport("__Internal")]
	private static extern void SocketSend (int socketInstance, byte[] ptr, int length);

	[DllImport("__Internal")]
	private static extern void SocketRecv (int socketInstance, byte[] ptr, int length);

	[DllImport("__Internal")]
	private static extern int SocketRecvLength (int socketInstance);

	[DllImport("__Internal")]
	private static extern void SocketClose (int socketInstance);

	[DllImport("__Internal")]
	private static extern int SocketError (int socketInstance, byte[] ptr, int length);

	int m_NativeRef = 0;

	public void Send(byte[] buffer)
	{
		SocketSend (m_NativeRef, buffer, buffer.Length);
	}

	public byte[] Recv()
	{
		int length = SocketRecvLength (m_NativeRef);
		if (length == 0)
			return null;
		byte[] buffer = new byte[length];
		SocketRecv (m_NativeRef, buffer, length);
		return buffer;
	}

	public void Connect()
	{
		m_NativeRef = SocketCreate (mUrl.ToString());
	}
 
	public bool Connected 
	{
		get
		{
			return SocketState(m_NativeRef) != 0;
		}
	}

	public void Close()
	{
		SocketClose(m_NativeRef);
	}

	public string error
	{
		get {
			const int bufsize = 1024;
			byte[] buffer = new byte[bufsize];
			int result = SocketError (m_NativeRef, buffer, bufsize);

			if (result == 0)
				return null;

			return Encoding.UTF8.GetString (buffer);				
		}
	}
#else
	WebSocketSharp.WebSocket m_Socket;
	WebSocketSharp.Server.WebSocketServer m_Server;
	Queue<byte[]> m_Messages = new Queue<byte[]>();
	string m_Error = null;

	bool m_IsConnected = false;

	public bool Connected
	{
		get {
			return m_IsConnected;
		}
	}

	public void Connect()
	{
		m_Socket = new WebSocketSharp.WebSocket(mUrl.ToString());
		m_Socket.OnMessage += (sender, e) => { lock (m_Messages) { m_Messages.Enqueue(e.RawData); } };
		m_Socket.OnOpen += (sender, e) => m_IsConnected = true;
		m_Socket.OnError += (sender, e) => m_Error = e.Message;
		m_Socket.Connect();
	}

	public void Send(byte[] buffer)
	{
		m_Socket.Send(buffer);
	}

	public byte[] Recv()
	{
		lock (m_Messages)
		{
			if (m_Messages.Count == 0)
				return null;
			return m_Messages.Dequeue();
		}
	}

	public void Close()
	{
		m_IsConnected = false;
		m_Socket.Close();
	}

	public string error
	{
		get {
			return m_Error;
		}
	}

#endif 
}