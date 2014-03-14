using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Displays friend list with available challenges
/// </summary>
public class MobileList : UIComponentSettings
{
    MobilePanel parent;

        
    private List<GameObject> buttons = new List<GameObject>();

    private bool initialized = false;

    private GameObject buttonInstance;
    private GameObject listContent;
    private GameObject listHeader;

    private string title = "NoTitle";

    private GraphComponent gComponent;

    void Start()
    {
        gComponent                  = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        listHeader                  = GameObjectUtils.SearchTreeByName(gameObject, "ListHeader");
        listContent                 = GameObjectUtils.SearchTreeByName(gameObject, "ListContent");
        Debug.Log("listHeader " + listHeader);
        Debug.Log("listContent " + listContent);

        buttonInstance              = listContent.transform.GetChild(0).gameObject;

        SetTitle(title);
        buttonInstance.SetActive(false);

        initialized = true;
        RebuildList();             
    }

    public void SetTitle(string title)
    {
        if (listHeader != null)
        {
            GameObjectUtils.SetTextOnLabelInChildren(listHeader, "listTitle", title);
        }

        this.title = title;
    }

    public void SetParent(MobilePanel parent)
    {
        this.parent = parent;
    }

    public void RebuildList()
    {

        if (!initialized)
        {
            return;
        }

        if (parent == null)
        {
            //Debug.LogError("panel not identified! ensure panel is set to one of MobilePanel types");
            return;
        }

        buttons = new List<GameObject>();

        Vector3 headerPos = listHeader.transform.localPosition;                
        List<ListButtonData> buttonData = parent.GetButtonData();
        if (buttonData != null && buttonData.Count > 0)
        {
            for (int i = 0; i < buttonData.Count; i++)
            {
                GameObject button;
                if (i == 0)
                {
                    buttonInstance.SetActive(true);
                    button = buttonInstance;
                }
                else
                {
                    button = GameObject.Instantiate(buttonInstance) as GameObject;
                }

                button.transform.parent = buttonInstance.transform.parent;
                button.transform.localScale = buttonInstance.transform.localScale;
                button.name = buttonData[i].buttonName;

                buttons.Add(button);

                UIButton buttonScript = button.GetComponentInChildren<UIButton>();
                if (buttonScript != null)
                {
                    FlowButton fb = buttonScript.gameObject.AddComponent<FlowButton>();
                    fb.owner = parent;
                    fb.name = buttonData[i].buttonName;
                }

                GameObjectUtils.SetTextOnLabelInChildren(button, "title", buttonData[i].textNormal);
                GameObjectUtils.SetTextOnLabelInChildren(button, "content", buttonData[i].buttonName);

                Debug.Log("AddButton " + buttonData[i].textNormal + " btName: " + buttonData[i].buttonName);
            }

            UIGrid grid = listContent.GetComponent<UIGrid>();
            if (grid != null)
            {
                grid.Reposition();
            }
        }
    }    
}
