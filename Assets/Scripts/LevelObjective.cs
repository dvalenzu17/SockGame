using UnityEngine;

public class LevelObjective
{
    public int levelIndex;       // Level number
    public int targetColorID;    // ID of the target color for this level
    public int targetMatches;    // Number of target socks to match
    public int moveLimit;        // Maximum moves allowed
    public int star1;            // Score threshold for 1 star
    public int star2;            // Score threshold for 2 stars
    public int star3;            // Score threshold for 3 stars
    public int matchedCount;     // Current count of objective socks matched

    // Constructor to initialize the objective data
    public LevelObjective(int levelIndex, int targetColorID, int targetMatches, int moveLimit, int star1, int star2, int star3)
    {
        this.levelIndex = levelIndex;
        this.targetColorID = targetColorID;
        this.targetMatches = targetMatches;
        this.moveLimit = moveLimit;
        this.star1 = star1;
        this.star2 = star2;
        this.star3 = star3;
        this.matchedCount = 0; // Initialize matched count to 0 at the start
    }
}