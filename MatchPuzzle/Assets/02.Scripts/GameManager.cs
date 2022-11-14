using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public int movesLeft = 30;
    public int scroeGoal = 1000;

    public ScreenFader screenFader;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI movesLeftText;

    bool m_isReadyToBegin = false;
    bool m_isGameOver = false;
    bool m_isWinner = false;
    bool m_isReadyToReload = false;

    Board m_board;

    public MessageWindow messageWindow;
    public Sprite winIcom;
    public Sprite loseIcon;
    public Sprite goalIcon;
    private void Start()
    {
        m_board = FindObjectOfType<Board>();
        Scene scene = SceneManager.GetActiveScene();
        if (levelNameText != null)
        {
            levelNameText.text = scene.name;
        }
        UpdateMoves();
        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves()
    {
        if (movesLeftText != null)
            movesLeftText.text = movesLeft.ToString();
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("EndGameRoutine");
    }

    public void BiginGame()
    {
        m_isReadyToBegin = true;
    }

    IEnumerator StartGameRoutine()
    {
        if (messageWindow != null)
        {
            messageWindow.GetComponent<RectXformMover>().MoveOn();
            messageWindow.ShowMessage(goalIcon, "score goal\n" + scroeGoal.ToString(), "start");
        }
        while (!m_isReadyToBegin)
        {
            yield return null;
        }
        screenFader?.FadeOff();

        yield return new WaitForSeconds(1f);

        m_board?.SetUpBoard();
    }

    IEnumerator PlayGameRoutine()
    {
        while (!m_isGameOver)
        {
            if(ScoreManager.Instance.m_currentScore >= scroeGoal)
            {
                m_isGameOver = true;
                m_isWinner = true;
            }
            else if (movesLeft == 0)
            {
                m_isGameOver = true;
                m_isWinner = false;
            }

            yield return null;
        }
    }

    IEnumerator EndGameRoutine()
    {
        m_isReadyToReload = false;
        if (m_isWinner)
        {
            if (messageWindow != null)
            {
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowMessage(winIcom, "You win", "Okay");
            }
        }
        else
        {
            if (messageWindow != null)
            {
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowMessage(loseIcon, "You lose", "Okay");
            }
        }
        screenFader?.FadeOn();

        while(!m_isReadyToReload)
        {
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }

}
