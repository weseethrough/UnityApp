using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component which provides management functionality for dynamic hex screens
/// </summary>
public class HexInfoManager : MonoBehaviour
{
    Animation animation;
    string anmationName = "HexInfoEnter";

    UISprite    icon;
    UILabel     title;
    UILabel     content;

    const string DV_ICON_NAME = "HexMenuIconName";
    const string DV_TITLE_NAME = "HexMenuTitleName";
    const string DV_CONTENT_NAME = "HexMenuContentName";


    void Awake()
    {
        animation = GetComponent<Animation>();

        if (animation == null)
        {
            Debug.LogError("animation not found!");
        }
        else
        {
            animation[anmationName].time = 0.000f;
        }

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

    void AnimEnter()
    {        
        title.text = DataVault.Get(DV_TITLE_NAME).ToString();
        content.text = DataVault.Get(DV_CONTENT_NAME).ToString();
        icon.spriteName = DataVault.Get(DV_ICON_NAME).ToString();

        ActiveAnimation activeAnim = ActiveAnimation.Play(animation, anmationName, AnimationOrTween.Direction.Forward);
        activeAnim.Reset();
    }

    void AnimExit()
    {
        ActiveAnimation.Play(animation, anmationName, AnimationOrTween.Direction.Reverse);
    }

}
