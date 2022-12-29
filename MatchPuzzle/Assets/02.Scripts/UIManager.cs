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

    public void SetupCollectionGoalLayout(CollectionGoal[] collectionGoals, GameObject goalLayout, int spacingWidth)
    {
        if (goalLayout != null && collectionGoals != null & collectionGoals.Length != 0)
        {
            RectTransform rectXform = goalLayout.GetComponent<RectTransform>();
            rectXform.sizeDelta = new Vector2(spacingWidth * collectionGoals.Length, rectXform.sizeDelta.y);

            CollectionGoalPanel[] panels = goalLayout.GetComponentsInChildren<CollectionGoalPanel>();

            for (int i = 0; i < panels.Length; i++)
            {
                if (i < collectionGoals.Length && collectionGoals[i] != null)
                {
                    panels[i].gameObject.SetActive(true);
                    panels[i].collectionGoal = collectionGoals[i];
                    panels[i].SetupPanel();
                }
                else
                {
                    panels[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetupCollectionGoalLayout(CollectionGoal[] collectionGoals)
    {
        SetupCollectionGoalLayout(collectionGoals, collectionGoalLayout, collectionGoalBaseWidth);
    }
    public void UpdateCollectionGoalLayout(GameObject goalLayout)
    {
        if(goalLayout != null)
        {
            CollectionGoalPanel[] panels = goalLayout.GetComponentsInChildren<CollectionGoalPanel>();
            if(panels != null && panels.Length != 0)
            {
                foreach(CollectionGoalPanel panel in panels)
                {
                    if(panel != null && panel.isActiveAndEnabled)
                    {
                        panel.UpdatePanel();
                    }
                }
            }
        }
    }
    public void UpdateCollectionGoalLayout()
    {
        // foreach(CollectionGoalPanel panel in m_collectionGoalPanels)
        // {
        //     if(panel != null && panel.isActiveAndEnabled)
        //     {
        //         panel.UpdatePanel();
        //     }
        // }
        UpdateCollectionGoalLayout(collectionGoalLayout);
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
