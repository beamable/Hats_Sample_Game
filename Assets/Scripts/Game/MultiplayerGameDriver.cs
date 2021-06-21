using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Experimental.Api.Sim;
using Hats.Simulation;
using UnityEngine;

namespace Hats.Game
{
	public class MultiplayerGameDriver : MonoBehaviour
	{
		public const string MSG_PLAYER_ACTION = "msgPlayerAction";

		private SimClient _sim;

		private Queue<HatsGameMessage> _messageQueue = new Queue<HatsGameMessage>();

		private string _roomId;

		private long _localPlayerDbid;

		public long LocalPlayerDBID { get; private set; }

		public Queue<HatsGameMessage> Init(string roomId, int framesPerSecond, List<long> playerDbids)
		{
			_roomId = roomId;
			Debug.Log("Setting up sim client");
			_sim = new SimClient(new SimNetworkEventStream(roomId), framesPerSecond, 4);
			LocalPlayerDBID = long.Parse(_sim.ClientId);
			Debug.Log($"Local player DBID={LocalPlayerDBID}");

			_sim.OnInit(HandleOnInit);
			_sim.OnConnect(HandleOnConnect);
			_sim.OnDisconnect(HandleOnDisconnect);
			_sim.OnTick(HandleOnTick);

			return _messageQueue;
		}

		public async Task<GameResults> DeclareResults(List<PlayerResult> results)
		{
			var beamable = await Beamable.API.Instance;
			// strip out AI players
			var strippedResults = results.Where(result => result.playerId >= 0).ToList();
			return await beamable.Experimental.GameRelayService.ReportResults(_roomId, strippedResults.ToArray());
		}

		public void DeclareLocalPlayerAction(HatsPlayerMove move)
		{
			Debug.Log("sending event. " + move);
			_sim.SendEvent(MSG_PLAYER_ACTION, move);
		}

		private void FixedUpdate()
		{
			_sim?.Update();
		}

		private void HandleOnInit(string seed)
		{
			Debug.Log("Sim client has initialized " + seed);
		}

		private void HandleOnTick(long tick)
		{
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
			_messageQueue.Enqueue(new HatsPlayerDropped
			{
				Dbid = long.Parse(dbid)
			});
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
	}
}