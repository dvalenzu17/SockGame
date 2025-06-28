using UnityEngine;
using static PowerupManager;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    [SerializeField] private float dragThreshold = 30f;

    private Vector2 dragStartPos;
    private bool isDragging;
    private Tile startTile;

    private Camera cam;
    private BoardManager boardManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cam = Camera.main;
        boardManager = FindAnyObjectByType<BoardManager>();
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = false;
            startTile = GetTileUnder(dragStartPos);
            if (startTile != null)
                Debug.Log($"[Input] Drag started on tile ({startTile.GridX},{startTile.GridY}) sockID={startTile.sockID}");
        }

        if (Input.GetMouseButton(0) && !isDragging &&
            Vector2.Distance(dragStartPos, Input.mousePosition) > dragThreshold)
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging && startTile != null)
            {
                // perform a swipe swap
                ProcessSwipe(dragStartPos, Input.mousePosition);
            }
            else if (!isDragging && startTile != null)
            {
                // **not dragging => treat as tap for power-up use**
                if (PowerupManager.Instance.activePowerup != PowerupType.None)
                {
                    Debug.Log($"[Input] Using powerup {PowerupManager.Instance.activePowerup} on ({startTile.GridX},{startTile.GridY})");
                    PowerupManager.Instance.TryUsePowerup(startTile);
                }
            }

            // reset for next input
            startTile = null;
            isDragging = false;
        }
    }


    private void HandleTouch()
    {
        if (Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);

        switch (t.phase)
        {
            case TouchPhase.Began:
                dragStartPos = t.position;
                isDragging = false;
                startTile = GetTileUnder(dragStartPos);
                if (startTile != null)
                    Debug.Log($"[Input] Drag started on tile ({startTile.GridX},{startTile.GridY}) sockID={startTile.sockID}");
                break;

            case TouchPhase.Moved:
                if (!isDragging && Vector2.Distance(dragStartPos, t.position) > dragThreshold)
                    isDragging = true;
                break;

            case TouchPhase.Ended:
                if (isDragging && startTile != null)
                {
                    ProcessSwipe(dragStartPos, t.position);
                }
                else if (!isDragging && startTile != null)
                {
                    if (PowerupManager.Instance.activePowerup != PowerupType.None)
                    {
                        Debug.Log($"[Input] Using powerup {PowerupManager.Instance.activePowerup} on ({startTile.GridX},{startTile.GridY})");
                        PowerupManager.Instance.TryUsePowerup(startTile);
                    }
                }
                startTile = null;
                isDragging = false;
                break;
        }
    }


    private void ProcessSwipe(Vector2 start, Vector2 end)
    {
        if (startTile == null) return;
        if (startTile.isMatched)
        {
            Debug.Log("[Input] startTile already matched—ignoring swipe");
            startTile = null;
            return;
        }
        Vector2 delta = end - start;
        if (delta.magnitude < dragThreshold) return;
        delta.Normalize();

        Vector2Int dir;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            dir = (delta.x > 0) ? Vector2Int.right : Vector2Int.left;
        else
            dir = (delta.y > 0) ? Vector2Int.up : Vector2Int.down;

        Debug.Log($"[Input] Swiped {dir} on startTile sockID={startTile.sockID}");
        boardManager.TrySwap(startTile, dir);


    }

    private Tile GetTileUnder(Vector2 screenPos)
    {
        // convert to world point on Z=0 plane
        Vector3 wp3 = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        Vector2 wp2 = wp3;
        Collider2D col = Physics2D.OverlapPoint(wp2);
        if (col != null && col.CompareTag("Tile"))
            return col.GetComponent<Tile>();
        return null;
    }
}
