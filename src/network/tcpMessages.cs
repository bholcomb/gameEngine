using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Util;
using Engine;

namespace Network
{
   public class TcpMessageServer
   {
      public delegate void ClientConnected(TcpClient client);
      public ClientConnected onClientConnected;

      public delegate bool EventCallback(Event evt, TcpClient client);
      public EventCallback filterReceiveEvent;
      public EventCallback filterSendEvent;
      public EventCallback receivedEvent;
      public EventCallback sendingEvent;

      TcpListener myTcpListener;
      Thread myListenThread;
      bool myShouldQuit = false;

      ConcurrentBag<ConcurrentQueue<Event>> myClientQueues = new ConcurrentBag<ConcurrentQueue<Event>>();

      public TcpMessageServer(int port)
      {
         myTcpListener = new TcpListener(IPAddress.Any, port);
         Info.print("Listening on port {0}", port);
         myListenThread = new Thread(new ThreadStart(listenForClients));
         myListenThread.Name = "TCP Message Listener thread";
         myListenThread.Start();
      }

      public void shutdown()
      {
         myShouldQuit = true;
      }

      public void send(Event e)
      {
         foreach (ConcurrentQueue<Event> queue in myClientQueues)
         {
            queue.Enqueue(e);
         }
      }

      void listenForClients()
      {
         myTcpListener.Start();

         while (myShouldQuit == false)
         {
            //blocks until a client has connected to the server
            if (myTcpListener.Pending() == true)
            {
               TcpClient client = myTcpListener.AcceptTcpClient();

               Info.print("Client Attached from {0}", client.Client.RemoteEndPoint.ToString());

               //create a thread to handle communication 
               //with connected client
               Thread clientThread = new Thread(new ParameterizedThreadStart(handleClient));
               clientThread.Name = "TCP Message Server thread";
               clientThread.Start(client);
            }

            Thread.Sleep(10);
         }
      }

      void handleClient(object client)
      {
         TcpClient tcpClient = (TcpClient)client;
         //2MB buffers, should help with overflow conditions
         tcpClient.ReceiveBufferSize = 1024 * 1024 * 2;
         tcpClient.SendBufferSize = 1024 * 1024 * 2;
         NetworkStream clientStream = tcpClient.GetStream();

         ConcurrentQueue<Event> myEventQueue = new ConcurrentQueue<Event>();
         myClientQueues.Add(myEventQueue);

         if(onClientConnected!=null)
         {
            onClientConnected(tcpClient);
         }

         byte[] header = new byte[1000];
         byte[] tempBuffer = new byte[4096];
         byte[] messageBuffer;
         Int32 bytesRead;

         while (myShouldQuit == false)
         {
            bytesRead = 0;

            try
            {
               double readTime = TimeSource.currentTime() + 0.25; //poll socket for 1/4 a second before giving up
               while (clientStream.DataAvailable == true && TimeSource.clockTime() < readTime)
               {
                  //blocks until a client sends a message,  then just read the size of the message
                  bytesRead = clientStream.Read(header, 0, 4);
                  Int32 msgSize = BitConverter.ToInt32(header, 0);

                  if (msgSize == Int32.MaxValue) //this is a shutdown request
                     break;

                  if (msgSize > (1024 * 64)) // a reasonable number
                     throw new Exception("Message received is too big");

                  messageBuffer = new byte[msgSize];
                  int bytesReceived = bytesRead;
                  //copy the size of the message into the buffer, since this is important for decoding
                  Array.Copy(header, 0, messageBuffer, 0, bytesReceived);

                  //read the rest of this message
                  while (bytesReceived < msgSize)
                  {
                     int bytesToRead = msgSize - bytesReceived;
                     bytesRead = clientStream.Read(messageBuffer, bytesReceived, bytesToRead);
                     bytesReceived += bytesRead;
                  }

                  //we got it all, so decode it, and send it off
                  Event e = Event.decode(ref messageBuffer);

                  bool filtered=false;
                  if(filterReceiveEvent!=null)
                  {
                     if (filterReceiveEvent(e, tcpClient) == true)
                     {
                        filtered=true;
                     }
                  }

                  if(filtered==false)
                  {
                     if(receivedEvent!=null)
                     {
                        receivedEvent(e, tcpClient);
                     }
                  }
               }
            }
            catch
            {
               //a socket error has occurred
               break;
            }

            //send any waiting messages
            double writeTime = TimeSource.currentTime() + 0.25;
            Event evt=null;
            while (myEventQueue.TryDequeue(out evt) == true && TimeSource.clockTime() < writeTime)
            {
               bool filtered=false;
               if(filterSendEvent!=null)
               {
                  filtered=filterSendEvent(evt, tcpClient);
               }

               if(filtered==false)
               {
                  try
                  {
                     if(sendingEvent!=null)
                     {
                        sendingEvent(evt, tcpClient);
                     }
                     //Debug.print("Fullfilled request for chunk: {0}", tr.chunkId);
                     byte[] msg = evt.encode();
                     clientStream.Write(msg, 0, msg.Length);
                     clientStream.Flush();
                  }
                  catch
                  {
                     //a socket error has occurred
                     break;
                  }
               }
            }

            //the client has disconnected from the server
            if (tcpClient.Connected == false)
            {
               break;
            }

            Thread.Sleep(1);
         }

         tcpClient.Close();
      }
   }

