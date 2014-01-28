using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// basic panel which allows to show ui
/// </summary>
[Serializable]
public class MultiPanel : Panel
{
    private List<MultiPanelChild> managedChildren;
    private float dragTime;
    private float dragOffset;    
    private Vector3 firstGliph = Vector3.zero;
    private TweenPosition firstTweener;
    private float firstOffset = 0.0f;
    private Vector2 firstTouchPosition;
	
	private static int SCREEN_WIDTH = 1440;

    //pointer to child which is the last one considered when navigating right. 
    private MultiPanelChild maxChildProgress;

    private MultiPanelChild focusedChildPanel;


    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public MultiPanel() : base() { }

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public MultiPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
    }

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialziation info where all data would be pushed to</param>
    /// <param name="ctxt">serialzation context</param>
    /// <returns></returns>
    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        base.GetObjectData(info, ctxt);
   	}

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "MultiPanel: " + gName.Value;
        }
        return "MultiPanel: UnInitialzied";
    }


    /// <summary>
    /// Initializes default variables including setting which blocks swiping more than single panel
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        NewParameter("AllowSingleDragOnly", GraphValueType.Boolean, "true");
    }

    /// <summary>
    /// Enter function which allows to enter all managed children so they are ready for swiping
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
        base.EnterStart();
        
        managedChildren = new List<MultiPanelChild>();

        foreach (FlowState fs in m_children)
        {
            if ( fs is MultiPanelChild )
            {                
                MultiPanelChild child = fs as MultiPanelChild;
                child.ManagedEnter();
                child.parentMachine = parentMachine;
                bool inserted = false;

                for (int i = 0; i < managedChildren.Count; i++)
                {
                    if (child.GetOrder() < managedChildren[i].GetOrder())
                    {
                        managedChildren.Insert(i, child);
                        inserted = true;
                        break;
                    }
                }

                if (!inserted)
                {
                    managedChildren.Add(child);
                }                                
            }
        }
		
		//set the datavault value so that the paging indicator knows the number of pages.
		DataVault.Set("numberOfPages", managedChildren.Count);		
        
        ArangeScreens();

        focusedChildPanel = managedChildren[0];
    }

    /// <summary>
    /// Exit function which calls it on all managed children as well
    /// </summary>
    /// <returns></returns>
    public override void Exited()
    {
        base.Exited();

        foreach (MultiPanelChild child in managedChildren)
        {           
            child.ManagedExit();           
        }
    }

    /// <summary>
    /// regular flow state update responsible for swipe /input processing
    /// </summary>
    /// <returns></returns>
    public override void StateUpdate()
    {
        base.StateUpdate();        
        Vector3 pos = Vector3.zero;

        Vector2? touch = Platform.Instance.GetTouchInput();

        //dragging
        if (touch.HasValue)
        {
            
            if (dragTime == 0.0f)
            {
                firstOffset = managedChildren[0].physicalWidgetRoot.transform.position.x;
                firstTouchPosition = touch.Value;
            }

            dragTime += Time.deltaTime;
            dragOffset = firstTouchPosition.x - touch.Value.x; 

            pos.x = (-dragOffset * SCREEN_WIDTH) + firstOffset;
            managedChildren[0].physicalWidgetRoot.transform.position = pos;

            if (firstTweener!=null)
            {
                firstTweener = null;
            }
        }
        //finished dragging
        else if (dragOffset != 0)
        {
            float acceleratedPosition = dragTime > 0 ? (dragOffset / dragTime) : 0.0f;            
            float animatedPosition = acceleratedPosition - managedChildren[0].physicalWidgetRoot.transform.position.x / SCREEN_WIDTH;

            int maxIndex = maxChildProgress != null ? managedChildren.IndexOf(maxChildProgress) : managedChildren.Count-1;
            if (maxIndex < 0) 
            {
                maxIndex = managedChildren.Count - 1;
            }

            GParameter allowSingleDragOnly = Parameters.Find(r => r.Key == "AllowSingleDragOnly");

            if (allowSingleDragOnly == null || allowSingleDragOnly.Value == "true")
            {
                int oldIndex = managedChildren.IndexOf(focusedChildPanel);
                if (oldIndex != -1)
                {
                    //ensure value is not bigger than old +1 and not smaller than old -1
                    animatedPosition = Mathf.Max(oldIndex - 1, Mathf.Min(animatedPosition, oldIndex + 1));
                }
            }

            int index = Mathf.Max(0, Mathf.Min(maxIndex, (int)(animatedPosition + 0.5f)));
            Debug.Log("Animated position: " + animatedPosition);
            pos.x = (-index * SCREEN_WIDTH);
			
	    //Set current page in the data vault, for paging indicator
	    DataVault.Set("currentPageIndex", index);
			
            firstTweener = TweenPosition.Begin(managedChildren[0].physicalWidgetRoot, 0.3f, pos);
            firstTweener.delay = 0.0f;
   
            dragOffset = 0.0f;
            firstOffset = 0.0f;
            dragTime = 0.0f;

            focusedChildPanel = managedChildren[index];
        }

        ArangeScreens();   
     
        if (focusedChildPanel != null)
        {
            focusedChildPanel.StateUpdate();
        }
    }

    /// <summary>
    /// Position panels for later scrolling, arranges them in a row
    /// </summary>
    /// <returns></returns>
    public void ArangeScreens()
    {
        firstGliph = managedChildren[0].physicalWidgetRoot.transform.position;

        for (int i =0; i< managedChildren.Count; i++)
        {
            Vector3 pos = Vector3.zero;
            if (i != 0)
            {
                pos.x = ((i) * SCREEN_WIDTH) + firstGliph.x;                
                managedChildren[i].physicalWidgetRoot.transform.position = pos;
            }
        }
    }

    /// <summary>
    /// Method setting max right child allowed for navigation.
    /// </summary>
    /// <param name="child">child which is last allowed during right navigation. Null allows to navigate to avaliable last item</param>
    /// <returns></returns>
    public void SetLastAvaliableChild(MultiPanelChild child)
    {
        maxChildProgress = child;
    }
   
    /// <summary>
    /// Returns child which is focused (or panel is navigating to)
    /// </summary>
    /// <returns></returns>
    public MultiPanelChild GetFocusedChild()
    {       
        return focusedChildPanel;
    }
}
