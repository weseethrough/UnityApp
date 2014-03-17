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

    new Animation animation;
    string anmationName = "HexInfoEnter";
    
    GameObject buyNowGameObject;
    GameObject contentBackground;    
    
    public const string DV_HEX_DATA = "HexInfoDataBlock";

    float maximumDelay = 1.00f;
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
        GameObject go = GameObject.Find("BuyPrice");               
        if (go != null)
        {
            buyNowGameObject = go;
        }

        go = GameObject.Find("DescriptionBcg");
        if (go != null)
        {
            contentBackground = go;
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
            if (buyNowGameObject == null)
            {
                FindComponents();
            }            

            //title.text = ;
            //content.text = data.activityContent;
            //icon.spriteName = data.imageName;
            //textualIcon.text = data.textNormal;

            buyNowGameObject.SetActive(data.locked);                

            DataVault.Set("hex_info_title", data.activityName);
            DataVault.Set("hex_info_content", data.activityContent);
            DataVault.Set("current_activity_cost", data.activityPrice);

            if (data.locked)
            {
				string actionName;
				if(data.textOverlay == "Coming Soon") {
					actionName = DataVault.Translate("Coming Soon", null);
				} else {
                	actionName = DataVault.Translate("Tap to buy", null );
				}
                DataVault.Set("action_name", actionName);
                buyNowGameObject.SetActive(true);

                Vector3 pos = contentBackground.transform.localPosition;
                pos.x = 0;
                contentBackground.transform.localPosition = pos;
					
            }
            else if(data.textSmall != string.Empty) 
			{
				string actionName = DataVault.Translate("Tap to toggle", null);
				DataVault.Set("action_name", actionName);
				buyNowGameObject.SetActive(false);

                Vector3 pos = contentBackground.transform.localPosition;
                pos.x = -165;
                contentBackground.transform.localPosition = pos;
			} 
			else
            {
                string actionName = DataVault.Translate("Tap to launch", null );
                DataVault.Set("action_name", actionName);
                buyNowGameObject.SetActive(false);

                Vector3 pos = contentBackground.transform.localPosition;
                pos.x = -165;
                contentBackground.transform.localPosition = pos;
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


    public bool IsInOpenStage()
    {
        return State.Entering == currentState;
    }
}
