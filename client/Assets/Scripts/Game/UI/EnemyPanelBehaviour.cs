using Beamable;
using System.Threading.Tasks;
using Hats.Simulation;
using Hats.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyPanelBehaviour : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI AliasText;

    public Image AvatarImage;

    [ReadOnly]
    [SerializeField]
    private PlayerStats _stats;

    private BeamContext _beamContext;
    
    public void Clear()
    {
        AvatarImage.gameObject.SetActive(false);
        AliasText.gameObject.SetActive(false);
    }

    public async Task SetForPlayer(long dbid)
    {
        _beamContext = BeamContext.Default;
        await _beamContext.OnReady;

        var stats = await _beamContext.Api.StatsService.GetStats("client", "public", "player", dbid);
        AliasText.text = stats["alias"];

        // TODO: somehow fetch the equipped inventory and apply the avatar information...
        AvatarImage.gameObject.SetActive(true);
        AliasText.gameObject.SetActive(true);
    }

    public void SetForPlayer(PlayerStats stats)
    {
        _stats = stats;
        AliasText.text = stats.Alias;
        AvatarImage.gameObject.SetActive(true);
        AliasText.gameObject.SetActive(true);
    }
}
