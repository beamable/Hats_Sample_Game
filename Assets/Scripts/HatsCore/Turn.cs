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

      public void CopyStateFromTurn(Turn other)
      {
         PlayerState.Clear();
         foreach (var kvp in other.PlayerState)
         {
            PlayerState[kvp.Key] = kvp.Value.Clone();
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