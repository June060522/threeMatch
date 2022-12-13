using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
    public Timer timer;
    int m_maxTime;

    private void Start()
    {
        if(timer!=null)
        {
            timer.InitTimer(timeLeft);
        }
        m_maxTime = timeLeft;
    }
    public void StartCountDown()
    {
        StartCoroutine(CountDownRoutine());
    }

    IEnumerator CountDownRoutine()
    {
        while(timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
            if(timer != null) timer.UpdateTimer(timeLeft);
        }
    }

    public override bool IsGameOver()
    {
        int maxScore = scoreGoals[scoreGoals.Length - 1];
        if (ScoreManager.Instance.Score >= maxScore) return true;

        return (timeLeft <= 0);
    }

    public override bool IsWinner()
    {
        if (ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.Score >= scoreGoals[0];
        }
        return false;
    }

    public void AddTime(int timeValue)
    {
        timeLeft += timeValue;
        timeLeft = Mathf.Clamp(timeLeft,0,m_maxTime);

        if(timer != null)
        {
            timer.UpdateTimer(timeLeft);
        }
    }
}
