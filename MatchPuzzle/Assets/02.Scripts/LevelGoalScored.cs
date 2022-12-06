using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalScored : LevelGoal
{
    public override bool IsGameOver()
    {
        if(ScoreManager.Instance.Score >= scoreGoals[2])
            return true;
        return movesLeft <= 0;
    }

    public override bool IsWinner()
    {
        if(ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.Score >= scoreGoals[0];
        }
        return false;
    }
}
