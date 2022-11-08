using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public int movesLeft = 30;
    public int scroeGoal = 10000;

    public ScreenFader screenFader;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI movesLeftText;

    bool m_isReadyToBegin = false;
    bool m_isGameOver = false;
    bool m_isWinner = false;

    Board m_board;
    private void Start()
    {
        m_board = FindObjectOfType<Board>();
        Scene scene = SceneManager.GetActiveScene();
        if(levelNameText != null)
        {
            levelNameText.text = scene.name;
        }
        UpdateMoves();
        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves()
    {
        if(movesLeftText != null)
            movesLeftText.text = movesLeft.ToString();
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("EndGameRoutine");
    }

    IEnumerator StartGameRoutine()
    {
        while(!m_isReadyToBegin)
        {
            yield return null;
            yield return new WaitForSeconds(2f);
            m_isReadyToBegin = true;
        }
        screenFader?.FadeOff();

        yield return new WaitForSeconds(0.5f);

        m_board?.SetUpBoard();
    }

    IEnumerator PlayGameRoutine()
    {
        while(!m_isGameOver)
        {
            if(movesLeft == 0)
            {
                m_isGameOver = true;
                m_isWinner = false;
            }
            yield return null;
        }
    }

    IEnumerator EndGameRoutine()
    {
        screenFader?.FadeOn()
        if(m_isWinner)
        {
            Debug.Log($"You Wun!!");
        }
        else
        {
            Debug.Log($"You Lose!!");
        }
        yield return null;
    }

}
