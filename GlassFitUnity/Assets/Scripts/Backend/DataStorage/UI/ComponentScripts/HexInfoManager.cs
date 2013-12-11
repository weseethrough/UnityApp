using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component which provides management functionality for dynamic hex screens
/// </summary>
public class HexInfoManager : MonoBehaviour
{
    enum State
    {
        Entering,        
        Exiting,        
    }

    Animation animation;
    string anmationName = "HexInfoEnter";

    UISprite    icon;
    UILabel     title;
    UILabel     content;
    
    public const string DV_HEX_DATA = "HexInfoDataBlock";

    float maximumDelay = 1.00f;
    float currentDelay;
    bool hexInfoRequired;

    State currentState;

    void Awake()
    {
        animation = GetComponent<Animation>();
        currentDelay = 0.0f;
        hexInfoRequired = false;
        currentState = State.Exiting;

        if (animation == null)
        {
            Debug.LogError("animation not found!");
        }
        else
        {
            animation[anmationName].time = 0.010f;
        }
    }

    void FindComponents()
    {
        GameObject go = GameObject.Find("HexInfoContent");
        if (go != null)
        {
            content = go.GetComponentInChildren<UILabel>();
        }

        go = GameObject.Find("HexInfoTitle");
        if (go != null)
        {
            title = go.GetComponentInChildren<UILabel>();
        }

        go = GameObject.Find("HexInfoIcon");
        if (go != null)
        {
            icon = go.GetComponentInChildren<UISprite>();
        }
    }

    void Update()
    {
        if (hexInfoRequired)
        {
            currentDelay += Time.deltaTime;
            if (currentDelay >= maximumDelay)
            {
                AnimEnter();
                hexInfoRequired = false;
            }
        }
    }

    public void PrepareForNewData()
    {
        HexButtonData data = DataVault.Get(DV_HEX_DATA) as HexButtonData;
        currentDelay = 0.0f;
        hexInfoRequired = data != null ? data.displayInfoData : false;
        AnimExit();
    }

    void AnimEnter()
    {
        HexButtonData data = DataVault.Get(DV_HEX_DATA) as HexButtonData;
        if (data != null)
        {            
            if (title == null)
            {
                FindComponents();
            }
            //at this stage we will crash if those components doesnt exist. We want to catch it as soon as possible
            title.text = data.activityName;
            content.text = data.activityContent;
            icon.spriteName = data.imageName;

            ActiveAnimation activeAnim = ActiveAnimation.Play(animation, anmationName, AnimationOrTween.Direction.Forward);
            activeAnim.Reset();
            animation[anmationName].time = 0.010f;
            currentState = State.Entering;
        }
    }

    public void AnimExit()
    {
        if (currentState == State.Exiting) return;
        currentState = State.Exiting;
        ActiveAnimation.Play(animation, anmationName, AnimationOrTween.Direction.Reverse);
    }

}
