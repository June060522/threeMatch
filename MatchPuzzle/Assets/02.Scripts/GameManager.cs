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
    public LevelGoal LevelGoalTimed {get=> m_levelGoal;}
    Collectable collectable;
    public LevelGoalCollected m_levlecollectionGoal;

    public Sprite winIcom;
    public Sprite loseIcon;
    public Sprite goalIcon;

    public override void Awake()
    {
        m_levelGoal = GetComponent<LevelGoal>();
        m_board = FindObjectOfType<Board>();
        //m_levelGoalTimed = GetComponent<LevelGoalTimed>();
        m_levlecollectionGoal = GetComponent<LevelGoalCollected>();
    }

    private void Start()
    {
        if(UIManager.Instance != null)
        {            
            if(UIManager.Instance.scoreMeter != null)
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
        if(m_levelGoal.levelCounter == LevelCounter.Moves)
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
        yield return StartCoroutine("WaitForBoardRoutine",0.5f);
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
            UIManager.Instance.messageWindow.ShowMessage(goalIcon, "score goal\n" + m_levelGoal.scoreGoals[0].ToString(), "start");
        }
        while (!m_isReadyToBegin)
        {
            //Debug.Log("123");
            yield return null;
        }
        //Debug.Log("123");
        if(UIManager.Instance != null)
            UIManager.Instance.screenFader?.FadeOff();

        yield return new WaitForSeconds(1f);

        m_board?.SetUpBoard();
    }

    IEnumerator PlayGameRoutine()
    {
        if(m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.StartCountDown();
        }

        while (!m_isGameOver)
        {
            m_isGameOver = m_levelGoal.IsGameOver();
            m_isWinner = m_levelGoal.IsWinner();

            yield return null;
        }
    }

    IEnumerator WaitForBoardRoutine(float delay = 0f)
    {
        if(m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            if(UIManager.Instance.timer != null && UIManager.Instance != null)
            {
                UIManager.Instance.timer.FadeOff();
                UIManager.Instance.timer.paused = true;
            }
        }

        if(m_board != null)
        {
            yield return new WaitForSeconds(m_board.swapTime);

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
            if (UIManager.Instance.messageWindow != null && UIManager.Instance != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowMessage(winIcom, "You win!", "ok");
            }

            if(SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWinSound();
            }
        }
        else
        {
            if (UIManager.Instance.messageWindow != null && UIManager.Instance != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowMessage(loseIcon, "You lose", "Ok");
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLoseSound();
            }
        }
        if(UIManager.Instance != null)
           UIManager.Instance.screenFader?.FadeOn();

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
                if(UIManager.Instance.scoreMeter != null && UIManager.Instance != null)
                {
                    UIManager.Instance.scoreMeter.UpdateScoreMeter(ScoreManager.Instance.Score,m_levelGoal.scoreStars);
                }
            }

            if(SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayClipAtPoint(piece.clearSound,Vector3.zero,SoundManager.Instance.fxVolume);
            }
        }
    }
    public void AddTime(int timeValue)
    {
        if(m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.AddTime(timeValue);
        }
    }

    public void UpdateCollectionBoards(GamePiece piecetoCheck)
    {
        if(m_levlecollectionGoal != null)
        {
            m_levlecollectionGoal.UpdateGoals(piecetoCheck);
        }
    }
}
