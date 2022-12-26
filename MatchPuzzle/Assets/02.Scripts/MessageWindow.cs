using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
    public Image messageIcon;
    public TextMeshProUGUI messageTxt;
    public TextMeshProUGUI buttonTxt;

    public Sprite winIcon;
    public Sprite loseIcon;
    public Sprite goalIcon;

    public Image goalImage;
    public TextMeshProUGUI goalTxt;

    public Sprite collectionIcon;
    public Sprite timerIcon;
    public Sprite movesIcon;


    public void ShowMessage(Sprite sprite = null, string massage = "", string buttonMsg = "start")
    {
        if(messageIcon != null)
            messageIcon.sprite = sprite;
        if(messageTxt != null)
            messageTxt.text = massage;
        if(buttonMsg != null)
            buttonTxt.text = buttonMsg;
    }

    public void ShowWinMessage()
    {
        ShowMessage(winIcon, "level\ncomplete", "OK");
    }

    public void ShowLoseMessage()
    {
        ShowMessage(loseIcon, "level\nfailed","OK");
    }

    public void ShowGoal(int scoreGoal)
    {
        string message = "score goal \n" + scoreGoal.ToString();

        ShowMessage(goalIcon, message,"Start");

    }

    public void ShowGoal(string caption = "", Sprite icon = null)
    {
        if(goalTxt != null && caption != "")
        {
            goalTxt.text = caption;
        }

        if(goalImage != null && icon != null)
        {
            goalImage.sprite = icon;
        }
    }

    public void ShowTimeGoal(int time)
    {
        string caption = time.ToString() + " seconds";
        ShowGoal(caption,timerIcon);
    }

    public void ShowMovesGoal(int moves)
    {
        string caption = moves.ToString() + " moves";
        ShowGoal(caption,movesIcon);
    }

    public void ShowCollectionGoal()
    {
        ShowGoal("",goalIcon);
    }
}
