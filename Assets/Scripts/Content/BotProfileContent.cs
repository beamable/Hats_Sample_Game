using System;
using System.Collections.Generic;
using Beamable.Common.Content;

namespace Hats.Content
{
   [ContentType("botprofile")]
   public class BotProfileContent : ContentObject
   {
      public List<string> names;
      public List<RandomBotAIWeights> weights;
   }

   [Serializable]
   public class RandomBotAIWeights
   {
      public string name;
      public float walkWeight;
      public float fireballWeight;
      public float arrowWeight;
      public float shieldWeight;
      public float skipWeight;
   }

   [Serializable]
   public class BotProfileRef : ContentRef<BotProfileContent> {}
}