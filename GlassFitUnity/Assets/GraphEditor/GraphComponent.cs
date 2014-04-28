using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    [System.NonSerialized]
	public GraphData m_graph;
    public UIAtlas m_defaultHexagonalAtlas;
    private int selectedFlow = 0;
    private bool initialize = false;

    private string nextStartNavigateTo = ""; 
   
    static private GraphComponent instance;

    public GraphData Data
    {
        get
        {
            MakeAwake();
            return m_graph;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    void Start()
    {       
        MakeAwake();        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    void MakeAwake()
    {
        if (!initialize)
        {
            initialize = true;
            DataStore.LoadStorage(DataStore.BlobNames.flow);            
            
            //test area. Need to be removed after tests are done
#if !UNITY_EDITOR
            //below is the example how to initialize game with specific flow
            if (!Platform.Instance.OnGlass() )
            {
                string flowName = "MobileUX";
            if (Platform.Instance.NetworkMessageListener.GetIntent().Length > 0)
                {
            flowName = Platform.Instance.NetworkMessageListener.GetIntent();
                }

                //make forwarding state go to challenge screen instead of main menu
                //DataVault.Set("custom_redirection_point", "Challenge");
                DataVault.Set("custom_redirection_point", "MenuPoint");
                SetSelectedFlowByName(flowName);
            }
            else
            {
                SetSelectedFlowIndex(selectedFlow);
            }
#else
			//to go in-game menu, or else to main menu point
			int toGameInt = PlayerPrefs.GetInt("toGame");
			bool toGame = (toGameInt == 1);
			bool toMobile = (toGameInt == 2);
			string flowName = "MainFlow";
			if(toGame)
			{
				DataVault.Set("custom_redirection_point", "GameIntroExit");
				flowName = "GameplayFlow";
			}
			else if(toMobile)
			{
				DataVault.Set("custom_redirection_point", "Exit");
				flowName = "MobileUX";
			}
			else
			{
                DataVault.Set("custom_redirection_point", "Exit"); // Exit to see first run flow; MenuPoint to jump to main menu
	            flowName = "MainFlow";//"MobileUX";
			}
            SetSelectedFlowByName(flowName);
#endif
            instance = this;
        }
    }	
	
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public GraphComponent()
	{
		m_graph = new GraphData();
	}    

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetSelectedFlowIndex()
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

        if (flowDictionary.Length() <= selectedFlow)
        {
            selectedFlow = 0;
        }

        return selectedFlow;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetSelectedFlowName()
    {        
        //DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        string name;
        System.Runtime.Serialization.ISerializable data;
        flowDictionary.Get(selectedFlow, out name, out data);

        return name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void SetSelectedFlowIndex()
    {
        SetSelectedFlowIndex(selectedFlow);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flowIndex"></param>
    /// <returns></returns>
    public void SetSelectedFlowIndex(int flowIndex)
    {
        //we do not check if index is the same because data could change, flow cold have removed or be modified.

        selectedFlow = flowIndex;

        //DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

        if (flowDictionary.Length() <= selectedFlow)
        {
            selectedFlow = 0;
        }

        GraphData data = flowDictionary.Get(selectedFlow) as GraphData;
        data.Style = m_graph.Style;
        m_graph = data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flowName"></param>
    /// <returns></returns>
    public void SetSelectedFlowByName(string flowName)
    {
       // DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int index = flowDictionary.GetIndex(flowName);
        if (index >= 0)
        {
            selectedFlow = index;
        }
        SetSelectedFlowIndex();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void SetSelectedFlowByLast()
    {
       // DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int index = flowDictionary.Length();
        if (index > 0)
        {
            selectedFlow = index -1;
        }
        SetSelectedFlowIndex();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GoToFlow(string name)
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int nextIndex = flowDictionary.GetIndex(name);
        if (nextIndex == -1 || selectedFlow == nextIndex) return false;
        
        //if flow exists it would be initialized, otherwise initialized would be flow currently defaulted.
        //Make sure statemachine is ready for this type of move
        nextStartNavigateTo = name;

        FlowStateMachine fsm = GetComponent<FlowStateMachine>();
        if (fsm != null)
        {
            fsm.GoToStart();
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool GoToFlowStage2()
    {        
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int index = flowDictionary.GetIndex(nextStartNavigateTo);

        if (index >= 0 && index != selectedFlow)
        {
            SetSelectedFlowByName(nextStartNavigateTo);

            FlowStateMachine fsm = GetComponent<FlowStateMachine>();
            if (fsm != null)
            {
                fsm.GoToStart();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    static public GraphComponent GetInstance()
    {
        if (instance == null)
        {
            return GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        }
        return instance;
    }
}
