using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
    public override void Start()
    {
        levelCounter = LevelCounter.Timer;
        base.Start();
        
    }

    public override bool IsGameOver()
    {
        int maxScore = scoreGoals[scoreGoals.Length - 1];
        if (ScoreManager.Instance.Score >= maxScore) return true;
        return (timeLeft < 0 );
    }

    public override bool IsWinner()
    {
        if (ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.Score >= scoreGoals[0];
        }
        return false;
    }
}
