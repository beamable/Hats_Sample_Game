using System.Collections.Generic;
using UnityEngine;

namespace HatsCore
{
   public class Turn
   {
      public int TurnNumber;
      public Dictionary<long, HatsPlayerMove> Moves = new Dictionary<long, HatsPlayerMove>();

      public Dictionary<long, HatsPlayerState> PlayerState = new Dictionary<long, HatsPlayerState>();

      public int CommittedMoves => Moves.Count;

      public HatsPlayerState GetPlayerState(long dbid)
      {
         if (!PlayerState.TryGetValue(dbid, out var state))
         {
            state = new HatsPlayerState();
            PlayerState[dbid] = state;
         }

         return state;
      }

      public bool IsPlayerDead(long dbid) => GetPlayerState(dbid).IsDead;

      public bool IsPlayerShielding(long dbid) => GetPlayerState(dbid).IsShield;

      public List<long> GetAlivePlayersAtPosition(Vector3Int position)
      {
         var players = new List<long>();
         foreach (var kvp in PlayerState)
         {
            if (!kvp.Value.IsDead && kvp.Value.Position == position)
            {
               players.Add(kvp.Key);
            }
         }

         return players;
      }

      public void DisableAllShields()
      {
         foreach (var kvp in PlayerState)
         {
            kvp.Value.IsShield = false;
         }
      }

      public void ActivateShieldForPlayer(long dbid)
      {
         GetPlayerState(dbid).IsShield = true;
      }

      public void CopyStateFromTurn(Turn other)
      {
         PlayerState.Clear();
         foreach (var kvp in other.PlayerState)
         {
            var nextState = kvp.Value.Clone();
            PlayerState[kvp.Key] = nextState;
         }
      }

      public void CommitMove(HatsPlayerMove move)
      {
         if (Moves.ContainsKey(move.Dbid))
         {
            Debug.LogWarning($"Player {move.Dbid} has already committed a move for turn {TurnNumber}");
            return;
         }

         Moves[move.Dbid] = move;
      }
   }
}