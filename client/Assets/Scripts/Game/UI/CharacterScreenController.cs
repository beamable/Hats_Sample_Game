using Hats.Game;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScreenController : MonoBehaviour
{

    [Header("UI References")]
    public Button MenuButton;

    public Button BuyMoreGemsButton;
    public GemStoreController GemStoreController;

    // Start is called before the first frame update
    void Start()
    {
        MenuButton.onClick.AddListener(HandleMenu);
        BuyMoreGemsButton.onClick.AddListener(HandleGemClick);
    }

    void HandleGemClick()
    {
        GemStoreController.IsOpen = true;
    }

    void HandleMenu()
    {
        HatsScenes.LoadMatchmaking();
    }
}
