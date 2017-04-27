using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using OpenTK;

using Util;
using Network;
using Events;

namespace Terrain
{
   public class TerrainClient
   {
      TcpMessageClient myTcpClient;
      bool myReset = false;

      ConcurrentQueue<TerrainResponseEvent> myReturnedChunks = new ConcurrentQueue<TerrainResponseEvent>();

      public TerrainClient(String serveraddress, int port)
      {
         myTcpClient = new TcpMessageClient(serveraddress, port);

         myTcpClient.receivedEvent += new TcpMessageClient.EventCallback(handleMessage);
      }

      bool handleMessage(Event e)
      {
         if (e is TerrainResponseEvent)
         {
            myReturnedChunks.Enqueue(e as TerrainResponseEvent);
         }

         return true;
      }

      public void shutdown()
      {
         myTcpClient.shutdown();
      }

      public TerrainResponseEvent nextResponse()
      {
         TerrainResponseEvent c = null;
         myReturnedChunks.TryDequeue(out c);
         return c;
      }

      public void requestChunk(UInt64 id)
      {
         myTcpClient.send(new TerrainRequestEvent(id));
      }

      public void rebuildChunk(UInt64 id)
      {
         myTcpClient.send(new TerrainRebuildEvent(id));
      }

      public void reset()
      {
         myTcpClient.send(new TerrainResetEvent());
      }
   }
}