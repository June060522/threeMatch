using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    // public int movesLeft = 30;
    // public int scroeGoal = 1000;

    bool m_isReadyToBegin = false;
    bool m_isGameOver = false;
    public bool M_isGameOver { get => m_isGameOver; set => m_isGameOver = value; }
    bool m_isWinner = false;
    bool m_isReadyToReload = false;

    Board m_board;

    LevelGoal m_levelGoal;
    //LevelGoalTimed m_levelGoalTimed;
    public LevelGoal LevelGoalTimed { get => m_levelGoal; }
    Collectable collectable;
    public LevelGoalCollected m_levlecollectionGoal;

    public override void Awake()
    {
        m_levelGoal = GetComponent<LevelGoal>();
        m_board = FindObjectOfType<Board>();
        //m_levelGoalTimed = GetComponent<LevelGoalTimed>();
        m_levlecollectionGoal = GetComponent<LevelGoalCollected>();
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.scoreMeter != null)
                UIManager.Instance.scoreMeter.SetUpstarts(m_levelGoal);
            Scene scene = SceneManager.GetActiveScene();
            if (UIManager.Instance.levelNameText != null)
            {
                UIManager.Instance.levelNameText.text = scene.name;
            }
            if (m_levlecollectionGoal != null)
            {
                UIManager.Instance.SetupCollectionGoalLayout(m_levlecollectionGoal.collectionGoals);
            }
            else
            {
                UIManager.Instance.EnableCollectionGoalLayout(false);
            }

            bool useTime = (m_levelGoal.levelCounter == LevelCounter.Timer);

            UIManager.Instance.EnableTimer(useTime);
            UIManager.Instance.EnableMovesCounter(!useTime);
        }
        m_levelGoal.movesLeft++;
        StartCoroutine(ExecuteGameLoop());
        UpdateMoves();
    }


    public void UpdateMoves()
    {
        if (m_levelGoal.levelCounter == LevelCounter.Moves)
        {
            m_levelGoal.movesLeft--;
            if (UIManager.Instance != null && UIManager.Instance.movesLeftText != null)
                UIManager.Instance.movesLeftText.text = m_levelGoal.movesLeft.ToString();
        }
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("WaitForBoardRoutine", 0.5f);
        yield return StartCoroutine("EndGameRoutine");
    }

    public void BiginGame()
    {
        m_isReadyToBegin = true;
    }

    IEnumerator StartGameRoutine()
    {
        if (UIManager.Instance.messageWindow != null && UIManager.Instance != null)
        {
            UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowGoal(m_levelGoal.scoreGoals[m_levelGoal.scoreGoals.Length - 1]);

            if (m_levelGoal.levelCounter == LevelCounter.Timer)
            {
                UIManager.Instance.messageWindow.ShowTimeGoal(m_levelGoal.timeLeft);
            }
            else
            {
                UIManager.Instance.messageWindow.ShowMovesGoal(m_levelGoal.movesLeft - 1);
            }

            if (m_levlecollectionGoal != null)
            {
                UIManager.Instance.messageWindow.ShowCollectionGoal(true);
                GameObject goalLayout = UIManager.Instance.messageWindow.collectionGoalLayout;
                if (goalLayout != null)
                {
                    UIManager.Instance.SetupCollectionGoalLayout(m_levlecollectionGoal.collectionGoals, goalLayout, 80);
                }
            }
            else
            {
                UIManager.Instance.messageWindow.ShowCollectionGoal(false);
            }
        }
        while (!m_isReadyToBegin)
        {
            //Debug.Log("123");
            yield return null;
        }
        //Debug.Log("123");
        if (UIManager.Instance != null)
            UIManager.Instance.screenFader?.FadeOff();

        yield return new WaitForSeconds(1f);

        m_board?.SetUpBoard();
    }

    IEnumerator PlayGameRoutine()
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.StartCountDown();
        }

        while (!m_isGameOver)
        {
            m_isGameOver = m_levelGoal.IsGameOver();
            m_isWinner = m_levelGoal.IsWinner();

            yield return null;
        }
        //yield return StartCoroutine("EndGameRoutine");
    }

    IEnumerator WaitForBoardRoutine(float delay = 0f)
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            if (UIManager.Instance.timer != null)
            {
                UIManager.Instance.timer.FadeOff();
                UIManager.Instance.timer.paused = true;
            }
        }
        if (m_board != null)
        {
            while (!m_board.isRefilling)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(delay);
    }

    IEnumerator EndGameRoutine()
    {
        m_isReadyToReload = false;

        if (m_isWinner)
        {
            ShowWinScreen();
        }
        else
        {
            ShowLoseScreen();
        }

        if (UIManager.Instance.screenFader != null) UIManager.Instance.screenFader.FadeOn();

        while (!m_isReadyToReload)
        {
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowLoseScreen()
    {
        if (UIManager.Instance.messageWindow != null)
        {
            UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowLoseMessage();
            UIManager.Instance.messageWindow.ShowCollectionGoal(false);

            string caption = "";
            if(m_levelGoal.levelCounter == LevelCounter.Timer)
            {
                caption = "Out of time!!";
            }
            else
            {
                caption = "Out of moves";
            }
            UIManager.Instance.messageWindow.ShowGoalCaption(caption,0,70);

            if(UIManager.Instance.messageWindow.goalFailedIcon != null)
            {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalFailedIcon);
            }
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLoseSound();
        }
    }

    private void ShowWinScreen()
    {
        if (UIManager.Instance.messageWindow != null)
        {
            UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowWinMessage();
            UIManager.Instance.messageWindow.ShowCollectionGoal(false);

            if(ScoreManager.Instance != null)
            {
                string scoreStr = "you scored\n" + ScoreManager.Instance.Score + "points!!";
                UIManager.Instance.messageWindow.ShowGoalCaption(scoreStr,0,70);
            }

            if(UIManager.Instance.messageWindow.goalCompleteIcon != null)
            {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalCompleteIcon);
            }
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWinSound();
        }
    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }

    public void ScorePoints(GamePiece piece, int multiplier = 1, int bonus = 0)
    {
        if (piece != null)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(piece.scoreValue * multiplier + bonus);
                m_levelGoal.UpdateScoreStars(ScoreManager.Instance.Score);
                if (UIManager.Instance.scoreMeter != null && UIManager.Instance != null)
                {
                    UIManager.Instance.scoreMeter.UpdateScoreMeter(ScoreManager.Instance.Score, m_levelGoal.scoreStars);
                }
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayClipAtPoint(piece.clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
            }
        }
    }
    public void AddTime(int timeValue)
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.AddTime(timeValue);
        }
    }

    public void UpdateCollectionBoards(GamePiece piecetoCheck)
    {
        if (m_levlecollectionGoal != null)
        {
            m_levlecollectionGoal.UpdateGoals(piecetoCheck);
        }
    }
}
