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
    private int currentSelection;
    private Vector3 firstGliph = Vector3.zero;
    private TweenPosition firstTweener;
    private float firstOffset = 0.0f;

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
    /// Enter function which allows to enter all managed children so they are ready for swiping
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
        base.EnterStart();

        currentSelection = -1;
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
        
        ArangeScreens();
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

    public override void StateUpdate()
    {
        base.StateUpdate();        
        Vector3 pos = Vector3.zero;
        
        if (Input.touchCount == 1)
        {
            
            if (dragOffset == 0.0f)
            {
                firstOffset = managedChildren[0].physicalWidgetRoot.transform.position.x;
            }

            dragTime += Input.touches[0].deltaTime;
            dragOffset += 10.0f * Input.touches[0].deltaPosition.x / Screen.width;            
            

            pos.x = 0.0018f * (dragOffset * Screen.width) + firstOffset;
            managedChildren[0].physicalWidgetRoot.transform.position = pos;

            if (firstTweener!=null)
            {
                firstTweener = null;
            }
        }

        if (Input.touchCount != 1 && dragOffset != 0)
        {

            float acceleratedPosition = dragTime > 0 ? (dragOffset * 0.1f / dragTime) : 0.0f;
            DataVault.Set("count", "accel " + acceleratedPosition);
            float animatedPosition = - acceleratedPosition - managedChildren[0].physicalWidgetRoot.transform.position.x / (0.0018f * Screen.width);
            int index = Mathf.Max(0, Mathf.Min(managedChildren.Count-1, (int)(animatedPosition + 0.5f)));
            pos.x = 0.0018f * (-index * Screen.width);
            firstTweener = TweenPosition.Begin(managedChildren[0].physicalWidgetRoot, 0.3f, pos);
   
            dragOffset = 0;
            firstOffset = 0.0f;
            dragTime = 0.0f;
        }

        ArangeScreens();        
    }

    public void ArangeScreens()
    {
        firstGliph = managedChildren[0].physicalWidgetRoot.transform.position;

        for (int i =0; i< managedChildren.Count; i++)
        {
            Vector3 pos = Vector3.zero;
            if (i != 0)
            {            
                pos.x = 0.0018f * ((i) * Screen.width) + firstGliph.x;
                managedChildren[i].physicalWidgetRoot.transform.position = pos;
            }
        }
    }
}
