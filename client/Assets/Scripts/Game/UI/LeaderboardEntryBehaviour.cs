using System.Globalization;
using Beamable.Common.Api.Leaderboards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
   public class LeaderboardEntryBehaviour : MonoBehaviour
   {
      [Header("UI References")]
      public TextMeshProUGUI RankText;
      public TextMeshProUGUI AliasText;
      public TextMeshProUGUI ScoreText;
      public Image CharacterImage;
      public Image HatImage;

      public VertexGradient GlowGradient;

      public void Set(string alias, Sprite characterSprite, RankEntry rank)
      {
         AliasText.text = alias;
         RankText.text = rank.rank.ToString();
         ScoreText.text = rank.score.ToString(CultureInfo.InvariantCulture);
         CharacterImage.sprite = characterSprite;
      }

      public void SetForSelf()
      {
         RankText.colorGradient = GlowGradient;
         ScoreText.colorGradient = GlowGradient;
         AliasText.colorGradient = GlowGradient;
      }
   }
}