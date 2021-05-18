using System;
using System.Threading.Tasks;
using HatsUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HatsContent
{
   [Serializable]
   public class CharacterAddressableAsset : AssetReferenceGameObject
   {
      public CharacterAddressableAsset(string guid) : base(guid)
      {

      }
      public override bool ValidateAsset(string path)
      {
         var piece = AssetDatabase.LoadAssetAtPath<CharacterBehaviour>(path);
         return piece != null;
      }

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