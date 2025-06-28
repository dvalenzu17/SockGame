using UnityEngine;

public static class LevelConfigFetcher
{
    // Base shapes
    private const int BASE_GRID_WIDTH = 6;
    private const int BASE_GRID_HEIGHT = 8;

    // Sock‐color & obstacle ramps (your existing logic)
    private const int BASE_SOCK_COLORS = 4;
    private const int MAX_SOCK_COLORS = 8;
    private const int BASE_OBSTACLES = 0;
    private const int MAX_OBSTACLES = 9;

    public static LevelConfig GetConfig(int levelIndex)
    {
        var cfg = new LevelConfig();
        cfg.levelIndex = levelIndex;

        // 1) Dynamic board shape by level bracket
        if (levelIndex <= 5)
        {
            cfg.gridWidth = BASE_GRID_WIDTH;      // 6
            cfg.gridHeight = BASE_GRID_HEIGHT;     // 8
        }
        else if (levelIndex <= 10)
        {
            cfg.gridWidth = BASE_GRID_WIDTH + 1;  // 7
            cfg.gridHeight = BASE_GRID_HEIGHT;     // 8
        }
        else if (levelIndex <= 20)
        {
            cfg.gridWidth = BASE_GRID_WIDTH + 2;  // 8
            cfg.gridHeight = BASE_GRID_HEIGHT + 1; // 9
        }
        else
        {
            cfg.gridWidth = BASE_GRID_WIDTH + 3;  // 9
            cfg.gridHeight = BASE_GRID_HEIGHT + 1; // 9
        }

        // 2) Sock‐color variety
        cfg.numSocksColors = Mathf.Min(
            BASE_SOCK_COLORS + (levelIndex - 1) / 4,
            MAX_SOCK_COLORS
        );

        // 3) Obstacle count
        int rawObstacles = (levelIndex <= 5) ? 0 : ((levelIndex - 5) / 3);
        cfg.numObstacles = Mathf.Clamp(
            rawObstacles, BASE_OBSTACLES, MAX_OBSTACLES
        );

        // 4) Pair capacity (your existing even‐slots logic)
        int totalSlots = cfg.gridWidth * cfg.gridHeight;
        int avail = totalSlots - cfg.numObstacles;
        if (avail % 2 != 0 && cfg.numObstacles > 0) cfg.numObstacles--;
        cfg.totalPairs = (cfg.gridWidth * cfg.gridHeight - cfg.numObstacles) / 2;

        // 5) Pull in objective & scoring from our generator
        var obj = LevelObjectiveGenerator.GetObjective(levelIndex, 30);

        // 6) Override moveLimit & objective fields
        cfg.moveLimit = obj.moveLimit;
        cfg.targetColorID = obj.targetColorID;
        cfg.targetMatches = obj.targetMatches;
        cfg.star1Threshold = obj.star1;
        cfg.star2Threshold = obj.star2;
        cfg.star3Threshold = obj.star3;

        // 7) Dog‐shuffle interval (linear ramp)
        const float BASE_INTERVAL = 20f;
        const float MIN_INTERVAL = 8f;
        float rawInterval = BASE_INTERVAL
            - (levelIndex - 1) * (BASE_INTERVAL - MIN_INTERVAL) / 29f;
        cfg.dogShuffleInterval = Mathf.Max(rawInterval, MIN_INTERVAL);

        cfg.extraSpawnRate = 0.5f;

        return cfg;
    }
}
