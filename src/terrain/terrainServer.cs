using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Terrain
{
   public class TerrainServer
   {
      TerrainCache myTerrainDb;
      TerrainGenerator myGenerator;

      TcpListener myTcpListener;
      Thread myListenerThread;

      public TerrainServer(TerrainCache db, TerrainGenerator generator)
      {
         myTerrainDb = db;
         myGenerator = generator;

         this.myTcpListener = new TcpListener(IPAddress.Any, 4567);
         this.myListenerThread = new Thread(new ThreadStart(listenForClients));
         this.myListenerThread.Start();
      }

      void listenForClients()
      {
         this.myTcpListener.Start();

         while (true)
         {
            //blocks until a client has connected to the server
            TcpClient client = this.myTcpListener.AcceptTcpClient();

            //create a thread to handle communication 
            //with connected client
            Thread clientThread = new Thread(new ParameterizedThreadStart(handleClient));
            clientThread.Start(client);
         }
      }

      void handleClient(object client)
      {
         TcpClient tcpClient = (TcpClient)client;
         NetworkStream clientStream = tcpClient.GetStream();

         byte[] message = new byte[4096];
         int bytesRead;

         while (true)
         {
            bytesRead = 0;

            try
            {
               //blocks until a client sends a message
               bytesRead = clientStream.Read(message, 0, 4096);
               int counter = 0;
               while ((bytesRead-counter) >= 8) //is there at least 8 bytes to read
               {
                  UInt64 requestedId = BitConverter.ToUInt64(message,counter);
                  byte[] data = myTerrainDb.compressedChunk(requestedId);

                  clientStream.Write(BitConverter.GetBytes(data.Length+4+8), 0, sizeof(int));  //size of payload, including the size itself
                  clientStream.Write(BitConverter.GetBytes(requestedId), 0, sizeof(UInt64));   //chunk id of the data
                  clientStream.Write(data, 0, data.Length);                                    //chunk data
                  
                  //get the next buffer
                  counter += 8;
               }
            }
            catch
            {
               //a socket error has occured
               break;
            }

            if (bytesRead == 0)
            {
               //the client has disconnected from the server
               break;
            }

            clientStream.Flush();
         }

         tcpClient.Close();
      }
   }
}