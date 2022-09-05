using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] int xIndex;
    [SerializeField] int yIndex;

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
}
