using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// component which is flow manager embedded in prefab. Manages state progression, flowstate stack and many more important flow behaviors and data 
/// </summary>
public class FlowStateMachine : MonoBehaviour 
{
    private List<FlowState> activeFlow;
    private List<FlowState> navigationHistory;
    private FlowState targetState;
    private GConnector targetStateConnector;

    private bool grabAnalyticsInitialized = false;

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

    public void RemoveFromHistory(FlowState flowState)
    {
        int idx = navigationHistory.IndexOf(flowState);
        if (idx >= 0)
            navigationHistory.RemoveAt(idx);
    }
	
    /// <summary>
    /// static function which allows to restart the state machine through any of the Start node's outputs
    /// </summary>
    /// <param name="output">name of output you want this flow to follow</param>
    /// <returns>returns true if follow connection is possible (connection is valid)</returns>
	static public bool Restart(string output)
	{
        FlowStateMachine fsm = GameObject.FindObjectOfType(typeof(FlowStateMachine)) as FlowStateMachine;
        if (fsm == null) return false;
		
        GraphComponent gc = fsm.GetComponent<GraphComponent>();
        if (gc != null)
        {                        
            foreach(GNode node in gc.Data.Nodes)
            {
                if (node is Start)
                {
					GConnector gConnect = node.Outputs.Find(r => r.Name == output);
					if (gConnect != null) return fsm.FollowConnection(gConnect);
                }
            }
        }
		return false;
	}
	
