using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using UnityEngine;

namespace HatsContent
{
   [ContentType("character")]
   public class CharacterContent : ItemContent
   {
      public CharacterAddressableAsset Prefab;
      public string Display;
      // TODO: add fireball, arrow, and shield specific FX ?

      // private Promise<GameObject> _loadPromise;

      // public Promise<GameObject> LoadPromise => _loadPromise ??= (!Prefab.OperationHandle.IsValid()
      //       ? Prefab.LoadAssetAsync()
      //       : Prefab.OperationHandle.Convert<GameObject>()).Task
      //    .ToPromise();
   }

   [System.Serializable]
   public class CharacterRef : ItemRef<CharacterContent>
   {
      public CharacterRef(){}

      public CharacterRef(string id)
      {
         Id = id;
      }
   }
}