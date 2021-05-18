using System;
using System.Collections.Generic;
using Beamable.Experimental.Api.Sim;
using HatsCore;
using UnityEngine;
namespace HatsMultiplayer
{
   public class MultiplayerGameDriver : MonoBehaviour
   {
      public const string MSG_PLAYER_ACTION = "msgPlayerAction";

      private SimClient _sim;

      private Queue<HatsGameMessage> _messageQueue = new Queue<HatsGameMessage>();


      private List<long> _playerDbids;

      public Queue<HatsGameMessage> Init(string roomId, int framesPerSecond, List<long> playerDbids)
      {
         _playerDbids = playerDbids;
         Debug.Log("Setting up sim client");
         _sim = new SimClient(new SimNetworkEventStream(roomId), framesPerSecond, 4);


         _sim.OnInit(HandleOnInit);
         _sim.OnConnect(HandleOnConnect);
         _sim.OnDisconnect(HandleOnDisconnect);
         _sim.OnTick(HandleOnTick);


         return _messageQueue;
      }

      private void Update()
      {
         _sim?.Update();
      }

      private void HandleOnInit(string seed)
      {
         Debug.Log("Sim client has initialized " + seed);
         // foreach (var playerDbid in _playerDbids)
         // {
         //    SetupNetworkedPlayer(playerDbid);
         // }
      }
      private void HandleOnTick(long tick)
      {
         // Debug.Log("Sim client is ticking " + tick);
         _messageQueue.Enqueue(new HatsTickMessage
         {
            FrameNumber = tick
         });
      }
      private void HandleOnConnect(string dbid)
      {
         Debug.Log("Sim client has connection from " + dbid);

         SetupNetworkedPlayer(dbid);
      }
      private void HandleOnDisconnect(string dbid)
      {
         Debug.Log("Sim client has disconnection from " + dbid);
      }
      private void SetupNetworkedPlayer(string dbid)
      {
         Debug.Log("Setting up listener for " + dbid);
         _sim.On<HatsPlayerMove>(MSG_PLAYER_ACTION, dbid.ToString(), move =>
         {
            // enqueue this message to be processed by the Hats Game manager.
            Debug.Log($"received event from=[{dbid}] move=[{move}]");
            _messageQueue.Enqueue(move);

         });
      }

      public void DeclareLocalPlayerReady()
      {
         throw new NotImplementedException();
      }

      public void DeclareLocalPlayerAction(HatsPlayerMove move)
      {
         Debug.Log("sending event. " + move);
         _sim.SendEvent(MSG_PLAYER_ACTION, move);

      }

   }
}