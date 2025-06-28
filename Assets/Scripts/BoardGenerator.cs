using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public static class BoardGenerator 
{
    public struct CellData
    {
        public bool isObstacle;
        public int sockID;
    }
    public static CellData[,] Generate(LevelConfig cfg)
    {
        int w = cfg.gridWidth, h = cfg.gridHeight;
        int totalSlots = w * h;

        // 1) Initialize board
        CellData[,] board = new CellData[w, h];

        // 2) Build list of all indices
        List<int> allIndices = new List<int>(totalSlots);
        for (int i = 0; i < totalSlots; i++)
            allIndices.Add(i);

        // 3) Pick obstacle slots
        List<int> obstacleIndices = new List<int>();
        for (int i = 0; i < cfg.numObstacles; i++)
        {
            int pick = Random.Range(0, allIndices.Count);
            obstacleIndices.Add(allIndices[pick]);
            allIndices.RemoveAt(pick);
        }

        // 4) Build weights for sock colors
        int C = cfg.numSocksColors;
        float extra = cfg.extraSpawnRate;    // e.g. 0.25f
        float[] wts = new float[C];
        float totalW = 0f;
        for (int i = 0; i < C; i++)
        {
            wts[i] = (i == cfg.targetColorID) ? 1f + extra : 1f;
            totalW += wts[i];
        }

        // 5) Fill each free slot with a weighted random color
        foreach (int slot in allIndices)
        {
            int x = slot % w, y = slot / w;
            float r = Random.value * totalW;
            float acc = 0f;
            int chosen = 0;
            for (int i = 0; i < C; i++)
            {
                acc += wts[i];
                if (r < acc)
                {
                    chosen = i;
                    break;
                }
            }
            board[x, y].isObstacle = false;
            board[x, y].sockID = chosen;
        }

        // 6) Place obstacles
        foreach (int obsIdx in obstacleIndices)
        {
            int x = obsIdx % w, y = obsIdx / w;
            board[x, y].isObstacle = true;
            board[x, y].sockID = -1;
        }

        // 7) Guarantee at least one adjacent pair
        if (!HasAtLeastOneAdjacentPair(board, w, h))
            ForceOneAdjacentPair(board, w, h);

        return board;
    }


    private static bool HasAtLeastOneAdjacentPair(CellData[,] board, int w, int h)
    {
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (board[x, y].isObstacle) continue;
                int id = board[x, y].sockID;

                // Check right
                if (x + 1 < w && !board[x + 1, y].isObstacle && board[x + 1, y].sockID == id)
                    return true;

                // Check up
                if (y + 1 < h && !board[x, y + 1].isObstacle && board[x, y + 1].sockID == id)
                    return true;
            }
        }
        return false;
    }

    private static bool ForceOneAdjacentPair(CellData[,] board, int w, int h)
    {
        
        Dictionary<int, List<Vector2Int>> posDict = new Dictionary<int, List<Vector2Int>>();
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (board[x, y].isObstacle) continue;
                int id = board[x, y].sockID;
                if (!posDict.ContainsKey(id)) posDict[id] = new List<Vector2Int>();
                posDict[id].Add(new Vector2Int(x, y));
            }
        }

      
        foreach (var kv in posDict)
        {
            var list = kv.Value;
            if (list.Count < 2) continue;

            Vector2Int a = list[0];
            Vector2Int b = list[1];

            
            var neighbors = new List<Vector2Int>();
            if (a.x + 1 < w && !board[a.x + 1, a.y].isObstacle) neighbors.Add(new Vector2Int(a.x + 1, a.y));
            if (a.x - 1 >= 0 && !board[a.x - 1, a.y].isObstacle) neighbors.Add(new Vector2Int(a.x - 1, a.y));
            if (a.y + 1 < h && !board[a.x, a.y + 1].isObstacle) neighbors.Add(new Vector2Int(a.x, a.y + 1));
            if (a.y - 1 >= 0 && !board[a.x, a.y - 1].isObstacle) neighbors.Add(new Vector2Int(a.x, a.y - 1));

            foreach (var n in neighbors)
            {
                
                int tempID = board[n.x, n.y].sockID;
                bool tempObs = board[n.x, n.y].isObstacle;

                board[n.x, n.y].sockID = kv.Key; 
                board[n.x, n.y].isObstacle = false;

                board[b.x, b.y].sockID = tempID;
                board[b.x, b.y].isObstacle = tempObs;
                return true;
            }
        }

        return false;
    }

}
