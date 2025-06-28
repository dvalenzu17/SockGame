using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central manager for all power-ups. 
/// Tracks counts, active selection, and executes effects on Tiles.
/// </summary>
public enum PowerupType { None, LintRoller, Softener, Basket }

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [Header("Counts")]
    public int lintRollerCount = 3;
    public int softenerCount = 2;
    public int basketCount = 1;

    [Header("UI Buttons")]
    public Button lintButton;
    public Button softenerButton;
    public Button basketButton;

    [Header("UI Labels")]
    public TextMeshProUGUI lintCountText;
    public TextMeshProUGUI softenerCountText;
    public TextMeshProUGUI basketCountText;

    [HideInInspector]
    public PowerupType activePowerup = PowerupType.None;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Wire up the buttons
        lintButton.onClick.AddListener(() => SelectPowerup(PowerupType.LintRoller));
        softenerButton.onClick.AddListener(() => SelectPowerup(PowerupType.Softener));
        basketButton.onClick.AddListener(() => SelectPowerup(PowerupType.Basket));
        UpdateUI();
    }

    public void SelectPowerup(PowerupType type)
    {
        if (activePowerup == type)
            activePowerup = PowerupType.None;
        else
            activePowerup = type;

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Highlight selected button
        lintButton.transform.localScale = activePowerup == PowerupType.LintRoller ? Vector3.one * 1.2f : Vector3.one;
        softenerButton.transform.localScale = activePowerup == PowerupType.Softener ? Vector3.one * 1.2f : Vector3.one;
        basketButton.transform.localScale = activePowerup == PowerupType.Basket ? Vector3.one * 1.2f : Vector3.one;

        // Update counts
        lintCountText.text = lintRollerCount.ToString();
        softenerCountText.text = softenerCount.ToString();
        basketCountText.text = basketCount.ToString();

        // Disable button if no uses left
        lintButton.interactable = lintRollerCount > 0;
        softenerButton.interactable = softenerCount > 0;
        basketButton.interactable = basketCount > 0;
    }

    /// <summary>
    /// Called from InputHandler when the player taps a tile while activePowerup != None.
    /// Executes the appropriate effect, decrements count, then resets activePowerup to None.
    /// </summary>
    public void TryUsePowerup(Tile t)
    {
        if (t.isObstacle || t.isMatched) return;

        switch (activePowerup)
        {
            case PowerupType.LintRoller:
                if (lintRollerCount > 0)
                {
                    lintRollerCount--;
                    RemoveSingleTile(t);
                    AudioManager.Instance.PlayPowerup();
                }
                break;

            case PowerupType.Softener:
                if (softenerCount > 0)
                {
                    softenerCount--;
                    RemoveAllObstacles();
                    AudioManager.Instance.PlayPowerup();
                }
                break;

            case PowerupType.Basket:
                // --- NEW CHECK: prevent basket on the objective color ---
                int target = BoardManager.Instance.targetColorID;
                if (t.sockID == target)
                {
                    Debug.Log("[Powerup] Cannot use Basket on the objective color!");
                    // you could also flash the button or show a tooltip here
                }
                else if (basketCount > 0)
                {
                    basketCount--;
                    RemoveAllOfColor(t.sockID);
                    AudioManager.Instance.PlayPowerup();
                }
                break;
                
        }

        TelemetryManager.Instance.SendPowerupUsed(activePowerup.ToString());
        activePowerup = PowerupType.None;
        UpdateUI();
    }

    private void RemoveAllObstacles()
    {
        var bm = BoardManager.Instance;
        var tiles = bm.tileInstances;
        int w = tiles.GetLength(0), h = tiles.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                GameObject go = tiles[x, y];
                if (go == null) continue;
                Tile tile = go.GetComponent<Tile>();
                if (tile.isObstacle && !tile.isMatched)
                    bm.OnTilesMatched(tile, tile);
            }
        }
    }

    private void RemoveAllOfColor(int colorID)
    {
        var bm = BoardManager.Instance;
        var tiles = bm.tileInstances;
        int w = tiles.GetLength(0), h = tiles.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                GameObject go = tiles[x, y];
                if (go == null) continue;
                Tile tile = go.GetComponent<Tile>();
                if (!tile.isObstacle && !tile.isMatched && tile.sockID == colorID)
                    bm.OnTilesMatched(tile, tile);
            }
        }
    }

    private void RemoveSingleTile(Tile t)
    {
        // same pattern: mark and pop
        BoardManager.Instance.OnTilesMatched(t, t);
    }
}
