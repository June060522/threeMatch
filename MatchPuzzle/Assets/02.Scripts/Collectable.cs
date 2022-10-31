using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : GamePiece
{
    public bool clearedByBomb = false;
    public bool clearedAtBottom = false;
    void Start()
    {
        matchValue = MatchValue.None;
    }
}
