using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public GameObject collectionGoalLayout;     //전체 사이즈
    public int collectionGoalBaseWidth = 125;   //골 사이의 거리
    CollectionGoalPanel[] m_collectionGoalPanels;

    public ScreenFader screenFader;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI movesLeftText;
    public ScoreMeter scoreMeter;
    
    public MessageWindow messageWindow;

    public Timer timer;
    public GameObject MovesCounter;
    public override void Awake()
    {
        base.Awake();

        if(messageWindow != null)
        {
            messageWindow.gameObject.SetActive(true);
        }
        if(screenFader != null)
        {
            screenFader.gameObject.SetActive(true);
        }
    }

    public void SetupCollectionGoalLayout(CollectionGoal[] collectionGoals)
    {
        if (collectionGoalLayout != null && collectionGoals != null & collectionGoals.Length != 0)
        {
            RectTransform rectXform = collectionGoalLayout.GetComponent<RectTransform>();
            rectXform.sizeDelta = new Vector2(collectionGoalBaseWidth * collectionGoals.Length, rectXform.sizeDelta.y);

            m_collectionGoalPanels = collectionGoalLayout.GetComponentsInChildren<CollectionGoalPanel>();

            for (int i = 0; i < m_collectionGoalPanels.Length; i++)
            {
                if (i < collectionGoals.Length && collectionGoals[i] != null)
                {
                    m_collectionGoalPanels[i].gameObject.SetActive(true);
                    m_collectionGoalPanels[i].collectionGoal = collectionGoals[i];
                    m_collectionGoalPanels[i].SetupPanel();
                }
                else
                {
                    m_collectionGoalPanels[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void UpdateCollectionGoalLayout()
    {
        foreach(CollectionGoalPanel panel in m_collectionGoalPanels)
        {
            if(panel != null && panel.isActiveAndEnabled)
            {
                panel.UpdatePanel();
            }
        }
    }

    public void EnableTimer(bool state)
    {
        if(timer != null)
        {
            timer.gameObject.SetActive(state);
        }
    }

    public void EnableMovesCounter(bool state)
    {
        if(MovesCounter != null)
        {
            MovesCounter.gameObject.SetActive(state);
        }
    }

    public void EnableCollectionGoalLayout(bool state)
    {
        if(collectionGoalLayout != null)
        {
            collectionGoalLayout.gameObject.SetActive(state);
        }
    }
}
