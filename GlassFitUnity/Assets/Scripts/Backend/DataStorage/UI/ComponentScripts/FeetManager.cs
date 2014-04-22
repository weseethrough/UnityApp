using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component which provides management functionality for Sensoria feet on hud display
/// </summary>
public class FeetManager : MonoBehaviour
{
    float[] data = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

    string[] prefixes = { "B", "A", "C" };

    UISprite[] footL = new UISprite[3];
    UISprite[] footR = new UISprite[3];

    void Start()
    {

        UISprite[] sprites = GetComponentsInChildren<UISprite>();
        foreach(UISprite sprite in sprites)
        {
            string name = sprite.gameObject.name;
            string parent = sprite.transform.parent.name;
            
            switch (name)
            {
                case "Toe1":
                    if (parent == "FootL")
                    {
                        footL[0] = sprite;
                    }
                    else
                    {
                        footR[0] = sprite;
                    }
                    break;

                case "Toe5":
                    if (parent == "FootL")
                    {
                        footL[1] = sprite;
                    }
                    else
                    {
                        footR[1] = sprite;
                    }
                    break;
                
                case "Heel":
                    if (parent == "FootL")
                    {
                        footL[2] = sprite;
                    }
                    else
                    {
                        footR[2] = sprite;
                    }
                    break;
            }
        }

        foreach(UISprite sprite in footL)
        {
            sprite.alpha = 0.0f;
        }

        foreach (UISprite sprite in footR)
        {
            sprite.alpha = 0.0f;
        }
    }

    void Update()
    {
        data = Platform.Instance.sensoriaSockPressure;
        if (data == null) return;

        for (int i=0; i<footL.Length; i++)
        {
            SelectByFloat(data[i], footL[i], prefixes[i]);
        }

        for (int i = 0; i < footR.Length; i++)
        {
            SelectByFloat(data[i + footL.Length], footR[i], prefixes[i]);
        }
    }

    void SelectByFloat(float status, UISprite target, string prefix)
    {
        if (target == null) return;

        int stateIndex = (int)(status * 7.0f);
        stateIndex = Mathf.Clamp(stateIndex, 0, 6);
        if (stateIndex == 0 )
        {
            target.alpha = 0.0f;
        }
        else
        {
            target.alpha = 1.0f;
            target.spriteName = prefix + stateIndex;
        }
    }
}
