using UnityEngine;

public static class LevelObjectiveGenerator
{
    // CONFIGURABLE BASES
    private const int NUM_COLORS = 8;
    private const int MIN_TARGET = 5;
    private const int MAX_TARGET = 20;
    private const int MIN_MOVES = 10;
    private const int MAX_MOVES = 25;

    /// <summary>
    /// Returns a LevelObjective for any levelIndex in [1..30].
    /// </summary>
    public static LevelObjective GetObjective(int levelIndex, int basePointPerMatch = 50, int maxLevels = 30)
    {
        int idx = Mathf.Clamp(levelIndex, 1, maxLevels);

        // Rotate target color through 0..NUM_COLORS-1
        int colorID = (idx - 1) % NUM_COLORS;

        // Scale targetMatches linearly: MIN_TARGET → MAX_TARGET over all levels
        float tNorm = (idx - 1) / (float)(maxLevels - 1);
        int targetMatches = Mathf.RoundToInt(Mathf.Lerp(MIN_TARGET, MAX_TARGET, tNorm));

        // Scale moveLimit inversely: MAX_MOVES → MIN_MOVES
        int moveLimit = Mathf.RoundToInt(Mathf.Lerp(MAX_MOVES, MIN_MOVES, tNorm));

        // Compute star thresholds based on max score
        int baseScore = targetMatches * basePointPerMatch;
        int bonus = Mathf.RoundToInt(baseScore * 0.2f);
        int maxScore = baseScore + bonus;

        int star1 = Mathf.RoundToInt(baseScore * 0.5f);
        int star2 = baseScore + bonus;
        int star3 = Mathf.RoundToInt(baseScore * 1.5f);

        return new LevelObjective(idx, colorID, targetMatches, moveLimit, star1, star2, star3);
    }
}
