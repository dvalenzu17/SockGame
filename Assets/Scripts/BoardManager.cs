using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Replaces your existing BoardManager. 
/// - Exposes sockSprites[ ] (8 total) and obstacleSprites[ ] (9 total).
/// - Computes an origin so that the board is centered in world space.
/// - Passes a random obstacle sprite to each obstacle tile.
/// </summary>
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Prefabs & References")]
    public GameObject tilePrefab;               // Prefab with a Tile.cs component
    public Transform boardParent;               // Empty GameObject under which to instantiate tiles
    private float originX, originY;

    [Header("Sprites")]
    [Tooltip("Assign exactly 8 sock sprites here (index 0..7).")]
    public Sprite[] sockSprites;                // length = 8
    [Tooltip("Assign exactly 9 obstacle sprites here.")]
    public Sprite[] obstacleSprites;            // length = 9

    [Header("Board Settings")]
    [Tooltip("Spacing between adjacent tiles in Unity units.")]
    public float tileSpacing = 1f;              // adjust if your tile art is not exactly 1×1

    [Header("Level Control")]
    public int levelIndex = 1;                  // Set this before loading the LevelScene
    public int BASE_POINT_PER_MATCH = 50;

    [Header("UI References")]
    public GameObject movesPanel;
    public TextMeshProUGUI movesText;
    public Image objectiveIcon;
    public TextMeshProUGUI objectiveCount;
    public GameObject powerupsPanel;

    [Header("Win/Lose Panels")]
    public UIPanelManager uIPanelManager;
    public LevelObjective currentObjective;  


    // Internal state
    private LevelConfig cfg;
    private BoardGenerator.CellData[,] boardData;
    public GameObject[,] tileInstances;
    private int movesLeft;
    private int pairsRemaining;
    private int score;
    private bool isShuffling = false;
    private float hintDelay = 5f;
    private float lastActionTime;
    private bool hintActive;

    [Header("Objectives & Scoring")]
    [HideInInspector]
    public int targetColorID;
    private int targetMatches;
    private int moveLimit; 
    private int star1Threshold;
    private int star2Threshold;
    private int star3Threshold;
    private int matchedCount;   // <<< declare it here
    private const int basePointsPerMatch = 50;
    private const int moveBonusPoints = 100;
    private int totalMatches;
    private int currentScore;
    private int comboCount;
    private const int basePairScore = 100;
    private const float comboBonusPct = 0.25f;



    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        // Always pull the “NextLevel” you set when the player tapped “Continue”
        levelIndex = PlayerPrefs.GetInt("NextLevel", 1);

        // find UIPanelManager if not assigned
        if (uIPanelManager == null)
            uIPanelManager = FindAnyObjectByType<UIPanelManager>();
    }

    private void Start()
    {
        // 1) Build objective
        var obj = LevelObjectiveGenerator.GetObjective(levelIndex, 30);
        targetColorID = obj.targetColorID;
        targetMatches = obj.targetMatches;
        star1Threshold = obj.star1;
        star2Threshold = obj.star2;
        star3Threshold = obj.star3;

        // 2) Init counters
        matchedCount = 0;
        totalMatches = 0;
        movesLeft = obj.moveLimit;
        lastActionTime = Time.time;
        hintActive = false;
        currentScore = 0;
        comboCount = 0;

        // 3) Show moves/objective panel
        movesPanel.SetActive(true);
        UpdateUI();

        // 4) Generate & spawn board…
        cfg = LevelConfigFetcher.GetConfig(levelIndex);
        boardData = BoardGenerator.Generate(cfg);
        InstantiateAndCenterBoard(boardData, cfg);
        TelemetryManager.Instance.SendLevelStart(levelIndex);
        currentObjective = LevelObjectiveGenerator.GetObjective(levelIndex, BASE_POINT_PER_MATCH);

        movesLeft = currentObjective.moveLimit;
    }
    private void Update()
    {
        if (!hintActive && Time.time - lastActionTime >= hintDelay)
            StartCoroutine(ShowHint());
    }
    private void NotifyPlayerAction()
    {
        lastActionTime = Time.time;
        hintActive = false;
    }

    private void InstantiateAndCenterBoard(BoardGenerator.CellData[,] boardData, LevelConfig cfg)
    {
        int w = cfg.gridWidth;
        int h = cfg.gridHeight;

        // Calculate world offsets for centering the board
        float boardWidthWorld = w * tileSpacing;
        float boardHeightWorld = h * tileSpacing;
        originX = -boardWidthWorld / 2f + tileSpacing / 2f;
        originY = -boardHeightWorld / 2f + tileSpacing / 2f;

        tileInstances = new GameObject[w, h];

        // Loop through each grid position to spawn tiles
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector3 pos = new Vector3(
                    originX + x * tileSpacing,
                    originY + y * tileSpacing,
                    0f
                );

                // Instantiate the tile prefab at the calculated position
                GameObject go = Instantiate(tilePrefab, pos, Quaternion.identity, boardParent);
                Tile tile = go.GetComponent<Tile>();
                var cell = boardData[x, y]; // Get board data for this tile

                if (cell.isObstacle)
                {
                    // Set the tile as an obstacle if necessary
                    var obs = obstacleSprites[Random.Range(0, obstacleSprites.Length)];
                    tile.Initialize(x, y, -1, true, sockSprites, obs); // -1 for obstacle
                }
                else
                {
                    // Initialize the tile with a valid sock ID if not an obstacle
                    tile.Initialize(x, y, cell.sockID, false, sockSprites, null); // Normal sock tile
                }

                // Store the tile in tileInstances array for reference
                tileInstances[x, y] = go;
            }
        }
    }



    public bool CheckForMatch(Tile a, Tile b)
    {
        if (a.sockID == b.sockID)
        {
            Debug.Log($"[Board] CheckForMatch: MATCH sockID={a.sockID}");
            a.PlayMatchAnimation();
            b.PlayMatchAnimation();
            OnTilesMatched(a, b);
            return true;
        }
        Debug.Log($"[Board] CheckForMatch: NO MATCH (a={a.sockID}, b={b.sockID})");
        return false;
    }

    private void UpdateUI()
    {
        // Display remaining moves
        if (movesText != null)
            movesText.text = $"Moves: {movesLeft}";

        // Display the target sock color icon
        if (objectiveIcon != null)
            objectiveIcon.sprite = sockSprites[currentObjective.targetColorID];

        // Display the progress of the objective (how many socks are matched vs required)
        if (objectiveCount != null)
            objectiveCount.text = $"{currentObjective.targetMatches}";
    }



    private IEnumerator DogShuffleRoutine()
    {
        while (pairsRemaining > 0 && movesLeft > 0)
        {
            yield return new WaitForSeconds(cfg.dogShuffleInterval);
            if (pairsRemaining > 0 && !isShuffling)
                StartCoroutine(ShuffleBoard());
        }
    }

    private IEnumerator ShuffleBoard()
    {
        isShuffling = true;

        int w = cfg.gridWidth;
        int h = cfg.gridHeight;

        // Gather all flat indices of active (non-obstacle, non-matched) tiles
        List<int> activeIndices = new List<int>();
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Tile t = tileInstances[x, y].GetComponent<Tile>();
                if (t.isObstacle || t.isMatched) continue;
                activeIndices.Add(x * w + y);
            }
        }

        // Shuffle the flat list
        for (int i = activeIndices.Count - 1; i > 0; i--)
        {
            int swapIdx = Random.Range(0, i + 1);
            int tmp = activeIndices[i];
            activeIndices[i] = activeIndices[swapIdx];
            activeIndices[swapIdx] = tmp;
        }

        // Reassign boardData and swap GameObject positions
        for (int i = 0; i < activeIndices.Count; i++)
        {
            int flatA = activeIndices[i];
            int xA = flatA / w;
            int yA = flatA % w;

            int flatB = activeIndices[(i + 1) % activeIndices.Count];
            int xB = flatB / w;
            int yB = flatB % w;

            // Swap sockIDs in boardData
            int tempSock = boardData[xA, yA].sockID;
            boardData[xA, yA].sockID = boardData[xB, yB].sockID;
            boardData[xB, yB].sockID = tempSock;

            // Swap matched/obstacle flags if needed
            bool tempObs = boardData[xA, yA].isObstacle;
            boardData[xA, yA].isObstacle = boardData[xB, yB].isObstacle;
            boardData[xB, yB].isObstacle = tempObs;

            // Swap positions of the actual GameObjects
            GameObject goA = tileInstances[xA, yA];
            GameObject goB = tileInstances[xB, yB];
            Vector3 posA = goA.transform.position;
            Vector3 posB = goB.transform.position;
            goA.transform.position = posB;
            goB.transform.position = posA;

            // Swap references in tileInstances[,]
            tileInstances[xA, yA] = goB;
            tileInstances[xB, yB] = goA;
        }

        // (Optional) play a brief shake or dog animation here

        yield return new WaitForSeconds(0.1f);
        isShuffling = false;
    }

    public void OnTilesMatched(Tile a, Tile b)
    {
        // Check if the two tiles match
        if (a.sockID == b.sockID)
        {
            // Play the match animation on both tiles
            a.PlayMatchAnimation();
            b.PlayMatchAnimation();
            AudioManager.Instance.PlayMatch();

            // Update matched count for objective tracking
            if (a.sockID == currentObjective.targetColorID)
            {
                currentObjective.matchedCount++;  // Increment the matched count if it's the target sock
                Debug.Log($"Objective socks matched: {currentObjective.matchedCount}/{currentObjective.targetMatches}");
            }

            // Calculate score based on combo and matches
            int ptsGained = Mathf.RoundToInt(basePairScore * (1f + comboCount * comboBonusPct));
            currentScore += ptsGained;
            comboCount++;  // Increase combo count for subsequent matches

            // Update UI and check for win condition
            UpdateUI();
            if (currentObjective.matchedCount >= currentObjective.targetMatches)
            {
                WinLevel();
            }
            else if (movesLeft <= 0)
            {
                FailLevel();
            }

            // Decrease moves left after a match
            movesLeft--;
        }
        else
        {
            Debug.Log("[Board] No match.");
        }
    }


    private void WinLevel()
    {
        TelemetryManager.Instance.SendLevelComplete(
        levelIndex,
        movesLeft,
        currentScore
    );
        // 0) Unlock the next level in PlayerPrefs
        int unlocked = PlayerPrefs.GetInt("LevelUnlocked", 1);
        int toUnlock = levelIndex + 1;
        if (toUnlock > unlocked)
        {
            PlayerPrefs.SetInt("LevelUnlocked", toUnlock);
            PlayerPrefs.Save();
            Debug.Log($"[Board] Unlocked level {toUnlock}");
        }

        // 1) Hide gameplay UI
        movesPanel.SetActive(false);
        powerupsPanel.SetActive(false);

        // 2) Calculate final score & stars (existing code)…
        int score = totalMatches * basePointsPerMatch + movesLeft * moveBonusPoints;
        int stars = (currentScore >= star3Threshold) ? 3
               : (currentScore >= star2Threshold) ? 2 : 1;

        Debug.Log($"[Board] Win! score={currentScore}, stars={stars}");
        
        uIPanelManager.ShowWinPanel(levelIndex, stars, currentScore);
    }

    private void FailLevel()
    {
        TelemetryManager.Instance.SendLevelFail(
        levelIndex,
        matchedCount
    );
        movesPanel.SetActive(false);
        powerupsPanel.SetActive(false);

        int loseScore = matchedCount;
        Debug.Log($"[Board] Fail: matched {matchedCount} target socks → loseScore={loseScore}");

        // Show the Lose panel with that score
        uIPanelManager.ShowLosePanel(levelIndex, currentScore);
    }

    // Called by the “Retry” button on failPanel
    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called by “Next Level” button on winPanel
    public void GoToNextLevel()
    {
        int next = levelIndex + 1;
        PlayerPrefs.SetInt("NextLevel", next);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void TrySwap(Tile a, Vector2Int dir)
    {
        comboCount = 0;
        NotifyPlayerAction();

        if (a.isMatched || a.isObstacle) return;
        int x = a.GridX, y = a.GridY;
        int nx = x + dir.x, ny = y + dir.y;
        if (nx < 0 || nx >= cfg.gridWidth || ny < 0 || ny >= cfg.gridHeight) return;

        Tile b = tileInstances[nx, ny].GetComponent<Tile>();
        if (b.isMatched || b.isObstacle) return;

        Debug.Log($"[Swap] Swapping ({x},{y},id={a.sockID}) ↔ ({nx},{ny},id={b.sockID})");
        StartCoroutine(SwapRoutine(a, b));
        AudioManager.Instance.PlaySwap();
    }



    private IEnumerator SwapRoutine(Tile a, Tile b)
    {
        // record
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        // 1) Animate swap with ease-out
        float duration = 0.2f, t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float f = t / duration;
            float eased = Mathf.SmoothStep(0f, 1f, f);
            a.transform.position = Vector3.Lerp(posA, posB, eased);
            b.transform.position = Vector3.Lerp(posB, posA, eased);
            yield return null;
        }
        a.transform.position = posB;
        b.transform.position = posA;

        // update coords & instances…
        // (your existing code fixing GridX/Y, tileInstances[,] & boardData…)

        // swap IDs & sprites
        int idA = a.sockID, idB = b.sockID;
        a.sockID = idB; b.sockID = idA;
        UpdateSprite(a);
        UpdateSprite(b);

        // 2) Check for match
        if (a.sockID == b.sockID)
        {
            Debug.Log("[Board] MATCH");
            OnTilesMatched(a, b);
        }
        else
        {
            Debug.Log("[Board] NO MATCH – Shake & Revert");

            // 3) Invalid-swap shake
            yield return StartCoroutine(Shake(a.gameObject, 0.1f, tileSpacing * 0.2f));
            yield return StartCoroutine(Shake(b.gameObject, 0.1f, tileSpacing * 0.2f));

            // brief pause
            yield return new WaitForSeconds(0.1f);

            // 4) Revert with ease-out
            t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float f2 = t / duration;
                float eased2 = Mathf.SmoothStep(0f, 1f, f2);
                a.transform.position = Vector3.Lerp(posB, posA, eased2);
                b.transform.position = Vector3.Lerp(posA, posB, eased2);
                yield return null;
            }
            a.transform.position = posA;
            b.transform.position = posB;

            // restore IDs & sprites & data
            a.sockID = idA; b.sockID = idB;
            UpdateSprite(a);
            UpdateSprite(b);

            // update UI so nothing hangs
            UpdateUI();
        }
    }

    private void UpdateSprite(Tile t)
    {
        t.GetComponent<SpriteRenderer>().sprite = sockSprites[t.sockID];
    }
    private IEnumerator AnimateDrop(GameObject go, Vector3 targetPos)
    {
        Vector3 startPos = go.transform.position;
        float duration = 0.2f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            go.transform.position = Vector3.Lerp(startPos, targetPos, t / duration);
            yield return null;
        }
        go.transform.position = targetPos;
    }
    private bool DetectAnyMatches()
    {
        for (int x = 0; x < cfg.gridWidth; x++)
        {
            for (int y = 0; y < cfg.gridHeight; y++)
            {
                Tile a = tileInstances[x, y].GetComponent<Tile>();
                if (a.isObstacle || a.isMatched) continue;

                // Check right neighbor
                if (x + 1 < cfg.gridWidth)
                {
                    Tile b = tileInstances[x + 1, y].GetComponent<Tile>();
                    if (!b.isObstacle && !b.isMatched && a.sockID == b.sockID)
                        return true;
                }
                // Check up neighbor
                if (y + 1 < cfg.gridHeight)
                {
                    Tile b = tileInstances[x, y + 1].GetComponent<Tile>();
                    if (!b.isObstacle && !b.isMatched && a.sockID == b.sockID)
                        return true;
                }
            }
        }
        return false;
    }
    private void SpawnTileAt(int x, int y)
    {
        // Compute world‐space position
        Vector3 worldPos = new Vector3(
            originX + x * tileSpacing,
            originY + y * tileSpacing,
            0f
        );

        // Instantiate prefab
        GameObject go = Instantiate(tilePrefab, worldPos, Quaternion.identity, boardParent);
        var tile = go.GetComponent<Tile>();

        // Choose a random sock color
        int randomSock = Random.Range(0, sockSprites.Length);

        // Initialize as a normal sock tile
        tile.Initialize(x, y, randomSock, false, sockSprites, null);

        // Store it so tileInstances[x,y] is valid
        tileInstances[x, y] = go;
    }
    private IEnumerator CollapseAndRefill()
    {
        // shift all columns down
        for (int x = 0; x < cfg.gridWidth; x++)
        {
            int writeY = 0;
            for (int y = 0; y < cfg.gridHeight; y++)
            {
                var go = tileInstances[x, y];
                if (go.activeSelf)
                {
                    // move it down to (x, writeY)
                    if (y != writeY)
                        StartCoroutine(AnimateDrop(go,
                            new Vector3(originX + x * tileSpacing, originY + writeY * tileSpacing, 0f)
                        ));

                    tileInstances[x, writeY] = go;
                    go.GetComponent<Tile>().GridY = writeY;
                    writeY++;
                }
            }
            // spawn new ones to fill above
            for (int y = writeY; y < cfg.gridHeight; y++)
            {
                SpawnTileAt(x, y);
                yield return null; // stagger if desired
            }
        }

        // wait for all drops
        yield return new WaitForSeconds(0.25f);

        // cascade if new matches appear
        if (DetectAnyMatches())
        {
            // you’ll need a method to collect those matches and call OnTilesMatched
           
            yield return StartCoroutine(CollapseAndRefill());
        }
    }
    private IEnumerator Shake(GameObject go, float duration, float magnitude)
    {
        Vector3 original = go.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float damper = 1f - (elapsed / duration);
            Vector2 offset = Random.insideUnitCircle * magnitude * damper;
            go.transform.position = original + (Vector3)offset;
            yield return null;
        }
        go.transform.position = original;
    }
    private (Tile a, Tile b)? FindFirstMatchPair()
    {
        for (int x = 0; x < cfg.gridWidth; x++)
        {
            for (int y = 0; y < cfg.gridHeight; y++)
            {
                Tile t = tileInstances[x, y].GetComponent<Tile>();
                if (t.isObstacle || t.isMatched) continue;

                // right
                if (x + 1 < cfg.gridWidth)
                {
                    Tile u = tileInstances[x + 1, y].GetComponent<Tile>();
                    if (!u.isObstacle && !u.isMatched && t.sockID == u.sockID)
                        return (t, u);
                }
                // up
                if (y + 1 < cfg.gridHeight)
                {
                    Tile u = tileInstances[x, y + 1].GetComponent<Tile>();
                    if (!u.isObstacle && !u.isMatched && t.sockID == u.sockID)
                        return (t, u);
                }
            }
        }
        return null;
    }

    private IEnumerator ShowHint()
    {
        var match = FindFirstMatchPair();
        if (match == null) yield break;

        hintActive = true;
        Tile a = match.Value.a, b = match.Value.b;

        const int pulses = 3;
        for (int i = 0; i < pulses; i++)
        {
            a.transform.localScale = Vector3.one * 1.2f;
            b.transform.localScale = Vector3.one * 1.2f;
            yield return new WaitForSeconds(0.3f);
            a.transform.localScale = Vector3.one;
            b.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.3f);
        }

        // reset timer for next hint
        lastActionTime = Time.time;
        hintActive = false;
    }
    private IEnumerator CollapseRefillSequence()
    {
        // Wait for both tiles' PlayMatchAnimation coroutines (0.2s scale + 0.2s fade)
        yield return new WaitForSeconds(0.4f);

        // Now run your collapse & refill
        yield return StartCoroutine(CollapseAndRefill());
    }
    private void CompleteObjective()
    {
        Debug.Log("Objective completed! Checking stars...");

        // Logic to check stars based on score (optional)
        int score = CalculateScore();

        int stars = 1;
        if (score >= currentObjective.star2)
            stars = 2;
        if (score >= currentObjective.star3)
            stars = 3;

        Debug.Log($"Stars earned: {stars}");
    }
    private int CalculateScore()
    {
        // Example: scoring based on number of moves left and matched socks
        int score = currentObjective.matchedCount * BASE_POINT_PER_MATCH + movesLeft * 100;
        return score;
    }

}

