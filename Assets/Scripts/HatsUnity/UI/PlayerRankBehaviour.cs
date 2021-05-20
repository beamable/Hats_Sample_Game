using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Experimental.Api.Sim;
using Beamable.UI.Scripts;
using HatsCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRankBehaviour : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI RankText;
    public TextMeshProUGUI AliasText;
    public Image CharacterImage;
    public Image HatImage;

    public VertexGradient GlowGradient;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public async Task Set(HatsPlayer player, PlayerResult result)
    {
        var selectedHatRef = await player.GetSelectedHat();
        var selectedHat = await selectedHatRef.Resolve();
        var hatSprite = await selectedHat.icon.LoadSprite();

        var selectedCharacterRef = await player.GetSelectedCharacter();
        var selectedCharacter = await selectedCharacterRef.Resolve();
        var characterSprite = await selectedCharacter.icon.LoadSprite();

        HatImage.sprite = hatSprite;
        CharacterImage.sprite = characterSprite;
        RankText.text = result.rank.ToString();
        AliasText.text = await player.GetPlayerAlias();

    }

    public void Glow()
    {
        // RankText.colo
        RankText.colorGradient = GlowGradient;
        AliasText.colorGradient = GlowGradient;
    }
}
