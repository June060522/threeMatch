using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    // public int movesLeft = 30;
    // public int scroeGoal = 1000;

    public ScreenFader screenFader;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI movesLeftText;

    bool m_isReadyToBegin = false;
    bool m_isGameOver = false;
    public bool M_isGameOver { get => m_isGameOver; set => m_isGameOver = value; }
    public ScoreMeter scoreMeter;
    bool m_isWinner = false;
    bool m_isReadyToReload = false;

    Board m_board;
    LevelGoal m_levelGoal;
    LevelGoalTimed m_levelGoalTimed;

    public MessageWindow messageWindow;
    public Sprite winIcom;
    public Sprite loseIcon;
    public Sprite goalIcon;

    public override void Awake()
    {
        m_levelGoal = GetComponent<LevelGoal>();
        m_board = FindObjectOfType<Board>();
        m_levelGoalTimed = GetComponent<LevelGoalTimed>();
    }

    private void Start()
    {
        if(scoreMeter != null)
            scoreMeter.SetUpstarts(m_levelGoal);
        Scene scene = SceneManager.GetActiveScene();
        if (levelNameText != null)
        {
            levelNameText.text = scene.name;
        }
        m_levelGoal.movesLeft++;
        UpdateMoves();
        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves()
    {
        if(m_levelGoalTimed == null)
        {
            m_levelGoal.movesLeft--;
            if (movesLeftText != null)
                movesLeftText.text = m_levelGoal.movesLeft.ToString();
        }
        else
        {
            if(m_levelGoalTimed != null)
            {
                movesLeftText.text = "\u221E";
                movesLeftText.fontSize = 70;
            }
        }
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
            messageWindow.ShowMessage(goalIcon, "score goal\n" + m_levelGoal.scoreGoals[0].ToString(), "start");
        }
        while (!m_isReadyToBegin)
        {
            Debug.Log("123");
            yield return null;
        }
        Debug.Log("123");
        screenFader?.FadeOff();

        yield return new WaitForSeconds(1f);

        m_board?.SetUpBoard();
    }

    IEnumerator PlayGameRoutine()
    {
        if(m_levelGoalTimed != null)
        {
            m_levelGoalTimed.StartCountDown();
        }

        while (!m_isGameOver)
        {
            m_isGameOver = m_levelGoal.IsGameOver();
            m_isWinner = m_levelGoal.IsWinner();

            yield return null;
        }
    }

    IEnumerator EndGameRoutine()
    {
        m_isReadyToReload = false;
        if (m_isWinner)
        {
            yield return new WaitForSeconds(2f);
            if (messageWindow != null)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayWinSound();
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowMessage(winIcom, "You win", "Okay");
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
            if (messageWindow != null)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayLoseSound();
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowMessage(loseIcon, "You lose", "Okay");
            }
        }
        screenFader?.FadeOn();

        while (!m_isReadyToReload)
        {
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }
    
    public void ScorePoints(GamePiece piece,int multiplier = 1, int bonus = 0)
    {
        if(piece != null)
        {
            if(ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(piece.scoreValue * multiplier + bonus);
                m_levelGoal.UpdateScoreStars(ScoreManager.Instance.Score);
                if(scoreMeter != null)
                {
                    scoreMeter.UpdateScoreMeter(ScoreManager.Instance.Score,m_levelGoal.scoreStars);
                }
            }

            if(SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayClipAtPoint(piece.clearSound,Vector3.zero,SoundManager.Instance.fxVolume);
            }
        }
    }
}
