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
    GameObject  buyNowBcg;
    GameObject  buyNowText;
    GameObject  buyNowPrice;
    
    public const string DV_HEX_DATA = "HexInfoDataBlock";

    float maximumDelay = 1.50f;
    float currentDelay;
    bool hexInfoRequired;

    State currentState;

    /// <summary>
    /// Default unity initialziation function used to setup animation to frame 1 and reseting variables
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Function used after whole structore is initialized to find components which are changed during runtime
    /// </summary>
    /// <returns></returns>
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

        go = GameObject.Find("BuyNowBcg");
        if (go != null)
        {
            buyNowBcg = go;
        }

        go = GameObject.Find("HexInfoBuyNow");
        if (go != null)
        {
            buyNowText = go;
        }

        go = GameObject.Find("HexInfoBuyCost");
        if (go != null)
        {
            buyNowPrice = go;
        }
    }

    /// <summary>
    /// Default unity function used to call enter animation when needed
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// triggers leaving previous info box and prepares for new one
    /// </summary>
    /// <returns></returns>
    public void PrepareForNewData()
    {
        HexButtonData data = DataVault.Get(DV_HEX_DATA) as HexButtonData;
        currentDelay = 0.0f;
        hexInfoRequired = data != null ? data.displayInfoData : false;
        AnimExit();
    }

    /// <summary>
    /// Triggers animation and setup required data for new info panel
    /// </summary>
    /// <returns></returns>
    void AnimEnter()
    {
        HexButtonData data = DataVault.Get(DV_HEX_DATA) as HexButtonData;
        if (data != null)
        {            
            if (title == null)
            {
                FindComponents();
            }
            //at this stage we will crash if those components doesn't exist. We want to catch it as soon as possible
            title.text = data.activityName;
            content.text = data.activityContent;
            icon.spriteName = data.imageName;

            if (data.locked == true)
            {
                buyNowBcg.SetActive(true);
                buyNowText.SetActive(true); 
                buyNowPrice.SetActive(true);

                UILabel label = buyNowPrice.GetComponentInChildren<UILabel>();
                if (label != null)
                {
                    label.text = data.activityPrice + " RP";
                }
            }
            else
            {
                buyNowBcg.SetActive(false);
                buyNowText.SetActive(false);
                buyNowPrice.SetActive(false); 
            }

            ActiveAnimation activeAnim = ActiveAnimation.Play(animation, anmationName, AnimationOrTween.Direction.Forward);
            activeAnim.Reset();
            animation[anmationName].time = 0.010f;
            currentState = State.Entering;
        }
    }

    /// <summary>
    /// triggers animation out for info box
    /// </summary>
    /// <returns></returns>
    public void AnimExit()
    {
        if (currentState == State.Exiting) return;
        currentState = State.Exiting;
        ActiveAnimation.Play(animation, anmationName, AnimationOrTween.Direction.Reverse);
    }

}