	/// <summary>
	/// static function allowing to take current flow state if one exists. Might be useful for some arbitrary calls in static locations which needs to know what panel is currently on or force navigation in/out
	/// </summary>
	/// <returns>returns current active flow state</returns>
	static public FlowState GetCurrentFlowState()
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
		if(activeFlow != null)
		{
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

            // log analytics
            Hashtable gameDetails = new Hashtable();
            object type = DataVault.Get("type");
            object log = DataVault.Get("warning_log");
            gameDetails.Add("event_type", "Flow state changed");
            gameDetails.Add("flow_state", activeFlow[activeFlow.Count - 1].GetDisplayName());
            gameDetails.Add("game_type", (string)type);
            gameDetails.Add("time_since_launch", (int)(Time.realtimeSinceStartup * 1000));
            gameDetails.Add("time_in_state", (int)( (Time.realtimeSinceStartup - activeFlow[activeFlow.Count - 1].GetStartingTimeStamp()) * 1000 ) );
            gameDetails.Add("custom_log", (string)log );
            Platform.Instance.LogAnalytics(JsonConvert.SerializeObject(gameDetails));

            if (navigationHistory == null)
            {
                navigationHistory = new List<FlowState>();
            }

            navigationHistory.Add(activeFlow[activeFlow.Count-1]);
            //DataVault.Set("has_history", true);
            targetState = connection.Link[0].Parent as FlowState;
            targetStateConnector = connection.Link[0];            
            return true;
        }
        return false;
    }

    /// <summary>
    /// Function which finds start node and navigates to it using generic GoToState function
    /// </summary>
    /// <returns></returns>
    public void GoToStart()
    {        
        GraphComponent gc = GetComponent<GraphComponent>();
        if (gc != null)
        {
            foreach (GNode node in gc.Data.Nodes)
            {
                if (node is Start)
                {                    
                    GoToState(node as Start);
                    break;
                }
            }
        }        
    }

    /// <summary>
    /// Function which goes to state with specified ID. It might 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public void GoToStateByID(uint id)
    {
        GraphComponent gc = GetComponent<GraphComponent>();
        if (gc != null)
        {
            foreach (GNode node in gc.Data.Nodes)
            {
                if (node.Id == id)
                {
                    GoToState(node as FlowState);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 'Teleports' state machine to an arbitrary state, without a transition being defined in the flow.
    /// </summary>
    /// <param name="state">state instance which we want navigate to</param>
    /// <returns>returns always true</returns>
    public bool GoToState(FlowState state)
    {
        
//        if (!grabAnalyticsInitialized)
//        {
//            //use this to enable debug output
//            GrabBridge.ToggleLog(true);
//
//            GrabBridge.Start("pxeqexpldwfcwdp:faef0c81e352b38894b8df87:R7mg9jl2t4UOOWGxHTDh2Ys3KRHH/NOs0QAy9osBUEE=");
//
//            string userid = "tester";
//            GrabBridge.FirstLogin(userid);
//
//            grabAnalyticsInitialized = true;
//        }
//
//
//        JSONObject gameDetails = new JSONObject();
//        object type = DataVault.Get("type");
//        object log = DataVault.Get("warning_log");
//        DataVault.Set("warning_log", "");
//
//        gameDetails.AddField("Flow state", activeFlow[activeFlow.Count - 1].GetDisplayName());
//        gameDetails.AddField("Game type", (string)type);
//        gameDetails.AddField("Time since launch", (int)(Time.realtimeSinceStartup * 1000));
//        gameDetails.AddField("State live", (int)((Time.realtimeSinceStartup - activeFlow[activeFlow.Count - 1].GetStartingTimeStamp()) * 1000));
//        gameDetails.AddField("Custom Log", (string)log);
//
//        GrabBridge.CustomEvent("Flow state changed", gameDetails);
//
//        // Our own internal logging for analytics
//        gameDetails.AddField("Event type", "Flow state changed");
//        Platform.Instance.LogAnalytics(gameDetails);

        ForbidBack();
        targetState = state;
        targetStateConnector = null;
        return true;        
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
            if (navigationHistory.Count == 0)
            {
                //DataVault.Set("has_history", false);
            }
			SoundManager.PlaySound(SoundManager.Sounds.HidePopup);
            targetState = fs;
            Debug.Log("'Back' to flow state: " + fs.GetDisplayName());
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
        // update data vault - set hasHistory
        //DataVault.Set("has_history", false);
    }

    public bool IsBackAllowed()
    {
        return navigationHistory.Count > 0;
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
                // Used to notify other components that we're at the end of a state transition.
                DataVault.Set ("transition_ended", true);
                targetStateConnector = null;
                targetState = null;
                return true;
            }
            DataVault.Set ("transition_ended", false);

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

                if (nextStepChild == targetState &&                    
                    targetStateConnector != null && 
                    targetStateConnector.EventFunction != null && 
                    targetStateConnector.EventFunction != "")
                {
                    if (targetState is FlowState)
                    {
                        (targetState as FlowState).CallStaticFunction(targetStateConnector.EventFunction, null);
                    }
                }

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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void LoadGameSaves()
    {
        DataStore.LoadStorage(DataStore.BlobNames.saves);        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void SaveCurrentGameState()
    {
        uint id;

        if (targetState == null)
        {
            if (activeFlow == null || activeFlow.Count == 0) return;

            FlowState fs = activeFlow[activeFlow.Count - 1];
            id = fs.Id;
        }
        else
        {
            id = targetState.Id;
        }

        GameStateRestorable gsRestorable = new GameStateRestorable();
        gsRestorable.StoreCurrent(this);
                                     
        Storage storage = DataStore.GetStorage(DataStore.BlobNames.saves);
        storage.dictionary.Set("Save1", gsRestorable);

        DataStore.SaveStorage(DataStore.BlobNames.saves);
    }

    /// <summary>
    /// Retursn current target state, which is not null only during transition to new state
    /// </summary>
    /// <returns></returns>
    public FlowState GetCurrentTargetState()
    {
        return targetState;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void RestoreFromSave()
    {
        Storage storage = DataStore.GetStorage(DataStore.BlobNames.saves);
        GameStateRestorable gsRestorable = storage.dictionary.Get("Save1") as GameStateRestorable;
        if (gsRestorable != null)
        {
            gsRestorable.RestoreCurrent();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public FlowState GetCurrentState()
    {
        
        if (activeFlow == null || activeFlow.Count == 0) return null;

        return activeFlow[activeFlow.Count - 1];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
	void OnApplicationQuit() 
    {
        if (SaveRestorableArea.allowGameStateAutoSave)
        {
            Debug.Log("-------SAVE ON EXIT by EXIT");
            SaveCurrentGameState();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    void OnApplicationPause() 
    {
        if (SaveRestorableArea.allowGameStateAutoSave)
        {
            Debug.Log("-------SAVE ON EXIT by PAUSE");
            SaveCurrentGameState();
        }
    }    
}
