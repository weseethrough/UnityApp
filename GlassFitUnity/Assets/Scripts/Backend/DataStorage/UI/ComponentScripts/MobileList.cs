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

    private Dictionary<string, GameObject> buttonPrototypes;
    private GameObject listContent;
    private GameObject listHeader;

    private string title = "";

    private GraphComponent gComponent;

    void Start()
    {
        gComponent                  = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        listHeader                  = GameObjectUtils.SearchTreeByName(gameObject, "ListHeader");
        listContent                 = GameObjectUtils.SearchTreeByName(gameObject, "ListContent");
        Debug.Log("listHeader " + listHeader);
        Debug.Log("listContent " + listContent);

        buttonPrototypes = GetPrototypes(listContent);

        SetTitle(title);
 
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
                GameObject prototype;
                GameObject button;

                prototype = buttonPrototypes[buttonData[i].buttonFormat];
                button = GameObject.Instantiate(prototype) as GameObject;
                button.SetActive(true);

                button.transform.parent = prototype.transform.parent;
                button.transform.localScale = prototype.transform.localScale;
                button.name = buttonData[i].buttonName;

                buttons.Add(button);

                UIButton buttonScript = button.GetComponentInChildren<UIButton>();
                if (buttonScript != null)
                {
                    FlowButton fb = buttonScript.gameObject.GetComponent<FlowButton>();
                    if (fb == null)
                    {
                        buttonScript.gameObject.AddComponent<FlowButton>();
                    }
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

    Dictionary<string, GameObject> GetPrototypes(GameObject root)
    {
        Dictionary<string, GameObject> collection = new Dictionary<string, GameObject>();
        foreach(Transform t in root.transform)
        {
            collection[t.name] = t.gameObject;
            t.gameObject.SetActive(false);
        }

        return collection;
    }

	public void RemoveButtons() {
		foreach(Transform t in listContent.transform) {
			if(t.gameObject.activeSelf) {
				Destroy(t.gameObject);
			}
		}
	}
}
