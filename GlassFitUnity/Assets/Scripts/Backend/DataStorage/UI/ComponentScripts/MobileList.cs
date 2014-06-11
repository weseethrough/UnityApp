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
	string listName = "default";
    private int itemCountBeforeTop = 100;
    private int itemsToManage = 200;

    private List<GameObject> buttons = new List<GameObject>();
    private int previousStartIndex = 0;
    private int previousCount = 0;
    private bool initialized = false;
    private Dictionary<string, GameObject> buttonPrototypes;
    private GameObject listContent;
    private GameObject listHeader;

    private string title = "";

    private GraphComponent gComponent;

    private Dictionary<string, List<GameObject>> instances = new Dictionary<string, List<GameObject>>();
	private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    private float defaultYOffset;

    void Awake()
    {
//		UIDraggablePanel drag = GetComponentInChildren<UIDraggablePanel>();
//		if (drag != null) drag.ResetPosition();
		gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        listHeader = GameObjectUtils.SearchTreeByName(gameObject, "ListHeader");
        listContent = GameObjectUtils.SearchTreeByName(gameObject, "ListContent");
        Debug.Log("listHeader " + listHeader);
        Debug.Log("listContent " + listContent);

        buttonPrototypes = GetPrototypes(listContent);

        defaultYOffset = listContent.GetComponent<UIGrid>().transform.localPosition.y;

        SetTitle(title);

        initialized = true;
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

	public void SetList(string list)
	{
		this.listName = list;
	}

    void Update()
    {
        if (parent == null) return;
        List<ListButtonData> buttonData = parent.GetButtonData(listName);
        UIGrid grid = listContent.GetComponent<UIGrid>();

        Vector3 pos = grid.transform.position;
        float itemHeight = grid.cellHeight;
        float position = pos.y - defaultYOffset;
        int start = -itemCountBeforeTop + (int)(position / itemHeight);

        Reposition(buttonData, start, itemsToManage);
    }

    public void Reposition(List<ListButtonData> items, int itemOffset, int itemCount)
    {
        bool requiresResetMask = false;

        if (previousCount == 0)
        {
            requiresResetMask = true;
        }

        int min = Mathf.Min(itemOffset, previousStartIndex);
        int max = Mathf.Max(itemOffset + itemCount, previousStartIndex + previousCount);

        UIGrid grid = listContent.GetComponent<UIGrid>();
        Transform transform = grid.transform;

        List<GameObject> newButtons = new List<GameObject>();

        /*int x = 0;
        int y = 0;*/
        int i;

//		UIDraggablePanel drag = GetComponentInChildren<UIDraggablePanel>();
//		if (drag != null) drag.ResetPosition();
		
		for (i = min; i < max; i++)
        {
            //do not try to manage items off the list
            if (i < 0 || i >= items.Count) continue;

            bool makeVisible = false;

            if (i >= itemOffset && i < itemOffset + itemCount)
            {
                makeVisible = true;
            }

            if (makeVisible)
            {
                GameObject item;

//				if(buttons.Count > 0){
                if (i >= previousStartIndex && i < previousStartIndex + previousCount && buttons.Count > 0)
                {
                    item = buttons[0];
                    buttons.RemoveAt(0);
                }
                else
                {
                    item = GetNewButton(items[i], newButtons);
                }
                newButtons.Add(item);

                GameObject[] theList = newButtons.ToArray();
                StaticBatchingUtility.Combine(theList, listHeader);

                Vector3 p = item.transform.localPosition;
                p.y = -grid.cellHeight * i;
                p.x = 0;

                item.transform.localPosition = p;
//				}
            }
            else
            {
				if (i >= previousStartIndex && i < previousStartIndex + previousCount && buttons.Count > 0)
                {
					UITexture[] textures = buttons[0].GetComponentsInChildren<UITexture>();
					foreach(UITexture texture in textures)
					{
						texture.mainTexture = null;
					}
                    buttons[0].SetActive(false);
                    buttons.RemoveAt(0);
                }
            }
        }

        buttons = newButtons;

        previousStartIndex = itemOffset;
        previousCount = itemCount;

        UIDraggablePanel drag = GetComponentInChildren<UIDraggablePanel>();
        if (drag != null)
        {                        
            if (requiresResetMask)
            {
                drag.relativePositionOnReset = Vector2.zero;
                drag.ResetPosition();
            }

            drag.UpdateScrollbars(true);            
        }
    }

    private GameObject GetNewButton(ListButtonData data, List<GameObject> newListButtons)
    {
        GameObject prototype = null;
        GameObject button = null;

        List<GameObject> existingInstances = null;

        if (instances.ContainsKey(data.buttonFormat))
        {
            existingInstances = instances[data.buttonFormat];
        }
        else
        {
            existingInstances = new List<GameObject>();
            instances[data.buttonFormat] = existingInstances;
        }

        for (int i = 0; i < existingInstances.Count; i++)
        {
            //check if button were used in previous or new list. if not then we can use it
            int index = Mathf.Max(buttons.IndexOf(existingInstances[i]), newListButtons.IndexOf(existingInstances[i]));
            if (index == -1)
            {
                button = existingInstances[i];
                break;
            }
        }

        if (button == null)
        {
            prototype = buttonPrototypes[data.buttonFormat];
            button = GameObject.Instantiate(prototype) as GameObject;
            button.SetActive(true);
            existingInstances.Add(button);

            button.transform.parent = prototype.transform.parent;
            button.transform.localScale = prototype.transform.localScale;
        }

        button.SetActive(true);
        button.name = data.buttonName;

        UIButton buttonScript = button.GetComponentInChildren<UIButton>();
        if (buttonScript != null)
        {
            FlowButton fb = buttonScript.gameObject.GetComponent<FlowButton>();
            if (fb == null)
            {
                buttonScript.gameObject.AddComponent<FlowButton>();
            }
            fb.owner = parent;
            fb.name = data.buttonName;
        }

		GameObjectUtils.SetTextOnLabelInChildren(button, "title", data.textNormal);
		GameObjectUtils.SetTextOnLabelInChildren(button, "content", data.buttonName);

		if(data.textDictionary != null) {
			foreach(var key in data.textDictionary.Keys) {
				GameObjectUtils.SetTextOnLabelInChildren(button, key, data.textDictionary[key]);
			}
		}

		if(data.imageDictionary != null) 
		{
			foreach(var key in data.imageDictionary.Keys) {
				string textureUrl = key;
				if (textureCache.ContainsKey(key)) {
					ImageCallback(textureCache[key], data.imageDictionary[key]);
					continue;
				}

				data.imageDictionary[key]["url"] = textureUrl;
				Platform.Instance.RemoteTextureManager.LoadImage(textureUrl, data.imageDictionary[key], ImageCallback);
			}
//			Platform.Instance.RemoteTextureManager.LoadImage(data.imageName, data.buttonName, (tex, buttonId) => {
//				Panel fs = FlowStateMachine.GetCurrentFlowState() as Panel;
//				GameObject foundButton = GameObjectUtils.SearchTreeByName(fs.physicalWidgetRoot, buttonId);
//				if(foundButton != null) {
//					foundButton.GetComponentInChildren<UITexture>().mainTexture = tex;
//				}
//			});

		}

//                Debug.Log("AddButton " + data.textNormal + " btName: " + buttonData[i].buttonName);
        return button;
	}

	private void ImageCallback(Texture2D tex, object callbackArgument) {
		Panel panel = FlowStateMachine.GetCurrentFlowState() as Panel;
        var dictionary = callbackArgument as Dictionary<string, string>;
		if (dictionary.ContainsKey("url")) textureCache[dictionary["url"]] = tex;
		try {
			GameObject buttonObj = GameObjectUtils.SearchTreeByName(panel.physicalWidgetRoot, dictionary["name"]);
			if(buttonObj != null) {
				GameObject textureObj = GameObjectUtils.SearchTreeByName(buttonObj, dictionary["texture"]);
				UITexture texture = textureObj.GetComponent<UITexture>();
				if(texture != null) {                               
					texture.mainTexture = tex;
				}
			}
		} catch (Exception e) {
			// probably switched screen so panel or widget root is no longer valid -> Null Exception of some kind
			Debug.LogWarning("MobileList.getNewButton() " + e.Message);
		}
	}
	
	//            UIGrid grid = listContent.GetComponent<UIGrid>();
	//            if (grid != null)
	//            {
    //                grid.Reposition();
    //            }
    //        }
    //    }    
    
    Dictionary<string, GameObject> GetPrototypes(GameObject root)
    {
        Dictionary<string, GameObject> collection = new Dictionary<string, GameObject>();
        foreach (Transform t in root.transform)
        {
            collection[t.name] = t.gameObject;
            t.gameObject.SetActive(false);
        }

        return collection;
    }

	public float GetItemHeight() 
	{
		UIGrid grid = listContent.GetComponent<UIGrid>();
		return grid.cellHeight;
	}

    public void ClearList()
    {

        foreach (KeyValuePair<string, List<GameObject>> list in instances)
        {
            foreach ( GameObject item in list.Value)
            {
                Destroy(item);
            }
        }

        //clear defaults
        buttons = new List<GameObject>();
        instances = new Dictionary<string, List<GameObject>>();
	}

	public void ResetList(float newItemHeight)
	{
		ClearList();

        UIGrid grid = listContent.GetComponent<UIGrid>();
        grid.cellHeight = newItemHeight;
        Vector3 pos = grid.transform.localPosition;
        pos.y = defaultYOffset;
        pos.x = 0;
		grid.transform.localPosition = pos;    

//		defaultYOffset = 
        
       /* UIPanel panel = NGUITools.FindInParents<UIPanel>(gameObject);

        pos = panel.transform.position;
        pos.x = 0;
        panel.transform.position = pos;

        UIDraggablePanel drag = GetComponentInChildren<UIDraggablePanel>();
        if (previousCount == 0)
        {
            if (drag != null)
            {
                drag.relativePositionOnReset = Vector2.zero;
                drag.ResetPosition();
            }
        }*/
        
    }
}
