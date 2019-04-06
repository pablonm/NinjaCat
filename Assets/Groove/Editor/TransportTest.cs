//using NUnit.Framework;
//using System;
//using System.Net;
//using System.Text;
//using System.Threading;

//namespace Mirror.Groove.Tests
//{
//	[TestFixture]
//	public class TransportTest
//	{
//		// just a random port that will hopefully not be taken
//		const int port = 9587;

//		WebSocketServerContainer server;

//		[SetUp]
//		public void Setup()
//		{
//			server = new WebSocketServerContainer();
//			server.StartServer("127.0.0.1", port, int.MaxValue);
//		}

//		[TearDown]
//		public void TearDown()
//		{
//			server.StopServer();
//		}

//		[Test]
//		public void DisconnectImmediateTest()
//		{
//			WebSocketClientContainer client = new WebSocketClientContainer();
//			client.Connect("127.0.0.1", port);

//			// I should be able to disconnect right away
//			// if connection was pending,  it should just cancel
//			client.Disconnect();

//			Assert.That(client.SocketConnected, Is.False);
//		}

//		[Test]
//		public void SpamConnectTest()
//		{
//			WebSocketClientContainer client = new WebSocketClientContainer();
//			for (int i = 0; i < 1000; i++)
//			{
//				client.Connect("127.0.0.1", port);
//				Assert.That(client.SocketConnected, Is.True);
//				client.Disconnect();
//				Assert.That(client.SocketConnected, Is.False);
//			}
//		}

//		[Test]
//		public void ReconnectTest()
//		{
//			WebSocketClientContainer client = new WebSocketClientContainer();
//			client.Connect("127.0.0.1", port);

//			// wait for successful connection
//			WebSocketMessage connectMsg = NextMessage(client);
//			Assert.That(connectMsg.Type, Is.EqualTo(TransportEvent.Connected));
//			// disconnect and lets try again
//			client.Disconnect();


//			// connecting should flush message queue  right?
//			client.Connect("127.0.0.1", port);
//			// wait for successful connection
//			connectMsg = NextMessage(client);
//			Assert.That(connectMsg.Type, Is.EqualTo(TransportEvent.Connected));

//			client.Disconnect();
//		}

//		[Test]
//		public void ServerTest()
//		{
//			Encoding utf8 = Encoding.UTF8;
//			WebSocketClientContainer client = new WebSocketClientContainer();

//			client.Connect("127.0.0.1", port);

//			// we  should first receive a connected message
//			WebSocketMessage connectMsg = NextMessage(server);
//			Assert.That(connectMsg.Type, Is.EqualTo(TransportEvent.Connected));


//			// then we should receive the data
//			client.ClientInterface.Send(utf8.GetBytes("Hello world"));
//			WebSocketMessage dataMsg = NextMessage(server);
//			Assert.That(dataMsg.Type, Is.EqualTo(TransportEvent.Data));
//			string str = utf8.GetString(dataMsg.Data);
//			Assert.That(str, Is.EqualTo("Hello world"));

//			// finally when the client disconnect,  we should get a disconnected message
//			client.Disconnect();
//			WebSocketMessage disconnectMsg = NextMessage(server);
//			Assert.That(disconnectMsg.Type, Is.EqualTo(TransportEvent.Disconnected));
//		}

//		[Test]
//		public void ClientTest()
//		{
//			Encoding utf8 = Encoding.UTF8;
//			WebSocketClientContainer client = new WebSocketClientContainer();

//			client.Connect("127.0.0.1", port);

//			// we  should first receive a connected message
//			WebSocketMessage serverConnectMsg = NextMessage(server);
//			int id = serverConnectMsg.connectionId;

//			// we  should first receive a connected message
//			WebSocketMessage clientConnectMsg = NextMessage(client);
//			Assert.That(serverConnectMsg.Type, Is.EqualTo(TransportEvent.Connected));

//			// Send some data to the client
//			server.Send(id, utf8.GetBytes("Hello world"));
//			WebSocketMessage dataMsg = NextMessage(client);
//			Assert.That(dataMsg.Type, Is.EqualTo(TransportEvent.Data));
//			string str = utf8.GetString(dataMsg.Data);
//			Assert.That(str, Is.EqualTo("Hello world"));

