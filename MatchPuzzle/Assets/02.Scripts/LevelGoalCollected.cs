using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalCollected : LevelGoal
{
    public CollectionGoal[] collectionGoals;
    public CollectionGoalPanel[] uiPanels;

    public void UpdateGoals(GamePiece pieceToCheck)
    {
        foreach (CollectionGoal goal in collectionGoals)
        {
            if(pieceToCheck != null)
            {
                goal.CollectPiece(pieceToCheck);
            }
        }
        UpdateUI();
    }
    public void UpdateUI()
    {
        foreach(CollectionGoalPanel panel in uiPanels)
        {
            if(panel != null)
            {
                panel.UpdatePanel();
            }
        }
    }
    bool AreGaolsComplete(CollectionGoal[] goals)
    {
        foreach(CollectionGoal g in goals)
        {
            if(g == null || goals == null)
                return false;
            if(goals.Length == 0)
                return false;
            if(g.numberToCollect != 0)
                return false;
        }
        return true;
    }

    public override bool IsGameOver()
    {
        if(AreGaolsComplete(collectionGoals) && ScoreManager.Instance != null)
        {
            int maxScore = scoreGoals[scoreGoals.Length - 1];
            if(ScoreManager.Instance.Score >= maxScore)
            {
                return true;
            }
        }
        return (movesLeft <= 0);
    }

    public override bool IsWinner()
    {
        if(ScoreManager.Instance != null)
        {
            return (ScoreManager.Instance.Score >= scoreGoals[0] && AreGaolsComplete(collectionGoals));
        }
        return false;
    }

}
