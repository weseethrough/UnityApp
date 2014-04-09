using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

/// <summary>
/// single generic state of the flow
/// </summary>
public abstract class FlowState : GNode
{
    public enum State
    {
        Entering,
        Idle,
        Exiting,
        Dead
    }
    
    public FlowStateMachine parentMachine;

	/// <summary>
	/// Follows the named flow link.
	/// </summary>
	/// <returns><c>true</c>, if named flow link was followed, <c>false</c> otherwise.</returns>
	/// <param name="linkName">Link name.</param>
	public static bool FollowFlowLinkNamed(string linkName)
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gc = fs.Outputs.Find( r => r.Name == linkName);
		if(gc != null)
		{
			if(gc.EventFunction != null && gc.EventFunction.Length > 0 && gc.EventFunction != "") {
				(gc.Parent as FlowState).CallStaticFunction(gc.EventFunction, null);
			}
			
			fs.parentMachine.FollowConnection(gc);
			return true;
		}
		else
		{
			UnityEngine.Debug.LogWarning("FlowState: Didn't find flow link named " + linkName);
			return false;
		}
	}

	/// <summary>
	/// Follows the back link.
	/// </summary>
	public static void FollowBackLink()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		fs.parentMachine.FollowBack();
	}


    // do not let state switch outside of order. It might cause some states get unplugged from order list and result with unpredictable results    
    protected State m_state;
    protected FlowState m_parent;    
    protected List<FlowState> m_children;
    protected Vector2 m_minimumChildBorder = new Vector2(100, 40);
    protected Vector2 m_toParentOffest;
    protected float m_enterTimeStamp;

    public UnityEngine.Vector2 ParentOffest
    {
        get { return m_toParentOffest; }
        set { m_toParentOffest = value; }
    }

    public FlowState.State state
    {
        get { return m_state; }        
    }
    public List<FlowState> children
    {
        get
        {
            if (m_children == null) m_children = new List<FlowState>();
            return m_children;
        }
    }
    public FlowState parent
    {
        get { return m_parent; }
        set { m_parent = value; }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public FlowState() : base() { }

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public FlowState(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
	{        
        foreach (SerializationEntry entry in info)
        {
            switch (entry.Name)
            {
                case "parent":
                    this.m_parent = entry.Value as FlowState;
                    break;
                case "children":
                    this.m_children = entry.Value as List<FlowState>;
                    break;
                case "ParentOffsetX":
                    this.m_toParentOffest.x = (float)entry.Value;
                    break;
                case "ParentOffsetY":
                    this.m_toParentOffest.y = (float)entry.Value;
                    break;                
            }
        }
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

        info.AddValue("parent", this.m_parent);
        info.AddValue("children", this.m_children);
        info.AddValue("ParentOffsetX", this.m_toParentOffest.x);
        info.AddValue("ParentOffsetY", this.m_toParentOffest.y);
   	}


    //collection of virtual functions for different states of the panel flow
    virtual public void EnterStart() 
    { 
        m_state = State.Entering; 
        m_enterTimeStamp = Time.realtimeSinceStartup; 
        Debug.Log("Enter state: " + this.ToString()); 
    }
    virtual public bool EnterUpdate() { return true; }
    virtual public void Entered() 
    { 
        m_state = State.Idle; 
        Debug.Log("Entered state: " + this.ToString()); 
    }

    virtual public void StateUpdate() {  }

    virtual public void ExitStart() 
    { 
        m_state = State.Exiting; 
        Debug.Log("Exit state: " + this.ToString()); 
    }
    virtual public bool ExitUpdate() { return true; }
    virtual public void Exited() 
    { 
        m_state = State.Dead; 
        Debug.Log("Exited state: " + this.ToString()); 
    }   

    /// <summary>
    /// default draw function for draw on graph viewer
    /// </summary>
    /// <param name="r">space to draw node in</param>
    /// <returns></returns>
    public override void OnDraw(Rect r)
    {
        if (IsEvaluated)
        {            
            GL.Begin(GL.QUADS);
            GL.Color(Color.blue);
            float x0 = Position.x + 32;
            float x1 = Position.x + Size.x - 32;
            float y0 = Position.y + 32;
            float y1 = Position.y + Size.y - 16;
            GL.Vertex3(x0, y0, 0);
            GL.Vertex3(x1, y0, 0);
            GL.Vertex3(x1, y1, 0);
            GL.Vertex3(x0, y1, 0);
            GL.End();
        }
    }
    
    /// <summary>
    /// default initialization settings
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        Size = new Vector2(160, 80);
    }

    /// <summary>
    /// Allows to add state as a child to current state
    /// </summary>
    /// <param name="child">state to be linked as a child to this one</param>
    /// <returns></returns>
    public void AddChild(FlowState child)
    {
        if (child.parent != null)
        {
            FlowState p = child.parent;
            p.RemoveChild(child);
            p.UpdateSize();
        }

        child.parent = this;
        children.Add(child);
        child.ParentOffest = child.Position - Position;
        UpdateSize();
    }

    /// <summary>
    /// Remove child from this state
    /// </summary>
    /// <param name="child">child to get removed from children list</param>
    /// <returns></returns>
    public void RemoveChild(FlowState child)
    {
        if (children.Contains(child))
        {
            children.Remove(child);
            child.parent = null;
            UpdateSize();
        }        
    }

    /// <summary>
    /// screen might have dynamic number of exits(and buttons) so it need function which updates its size dynamically as well
    /// </summary>
    /// <returns></returns>
    public void UpdateSize()
    {
        UpdateSize(true);
    }
    
    public void UpdateSize(bool updateParent)
    {
        int count = Mathf.Max(Inputs.Count, Outputs.Count);

        int height = Mathf.Max(count * LineHeight + TitleHeight, 80);
        Size.y = height;
        Size.x = 175 + ((Inputs.Count > 0 && Outputs.Count > 0) ? 75 : 0);

        Vector2 oldPos = Position;
        Vector2 oldSize = Size;

        foreach (FlowState child in children)
        {
            Position.x = Mathf.Min(child.Position.x - m_minimumChildBorder.x, Position.x);
            Position.y = Mathf.Min(child.Position.y - m_minimumChildBorder.y, Position.y);
            Size.x = Mathf.Max(child.Position.x - Position.x + child.Size.x + m_minimumChildBorder.x, Size.x);
            Size.y = Mathf.Max(child.Position.y - Position.y + child.Size.y + m_minimumChildBorder.y, Size.y);
        }

        if (oldPos != Position )
        {
            Vector2 offset = Position - oldPos;

            foreach (FlowState child in children)
            {
                child.ParentOffest -= offset;
                child.UpdateSize();
            }
        }

        //cascade updates to the parents, update offset as well
        if (updateParent && parent != null)
        {
            parent.UpdateSize();
            ParentOffest = Position - parent.Position;
        }
        		
    }

    /// <summary>
    /// Functionality which moves node with content to new position or updates offset to the parent to new value
    /// </summary>
    /// <param name="fromOffset">True would update postion based on offset and parent position. False would save new offest to the parent</param>
    /// <returns></returns>
    public void UpdatePosition(bool fromOffset)
    {        
        if (parent != null)
        {
            if (fromOffset)
            {
                Position = parent.Position + ParentOffest;
            }
            else
            {
                ParentOffest = Position - parent.Position;
            }
        }

        foreach (FlowState child in children)
        {            
            child.UpdatePosition(true);
        }                                
    }

    /// <summary>
    /// Find state among children and children of the children etc.
    /// </summary>
    /// <param name="fs">searched state</param>
    /// <returns>true if state is found</returns>
    public bool InChildSubtree(FlowState fs)
    {
        foreach (FlowState child in children)
        {
            if (child == fs || child.InChildSubtree(fs)) return true;            
        }
        return false;
    }

    /// <summary>
    /// Returns timestamp of the moment this state came to live
    /// </summary>
    /// <returns></returns>
    public float GetStartingTimeStamp()
    {
        return m_enterTimeStamp;
    }

    public virtual void FollowOutput(string name)
    {
        GConnector gc = Outputs.Find(r => r.Name == name);
        if (gc != null)
        {
            ConnectionWithCall(gc, null);
        }
    }

    /// <summary>
    /// function which calls exiting function and if it succeed then continues along connector
    /// </summary>
    /// <param name="gConect">connector to follow</param>
    /// <param name="button">button which triggered event</param>
    /// <returns></returns>
    public void ConnectionWithCall(GConnector gConect, FlowButton button)
    {
        if (gConect.EventFunction != null && gConect.EventFunction != "")
        {            
			UnityEngine.Debug.Log("BFC: function is called " + gConect.EventFunction);
            if (CallStaticFunction(gConect.EventFunction, button))
            {
                parentMachine.FollowConnection(gConect);         
            }
            else
            {
                Debug.Log("Debug: Function forbids further navigation");
            }
        }
        else
        {
            parentMachine.FollowConnection(gConect);
        }
    }

    

    static void FollowNamedLinkInCurrentFlowstate(string name)
    {
        FlowState fs = FlowStateMachine.GetCurrentFlowState();
        GConnector gc = fs.Outputs.Find(r => r.Name == name);

        if (gc != null)
        {
            fs.parentMachine.FollowConnection(gc);
        }
    }

    /// <summary>
    /// refreshes connections lists
    /// </summary>
    /// <returns></returns>
    public override void RebuildConnections()
    {
        base.RebuildConnections();
        UpdateSize();
    }

    /// <summary>
    /// function structure which helps with calling static functions from connectors
    /// </summary>
    /// <param name="functionName">function name to be called</param>
    /// <param name="caller">button which have initialzied process</param>
    /// <returns>true is indication that connection should continue</returns>
    public abstract bool CallStaticFunction(string functionName, FlowButton caller); 

}
