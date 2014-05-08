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
    private int itemCountBeforeTop = 4;
    private int itemsToManage = 14;

    private List<GameObject> buttons = new List<GameObject>();
    private int previousStartIndex = 0;
    private int previousCount = 0;
    private bool initialized = false;
    private Dictionary<string, GameObject> buttonPrototypes;
    private GameObject listContent;
    private GameObject listHeader;
    private string title = "NoTitle";
    private GraphComponent gComponent;

    private Dictionary<string, List<GameObject>> instances = new Dictionary<string, List<GameObject>>();

    private float defayltYOffset;

    void Start()
    {
        gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        listHeader = GameObjectUtils.SearchTreeByName(gameObject, "ListHeader");
        listContent = GameObjectUtils.SearchTreeByName(gameObject, "ListContent");
        Debug.Log("listHeader " + listHeader);
        Debug.Log("listContent " + listContent);

        buttonPrototypes = GetPrototypes(listContent);

        defayltYOffset = listContent.GetComponent<UIGrid>().transform.position.y;

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

    void Update()
    {
        if (parent == null) return;
        List<ListButtonData> buttonData = parent.GetButtonData();
        UIGrid grid = listContent.GetComponent<UIGrid>();

        Vector3 pos = grid.transform.position;
        float itemHeight = grid.cellHeight;
        float position = pos.y - defayltYOffset;
        int start = -itemCountBeforeTop + (int)(position / itemHeight);

        Reposition(buttonData, start, itemsToManage);
    }

    public void Reposition(List<ListButtonData> items, int itemOffset, int itemCount)
    {

        int min = Mathf.Min(itemOffset, previousStartIndex);
        int max = Mathf.Max(itemOffset + itemCount, previousStartIndex + previousCount);

        UIGrid grid = listContent.GetComponent<UIGrid>();
        Transform transform = grid.transform;

        List<GameObject> newButtons = new List<GameObject>();

        /*int x = 0;
        int y = 0;*/
        int i;

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

                if (i >= previousStartIndex && i < previousStartIndex + previousCount)
                {
                    item = buttons[0];
                    buttons.RemoveAt(0);
                }
                else
                {
                    item = GetNewButton(items[i], newButtons);
                }
                newButtons.Add(item);

                Vector3 p = item.transform.localPosition;
                p.y = -grid.cellHeight * i;
                p.x = 0;

                item.transform.localPosition = p;

            }
            else
            {
                if (i >= previousStartIndex && i < previousStartIndex + previousCount)
                {
                    buttons[0].SetActive(false);
                    buttons.RemoveAt(0);
                }
            }
        }

        buttons = newButtons;

        previousStartIndex = itemOffset;
        previousCount = itemCount;

        UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
        if (drag != null) drag.UpdateScrollbars(true);
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

        return button;
    }


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

    public void ResetList(float newItemHeight)
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
        previousStartIndex = 0;
        previousCount = 0;

        UIGrid grid = listContent.GetComponent<UIGrid>();
        grid.cellHeight = newItemHeight;
        Vector3 pos = grid.transform.position;
        pos.y = 0;
        grid.transform.position = pos;
    }
}
