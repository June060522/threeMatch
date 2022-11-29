using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class BoardDeadlock : MonoBehaviour
{
    List<GamePiece> GetRowOrColumnList(GamePiece[,] allPieces, int x, int y, int listLenth = 3, bool checkRow = true)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        List<GamePiece> piecesList = new List<GamePiece>();

        for (int i = 0; i < listLenth; i++)
        {
            if (checkRow)
            {
                if (x + i < width && y < height && allPieces[x + i, y] != null)
                {
                    piecesList.Add(allPieces[x + i, y]);
                }
            }
            else
            {
                if (x < width && y + i < height && allPieces[x, y + i] != null)
                {
                    piecesList.Add(allPieces[x, y + i]);
                }
            }
        }
        return piecesList;
    }
    List<GamePiece> GetMinimumMatchs(List<GamePiece> gamePieces, int minForMatch = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();

        var groups = gamePieces.GroupBy(n => n.matchValue);

        foreach (var grp in groups)
        {
            if (grp.Count() >= minForMatch && grp.Key != MatchValue.None)
            {
                matches = grp.ToList();
            }
        }
        return matches;
    }

    List<GamePiece> GetNeighbors(GamePiece[,] allPieces, int x, int y)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        List<GamePiece> neighbors = new List<GamePiece>();

        Vector2[] searchDirections = new Vector2[4]
        {
            new Vector2(-1f,0f),
            new Vector2(1f,0f),
            new Vector2(0f,1f),
            new Vector2(0f,-1f)
        };

        foreach (Vector2 dir in searchDirections)
        {
            if (x + (int)dir.x >= 0 && x + (int)dir.x < width && y + (int)dir.y > 0 && y + (int)dir.y < height)
            {
                if (allPieces[x + (int)dir.x, y + (int)dir.y] != null)
                {
                    if (!neighbors.Contains(allPieces[x + (int)dir.x, y + (int)dir.y]))
                        neighbors.Add(allPieces[x + (int)dir.x, y + (int)dir.y]);
                }
            }
        }
        return neighbors;
    }

    bool HasMoveAt(GamePiece[,] allpieces, int x, int y, int listLenth = 3, bool checkRow = true)
    {
        List<GamePiece> pieces = GetRowOrColumnList(allpieces, x, y, listLenth, checkRow);

        List<GamePiece> matches = GetMinimumMatchs(pieces);

        GamePiece unmatchedPieces = null;

        if (pieces != null && matches != null)
        {
            if (pieces.Count == listLenth && matches.Count == listLenth - 1)
            {
                unmatchedPieces = pieces.Except(matches).FirstOrDefault();

            }
            if (unmatchedPieces != null)
            {
                List<GamePiece> neighbors = GetNeighbors(allpieces, unmatchedPieces.xIndex, unmatchedPieces.yIndex);
                neighbors = neighbors.Except(matches).ToList();
                neighbors = neighbors.FindAll(n => n.matchValue == matches[0].matchValue);
                matches = matches.Union(neighbors).ToList();
            }

            if (matches.Count >= listLenth)
            {
                string rowColstr = (checkRow) ? "row" : "Column";
                Debug.Log("======= Available Move =======");
                //Debug.Log("Move " + matches[0].matchValue + "piece to " + unmatchedPieces.xIndex + "," + unmatchedPieces.yIndex + " to from matching " + rowColstr);
                return true;
            }
        }
        return false;
    }

    public bool IsDeadlocked(GamePiece[,] allpieces, int listLenth)
    {
        int width = allpieces.GetLength(0);
        int height = allpieces.GetLength(1);

        bool isDeadlocked = true;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (HasMoveAt(allpieces, i, j, listLenth, true) || HasMoveAt(allpieces, i, j, listLenth, false))
                    isDeadlocked = false;

            }
        }

        if (isDeadlocked)
        {
            Debug.Log("---Deadlock---");
        }
        return isDeadlocked;
    }
}
