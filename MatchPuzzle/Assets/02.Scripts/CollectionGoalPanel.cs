using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionGoalPanel : MonoBehaviour
{
    public CollectionGoal collectionGoal;
    public TextMeshProUGUI numberLeftTxt;
    public Image prefabImage;
    private void Start()
    {
        SetupPanel();
    }
    public void SetupPanel()
    {
        if(collectionGoal != null && numberLeftTxt != null && prefabImage != null)
        {
            SpriteRenderer prefabSprite = collectionGoal.prefabToCollect.GetComponent<SpriteRenderer>();
            if(prefabSprite != null)
            {
                prefabImage.sprite = prefabSprite.sprite;
                prefabImage.color = prefabSprite.color;
            }
            numberLeftTxt.text = collectionGoal.numberToCollect.ToString();
        }
    }
    public void UpdatePanel()
    {
        if(collectionGoal != null && numberLeftTxt != null)
        {
            numberLeftTxt.text = collectionGoal.numberToCollect.ToString();
        }
    }
}
