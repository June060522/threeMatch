using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardShuffler : MonoBehaviour
{
    public List<GamePiece> RemoveNormalPieces(GamePiece[,] allGamePieces)
    {
        List<GamePiece> normalPieces = new List<GamePiece>();
        int width = allGamePieces.GetLength(0);
        int height = allGamePieces.GetLength(1);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allGamePieces[i, j] != null)
                {
                    Bomb bomb = allGamePieces[i, j].GetComponent<Bomb>();
                    Collectable collectable = allGamePieces[i, j].GetComponent<Collectable>();
                    if (bomb == null && collectable == null)
                    {
                        normalPieces.Add(allGamePieces[i, j]);
                        allGamePieces[i, j] = null;
                    }
                }
            }
        }
        return normalPieces;
    }

    public void ShuffleList(List<GamePiece> piecesToShuffle)
    {
        int maxCount = piecesToShuffle.Count;
        for (int i = 0; i < maxCount; i++)
        {
            int j = Random.Range(i, maxCount);
            if (i == j)
                continue;

            GamePiece temp = piecesToShuffle[i];
            piecesToShuffle[i] = piecesToShuffle[j];
            piecesToShuffle[j] = temp;
        }
    }

    public void MovePieces(GamePiece[,] allGamePieces, float swapTime = 0.5f)
    {
        int width = allGamePieces.GetLength(0);
        int height = allGamePieces.GetLength(1);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allGamePieces[i, j] != null)
                {
                    allGamePieces[i, j].Move(i, j, swapTime);
                }
            }
        }
    }
}