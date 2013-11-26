using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

    // do not let state switch outside of order. It might cause some states get unplugged from order list and result with unpredictable results    
    private State m_state;   
    private FlowState m_parent;    
    private List<FlowState> m_children;

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
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public FlowState(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
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


    //collection of virtual functions for different states of the panel flow
    virtual public void EnterStart() { m_state = State.Entering; Debug.Log("Enter state: " + this.ToString()); }
    virtual public bool EnterUpdate() { return true; }
    virtual public void Entered() { m_state = State.Idle; Debug.Log("Entered state: " + this.ToString()); }

    virtual public void StateUpdate() {  }

    virtual public void ExitStart() { m_state = State.Exiting; Debug.Log("Exit state: " + this.ToString()); }
    virtual public bool ExitUpdate() { return true; }
    virtual public void Exited() { m_state = State.Dead; Debug.Log("Exited state: " + this.ToString()); }   

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

}
