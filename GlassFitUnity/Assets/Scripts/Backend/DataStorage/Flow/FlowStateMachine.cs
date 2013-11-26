using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// component which is flow manager embedded in prefab. Manages state progression, flowstate stack and many more important flow behaviors and data 
/// </summary>
public class FlowStateMachine : MonoBehaviour 
{
    private List<FlowState> activeFlow;
    private List<FlowState> navigationHistory;
    private FlowState targetState;
    private GConnector targetStateConnector;

    /// <summary>
    /// default unity initialization call, used to stop this structure from destruction when navigation between scenes
    /// </summary>
    /// <returns></returns>
    void Awake()
    {
		DontDestroyOnLoad(transform.gameObject);
    }

    /// <summary>
    /// default unity initialization call after all Awake are called. Used to find starting node and format some variables.
    /// </summary>
    /// <returns></returns>
    void Start()
    {                     
        activeFlow = new List<FlowState>();
        targetState = null;
        GraphComponent gc = GetComponent<GraphComponent>();
        if (gc != null)
        {                        
            foreach(GNode node in gc.Data.Nodes)
            {
                if (node is Start)
                {
                    targetState = node as Start;
                    break;
                }
            }
        }
    }
	
	/// <summary>
	/// static function allowing to take current flow state if one exists. Might be useful for some arbitrary calls in static locations which needs to know what panel is currently on or force navigation in/out
	/// </summary>
	/// <returns>returns current active flow state</returns>
	static public FlowState GetCurentFlowState()
    {
        FlowStateMachine fsm = GameObject.FindObjectOfType(typeof(FlowStateMachine)) as FlowStateMachine;
        if (fsm == null) return null;

        if (fsm.activeFlow == null || fsm.activeFlow.Count == 0) return null;

        return fsm.activeFlow[fsm.activeFlow.Count - 1];
    }
	
    /// <summary>
    /// default unity function called once every frame. used by internal state machine to progress between states
    /// </summary>
    /// <returns></returns>
    void Update()
    {
        ProgressStateChanges();

        for (int i = 0; i < activeFlow.Count; i++)
        {
            bool specialProgress = UpdateState(activeFlow[i]);

            if (activeFlow[i].state == FlowState.State.Dead)
            {
                activeFlow.RemoveAt(i);
                i--;
                ProgressStateChanges();
            }
            else if (specialProgress)
            {
                i--;
                ProgressStateChanges();
            }
        }
    }

    /// <summary>
    /// function to progress update on current state or forward update call if required
    /// </summary>
    /// <param name="fs"></param>
    /// <returns>if true is returned another update would eb called, for example if we are leaving from stack of the states we need to close one after another but all in same frame if none of them stuck for their own exitUpdate function</returns>
    public bool UpdateState(FlowState fs)
    {
        bool requiresAnotherPass = false;

        switch (fs.state)
        {
            case FlowState.State.Entering:
                if (fs.EnterUpdate())
                {
                    fs.Entered();
                    requiresAnotherPass = true;
                }
                break;
            case FlowState.State.Idle:
                fs.StateUpdate();
                break;
            case FlowState.State.Exiting:
                if (fs.ExitUpdate())
                {
                    fs.Exited();
                    requiresAnotherPass = true;
                }
                break;
        }

        return requiresAnotherPass;
    }

    /// <summary>
    /// function which allows to navigate along specified connection
    /// </summary>
    /// <param name="connection">connection you want this flow to follow</param>
    /// <returns>returns true if follow connection is possible (connection is valid)</returns>
    public bool FollowConnection(GConnector connection)
    {
        if (connection != null && 
            connection.Link != null && 
            connection.Link.Count > 0 &&
            connection.Link[0].Parent != null)
        {
            if (navigationHistory == null)
            {
                navigationHistory = new List<FlowState>();
            }
            navigationHistory.Add(activeFlow[activeFlow.Count-1]);
            targetState = connection.Link[0].Parent as FlowState;
            targetStateConnector = connection.Link[0];
            return true;
        }
        return false;
    }

    /// <summary>
    /// function which allows for easy navigation back in the history of the previously visited panels
    /// </summary>
    /// <returns>returns false if navigation history was invalid/empty</returns>
    public bool FollowBack()
    {
        if (navigationHistory != null && navigationHistory.Count > 0)
        {
            FlowState fs = navigationHistory[navigationHistory.Count - 1];
            navigationHistory.RemoveAt(navigationHistory.Count - 1);

            targetState = fs;
            return true;
        }
        return false;
    }

    /// <summary>
    /// clears previous history and makes further NavigationBack invalid until new history is made. This for example blocks user from navigation back to the game from summary screen and other back autonavigations avaliable in UI
    /// </summary>
    /// <returns></returns>
    public void ForbidBack()
    {
        navigationHistory = new List<FlowState>();
    }

    /// <summary>
    /// Used to navigate up and down on state hierarchy
    /// </summary>
    /// <param name="fs">state destination</param>
    /// <returns>returns true when operation is finished. until then all calls return false</returns>
    private bool ProgressStateChanges()
    {
        //target state cant be null, machine must be ready and we cant be exactly in target state
        if (targetState != null && 
            IsReady())
        {
            if (activeFlow.Count > 0 && activeFlow[activeFlow.Count - 1] == targetState)
            {
                if (targetStateConnector != null && targetStateConnector.EventFunction != null && targetStateConnector.EventFunction != "")
                {
                    if (targetState is Panel)
                    {
                        (targetState as Panel).CallStaticFunction(targetStateConnector.EventFunction, null);
                    }
                }
                targetStateConnector = null;
                targetState = null;
                return true;
            }

            FlowState nextStep = targetState;
            FlowState nextStepChild = null;
            while (nextStep != null)
            {
                if (activeFlow.Contains(nextStep))
                {
                    break;
                }
                nextStepChild = nextStep;
                nextStep = nextStep.parent;
            }
            
            //we have any states entered but last one is not within our list of expectations
            if (activeFlow.Count > 0)
            {
                if (activeFlow[activeFlow.Count - 1] != nextStep)
                {
                    activeFlow[activeFlow.Count - 1].ExitStart();
                    return false;
                }
            }
            
            
            //our next step is applied, we will navigate to the child
            if (nextStepChild != null)
            {
                activeFlow.Add(nextStepChild);
                nextStepChild.parentMachine = this;
                nextStepChild.EnterStart();
                return false;
            }
            else
            {
                //shouldn't ever reach this point
                targetState = null;
                return true;
            }                        
        }
        return false;
    }

    /// <summary>
    /// Checks if latest state in stack is idle making whole stack structure idle and so state machine
    /// </summary>
    /// <returns>returns true if state machine is ready to do new navigations etc</returns>
    public bool IsReady()
    {
        if (activeFlow.Count > 0)
        {
            if (activeFlow[activeFlow.Count - 1].state == FlowState.State.Idle)
            {
                return true;
            }
            return false;
        }
        return true;
    }
    
}
