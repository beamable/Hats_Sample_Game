using Beamable.Common.Content;
using Beamable.Common.Inventory;
using UnityEngine;

namespace Hats.Content
{
   [ContentType("hat")]
   public class HatContent : ItemContent
   {
      public string Display;
      public Vector2 Offset;
      public float Rotation;
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