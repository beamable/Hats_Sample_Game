using System;
using System.Threading.Tasks;
using Hats.Game;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Hats.Content
{
   [Serializable]
   public class CharacterAddressableAsset : AssetReferenceGameObject
   {
      public CharacterAddressableAsset(string guid) : base(guid)
      {

      }

      #if UNITY_EDITOR
      public override bool ValidateAsset(string path)
      {
         var piece = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterBehaviour>(path);
         return piece != null;
      }
      #endif

      public async Task<CharacterBehaviour> SafeResolve()
      {
         var taskHandle = !OperationHandle.IsValid()
            ? LoadAssetAsync()
            : OperationHandle.Convert<GameObject>();
         var gob = await taskHandle.Task;
         return gob.GetComponent<CharacterBehaviour>();
      }
   }
}