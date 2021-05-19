using Beamable.Common.Content;
using Beamable.Common.Inventory;

namespace HatsContent
{
   [ContentType("hat")]
   public class HatContent : ItemContent
   {
      public string Display;
   }

   [System.Serializable]
   public class HatRef : ItemRef<HatContent>
   {
      public HatRef()
      {

      }

      public HatRef(string id)
      {
         Id = id;
      }
   }
}