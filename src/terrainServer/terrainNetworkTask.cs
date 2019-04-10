using System;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Engine;
using Terrain;
using Util;
using Network;

namespace TerrainServer
{
   public class TerrainNetworkTask : Task
   {
      TcpMessageServer myServer;
      int responses;
      int requests;

      ConcurrentDictionary<TcpClient, HashSet<UInt64>> myClientInterest = new ConcurrentDictionary<TcpClient, HashSet<UInt64>>();

      public TerrainNetworkTask(Initializer init)
         : base("Terrain Server")
      {
         //register for events
         Application.eventManager.addListener(handleTerrainResponse, "terrain.chunk.response");

         //setup socket
         int port=init.findDataOr("terrainServer.port", 2377);
         myServer=new TcpMessageServer(port);


         //add message callbacks
         myServer.onClientConnected += new TcpMessageServer.ClientConnected(clientConnected);
         myServer.filterReceiveEvent += new TcpMessageServer.EventCallback(filterReceiveMessages);
         myServer.filterSendEvent += new TcpMessageServer.EventCallback(filterSendMessages);
         myServer.receivedEvent += new TcpMessageServer.EventCallback(
            delegate(Event e, TcpClient client)
            {
               Application.eventManager.queueEvent(e); 
               return true; 
            });

         frequency = 1;
      }

      void clientConnected(TcpClient client)
      {
         myClientInterest[client] = new HashSet<UInt64>();
      }

      bool filterReceiveMessages(Event e, TcpClient client)
      {
         if (e is TerrainRequestEvent)
         {
            TerrainRequestEvent tr = e as TerrainRequestEvent;
            myClientInterest[client].Add(tr.chunkId);
            requests++;
            return false;
         }
         if (e is TerrainRebuildEvent)
         {
            TerrainRebuildEvent tr = e as TerrainRebuildEvent;
            myClientInterest[client].Add(tr.chunkId);
            requests++;
            return false;
         }
         if (e is TerrainResetEvent) return true;

         return true;
      }

      bool filterSendMessages(Event e, TcpClient client)
      {
         if (e is TerrainResponseEvent)
         {
            TerrainResponseEvent tr = e as TerrainResponseEvent;
            HashSet<UInt64> myRequests = myClientInterest[client];
            if (myRequests.Contains(tr.chunkId))
            {
               responses++;
               return false;
            }
            else
            {
               return true;
            }
         }

         return true;
      }

      public override void onUpdate(double dt)
      {
         Console.Write("\rRequests: {0}  Responses: {1}              ", requests, responses);
      }

      public void shutdown()
      {
         myServer.shutdown();
      }

      EventManager.EventResult handleTerrainResponse(Event e)
      {
         TerrainResponseEvent tr = e as TerrainResponseEvent;
         if (tr != null)
         {
            myServer.send(tr);

            return EventManager.EventResult.HANDLED;
         }

         return EventManager.EventResult.IGNORED;
      }
   }
}