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

    public void ShowMessage(Sprite sprite = null, string massage = "", string buttonMsg = "start")
    {
        if(messageIcon != null)
            messageIcon.sprite = sprite;
        if(messageTxt != null)
            messageTxt.text = massage;
        if(buttonMsg != null)
            buttonTxt.text = buttonMsg;

    }
}