   public class TcpMessageClient
   {
      public delegate bool EventCallback(Event evt);
      public EventCallback filterReceiveEvent;
      public EventCallback filterSendEvent;
      public EventCallback receivedEvent;
      public EventCallback sendingEvent;

      String myServerAddress;
      int myPort;

      bool myTimeToStop = false;
      Thread myClientThread;

      ConcurrentQueue<Event> myEventQueue = new ConcurrentQueue<Event>();

      public TcpMessageClient(String serveraddress, int port)
      {
         myServerAddress = serveraddress;
         myPort = port;
         myClientThread = new Thread(new ThreadStart(connectToServer));
         myClientThread.Name = "TCP Message Client thread";
         myClientThread.Start();
      }

      public void shutdown()
      {
         myTimeToStop = true;
         myClientThread.Join(200);
      }

      public void send(Event e)
      {
         myEventQueue.Enqueue(e);
      }

      public void connectToServer()
      {
         using (TcpClient tcpClient = new TcpClient())
         {
            try
            {
               tcpClient.Connect(myServerAddress, myPort);
            }
            catch
            {
               Info.print("Failed to connect to server {0}:{1}", myServerAddress, myPort);
               return;
            }

            if (tcpClient.Connected == true)
            {
               Info.print("Connected to server {0}:{1}", myServerAddress, myPort);
            }
            //2MB buffers, should help with overflow conditions
            tcpClient.ReceiveBufferSize = 1024 * 1024 * 2;
            tcpClient.SendBufferSize = 1024 * 1024 * 2;

            NetworkStream clientStream = tcpClient.GetStream();

            byte[] header = new byte[4];
            byte[] tempBuffer = new byte[4096];
            byte[] messageBuffer;
            int bytesRead;

            while (myTimeToStop == false)
            {
               try
               {
                  //send events
                  Event e = null;
                  if (myEventQueue.TryDequeue(out e) == true)
                  {
                     bool filtered = false;
                     if (filterSendEvent != null)
                     {
                        filtered = filterSendEvent(e);
                     }

                     if (filtered == false)
                     {
                        try
                        {
                           if (sendingEvent != null)
                           {
                              sendingEvent(e);
                           }
                           //Debug.print("Fullfilled request for chunk: {0}", tr.chunkId);
                           byte[] msg = e.encode();
                           clientStream.Write(msg, 0, msg.Length);
                           clientStream.Flush();
                        }
                        catch
                        {
                           //a socket error has occurred
                           break;
                        }
                     }
                  }

                  //receive events
                  if (clientStream.DataAvailable == true)
                  {
                     bytesRead = 0;

                     //blocks until a client sends a message,  then just read the size of the message
                     bytesRead = clientStream.Read(header, 0, 4);
                     int msgSize = BitConverter.ToInt32(header, 0);

                     messageBuffer = new byte[msgSize];
                     int bytesReceived = bytesRead;
                     //copy the size of the message into the buffer, since this is important for decoding
                     Array.Copy(header, 0, messageBuffer, 0, bytesReceived);

                     //read the rest of this message
                     while (bytesReceived < msgSize)
                     {
                        int bytesToRead =  msgSize - bytesReceived;
                        bytesRead = clientStream.Read(messageBuffer, bytesReceived, bytesToRead);
                        bytesReceived += bytesRead;
                     }

                     //we got it all, so decode it, and send it off
                     Event evt = Event.decode(ref messageBuffer);

                     bool filtered = false;
                     if (filterReceiveEvent != null)
                     {
                        if (filterReceiveEvent(evt) == true)
                        {
                           filtered = true;
                        }
                     }

                     if (filtered == false)
                     {
                        if (receivedEvent != null)
                        {
                           receivedEvent(evt);
                        }
                     }
                  }
               }
               catch
               {
                  //a socket error has occurred
                  break;
               }

               Thread.Sleep(1);
            }

            //send the close token (Int32 max size)
            byte[] end = BitConverter.GetBytes(Int32.MaxValue);
            clientStream.Write(end, 0, 4);
            clientStream.Flush();
            clientStream.Close();
            tcpClient.Close();
         }
      }
   }
}