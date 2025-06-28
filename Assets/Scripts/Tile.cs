using UnityEngine;
using System.Collections;

/// <summary>
/// Tile represents a single grid cell: sock or obstacle. 
/// All input (taps/swipes) is handled by InputHandler.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Tile : MonoBehaviour
{
    [HideInInspector] public int sockID;        // 0..7 for socks; -1 for obstacles
    [HideInInspector] public bool isObstacle;   // true if obstacle
    [HideInInspector] public bool isMatched;    // true once it's been matched/removed

    [HideInInspector] public int GridX;         // set by BoardManager
    [HideInInspector] public int GridY;         // set by BoardManager

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Call immediately after Instantiate() from BoardManager.
    /// </summary>
    public void Initialize(int x, int y, int sockID, bool isObstacle, Sprite[] sockSprites, Sprite obstacleSprite)
    {
        GridX = x;
        GridY = y;
        this.sockID = sockID;
        this.isObstacle = isObstacle;
        this.isMatched = false;

        if (isObstacle)
        {
            spriteRenderer.sprite = obstacleSprite;
            col.enabled = false;
        }
        else
        {
            if (sockSprites != null && sockID >= 0 && sockID < sockSprites.Length)
                spriteRenderer.sprite = sockSprites[sockID];
            else
                Debug.LogWarning($"Tile.Initialize: invalid sockID {sockID}");

            col.enabled = true;
        }

        // reset transform & color
        transform.localScale = Vector3.one;
        spriteRenderer.color = Color.white;
    }

    /// <summary>
    /// Trigger the pop+fade animation when matched.
    /// </summary>
    public void PlayMatchAnimation()
    {
        if (!isMatched) StartCoroutine(PopAndFadeOut());
    }

    private IEnumerator PopAndFadeOut()
    {
        isMatched = true;

        const float duration = 0.2f;
        Vector3 origScale = transform.localScale;
        Vector3 targetScale = origScale * 1.3f;
        float t = 0f;

        // scale up
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(origScale, targetScale, t / duration);
            yield return null;
        }

        // fade out
        t = 0f;
        Color start = spriteRenderer.color;
        Color end = new Color(start.r, start.g, start.b, 0f);
        while (t < duration)
        {
            t += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(start, end, t / duration);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
