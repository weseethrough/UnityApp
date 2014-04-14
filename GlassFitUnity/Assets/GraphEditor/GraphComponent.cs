using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Sqo;

//[ExecuteInEditMode]
public class GraphComponent : GraphComponentBase
{
    public UIAtlas m_defaultHexagonalAtlas;    
   
    static private GraphComponent instance;

    static private string[] avaliableFlows = null;

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
    public override void MakeAwake()
    {
        if (!initialize)
        {
            initialize = true;
            instance = this;


            GraphDataBase.Style = new GStyle();

            DataStore.LoadStorage(DataStore.BlobNames.flow);

            nextStartNavigateTo = "MasterFlow";

            LoadFlow(nextStartNavigateTo);
            return;
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
			bool toGame = toGameInt > 0;
			string flowName = "MainFlow";
			if(toGame)
			{
				DataVault.Set("custom_redirection_point", "GameIntroExit");
				flowName = "GameplayFlow";
			}
			else
			{
	            DataVault.Set("custom_redirection_point", "MenuPoint");
	            flowName = "MainFlow";//"MobileUX";
			}
            SetSelectedFlowByName(flowName);
#endif
            
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
        //TODO: we dont want to use indexes now, I'm not sure how well they work for database yet
        return 0;//


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
        /*
        //DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

        if (flowDictionary.Length() <= selectedFlow)
        {
            selectedFlow = 0;
        }

        GraphData data = flowDictionary.Get(selectedFlow) as GraphData;
        if (data == null)
        {
            data = new GraphData();
        }
        data.Style = new GStyle();//m_graph.Style;
        
        m_graph = data;*/
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flowName"></param>
    /// <returns></returns>
    public void SetSelectedFlowByName(string flowName)
    {
        LoadFlow(flowName);
        return;

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
        return;

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
        return false;

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
    public override bool GoToFlowStage2()
    {
        return false;

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

    //New functionality

    public string[] GetFlowArray()
    {
        if (avaliableFlows != null) return avaliableFlows;

        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStaticInstance();
        ISqoQuery<GraphDataBase> q = db.Query<GraphDataBase>();

        int count = q.Count();
        string[] ret;
        if (count > 0)
        {
            ret = new string[count];
            int i =0;

            foreach(GraphDataBase d in q)
            {
                ret[i] = d.Name;
                i++;
            }
        }
        else
        {
            ret = new string[1];
            ret[0] = "NONE";        
        }

        avaliableFlows = ret;

        return ret;
    }

    public void SaveFlow()
    {
        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStaticInstance();
        
        if (m_graph != null)
        {
            GStyle s = GraphDataBase.Style;
            GraphDataBase.Style = null;

            db.StoreObject(m_graph);

            GraphDataBase.Style = s;
            avaliableFlows = null;
        }        
    }

    public void SaveFlow(GraphDataBase data)
    {
        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStaticInstance();

        if (data != null)
        {
            GStyle s = GraphDataBase.Style;
            GraphDataBase.Style = null;

            db.StoreObject(data);

            GraphDataBase.Style = s;
            avaliableFlows = null;
        }
    }

    public void LoadFlow(string name)
    {
        Siaqodb db = SiaqodbUtils.DatabaseFactory.GetStaticInstance();
        ISqoQuery<GraphDataBase> q = db.Query<GraphDataBase>();

        GraphDataBase data = q.Where(d => d.Name == name).FirstOrDefault();

        if (data == null)
        {
            m_graph = new GraphDataBase();
            m_graph.Name = name;
        }
        else
        {
            m_graph = data;
        }
        GraphDataBase.Style = new GStyle();
    }
}