//			// finally if the server stops,  the clients should get a disconnect error
//			server.StopServer();
//			WebSocketMessage disconnectMsg = NextMessage(client);
//			Assert.That(disconnectMsg.Type, Is.EqualTo(TransportEvent.Disconnected));

//			client.Disconnect();
//		}

//		[Test]
//		public void ServerDisconnectClientTest()
//		{
//			WebSocketClientContainer client = new WebSocketClientContainer();

//			client.Connect("127.0.0.1", port);

//			// we  should first receive a connected message
//			WebSocketMessage serverConnectMsg = NextMessage(server);
//			int id = serverConnectMsg.connectionId;

//			bool result = server.Disconnect(id);
//			Assert.That(result, Is.True);
//		}

//		[Test]
//		public void ClientKickedCleanupTest()
//		{
//			WebSocketClientContainer client = new WebSocketClientContainer();

//			client.Connect("127.0.0.1", port);

//			// read connected message on client
//			WebSocketMessage clientConnectedMsg = NextMessage(client);
//			Assert.That(clientConnectedMsg.Type, Is.EqualTo(TransportEvent.Connected));

//			// read connected message on server
//			WebSocketMessage serverConnectMsg = NextMessage(server);
//			int id = serverConnectMsg.connectionId;

//			// server kicks the client
//			bool result = server.Disconnect(id);
//			Assert.That(result, Is.True);

//			// wait for client disconnected message
//			WebSocketMessage clientDisconnectedMsg = NextMessage(client);
//			Assert.That(clientDisconnectedMsg.Type, Is.EqualTo(TransportEvent.Disconnected));

//			// was everything cleaned perfectly?
//			// if Connecting or Connected is still true then we wouldn't be able
//			// to reconnect otherwise
//			Assert.That(client.SocketConnected, Is.False);
//		}

//		[Test]
//		public void GetConnectionInfoTest()
//		{
//			// connect a client
//			WebSocketClientContainer client = new WebSocketClientContainer();
//			client.Connect("127.0.0.1", port);

//			// get server's connect message
//			WebSocketMessage serverConnectMsg = NextMessage(server);
//			Assert.That(serverConnectMsg.Type, Is.EqualTo(TransportEvent.Connected));

//			// get server's connection info for that client
//			string address;
//			if (server.GetConnectionInfo(serverConnectMsg.connectionId, out address))
//			{
//				Assert.That(address == "127.0.0.1");
//			}
//			else Assert.Fail();

//			client.Disconnect();
//		}

//		[Test]
//		public void LargeMessageTest()
//		{
//			// connect a client
//			WebSocketClientContainer client = new WebSocketClientContainer();
//			client.Connect("127.0.0.1", port);

//			// we  should first receive a connected message
//			WebSocketMessage serverConnectMsg = NextMessage(server);
//			int id = serverConnectMsg.connectionId;

//			// Send a large message,  bigger thank 64K
//			client.ClientInterface.Send(new byte[100000]);
//			WebSocketMessage dataMsg = NextMessage(server);
//			Assert.That(dataMsg.Type, Is.EqualTo(TransportEvent.Data));
//			Assert.That(dataMsg.Data.Length, Is.EqualTo(100000));

//			// finally if the server stops,  the clients should get a disconnect error
//			server.StopServer();
//			client.Disconnect();

//		}

//		static WebSocketMessage NextMessage(WebSocketServerContainer server)
//		{
//			WebSocketMessage message;
//			int count = 0;

//			while (!server.GetNextMessage(out message))
//			{
//				count++;
//				Thread.Sleep(100);

//				if (count >= 100)
//				{
//					Assert.Fail("The message did not get to the server");
//				}
//			}

//			return message;
//		}

//		static WebSocketMessage NextMessage(WebSocketClientContainer client)
//		{
//			WebSocketMessage message;
//			int count = 0;

//			while (!client.GetNextMessage(out message))
//			{
//				count++;
//				Thread.Sleep(100);

//				if (count >= 100)
//				{
//					Assert.Fail("The message did not get to the server");
//				}
//			}

//			return message;
//		}

//	}
//}