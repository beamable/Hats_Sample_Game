using Beamable.Common.Content;
using Beamable.Common.Inventory;

namespace Hats.Content
{
   [ContentType("character")]
   public class CharacterContent : ItemContent
   {
      public CharacterAddressableAsset Prefab;
      public string Display;
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