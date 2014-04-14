using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RaceYourself.Models.Blob;

#if UNITY_EDITOR
//using UnityEditor;
#endif

[Serializable]
public abstract class GNodeBase : ISerializable
{
	//[System.NonSerialized]
	//IGData m_graph; // owner
	
	public uint Id; // unique identifier within graph
	public GVector2 Position = new GVector2();
    public GVector2 Size = new GVector2();
	
	// Transient variable to safely evaluate all the nodes in the tree once.
	private int Evaluated; // 0=unknown, 1=ready, -1=false
	//private bool isEvaluating;


    public const int R = 8;
    public const int Border = 3;
    public const int TitleHeight = 24;
    public const int LineHeight = 16;


	// Returns (true) if node's inputs/outputs have been processed.
	public bool IsEvaluated
	{
		get { return Evaluated == 1; }
	}
	
	[SerializeField]
	List<GConnectorBase> m_inputs;
	[SerializeField]
	List<GConnectorBase> m_outputs;
	[SerializeField]
	List<GParameterBase> m_parameters;
	
	//public void OnEnable() { hideFlags = HideFlags.HideAndDontSave; }
	
	public List<GConnectorBase> Inputs
	{
        get
        {
            if (m_inputs == null) m_inputs = new List<GConnectorBase>();
            return m_inputs; 
        }
	}
	public List<GConnectorBase> Outputs
	{
		get 
        {
            if (m_outputs == null) m_outputs = new List<GConnectorBase>();
            return m_outputs; 
        }
	}
	public List<GParameterBase> Parameters
	{
		get 
        {
            if (m_parameters == null) m_parameters = new List<GParameterBase>();
            return m_parameters;            
        }
	}

	public bool HasInputs
	{
		get { return Inputs != null && Inputs.Count > 0; }
	}
	public bool HasOutputs
	{
		get { return Outputs != null && Outputs.Count > 0; }
	}
	public int NumParameters
	{
		get { return (Parameters != null) ? Parameters.Count : 0; }
	}
	
	public GNodeBase()
	{
		Id = 0;

        OnEnable();       
	}
	
	public bool Initialized
	{
		get { return (Size.GetVector2() != Vector2.zero); }
	}

    public GNodeBase(SerializationInfo info, StreamingContext ctxt)
	{              
        this.Id = (uint)info.GetValue("Id", typeof(uint));
        this.Position.x = (float)info.GetValue("PosX", typeof(float));
        this.Position.y = (float)info.GetValue("PosY", typeof(float));
        this.Size.x = (float)info.GetValue("SizeX", typeof(float));
        this.Size.y = (float)info.GetValue("SizeY", typeof(float));

        this.m_inputs = (List<GConnectorBase>)info.GetValue("Inputs", typeof(List<GConnectorBase>));
        this.m_outputs = (List<GConnectorBase>)info.GetValue("Outputs", typeof(List<GConnectorBase>));
        this.m_parameters = (List<GParameterBase>)info.GetValue("Params", typeof(List<GParameterBase>));
	}

    public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("Id", this.Id);
        info.AddValue("PosX", this.Position.x);
        info.AddValue("PosY", this.Position.y);
        info.AddValue("SizeX", this.Size.x);
        info.AddValue("SizeY", this.Size.y);

        info.AddValue("Inputs", this.m_inputs);
        info.AddValue("Outputs", this.m_outputs);
        info.AddValue("Params", this.m_parameters);
   	}


	// ScriptableObject callback
	void OnEnable()
	{
		if (!Initialized)
		{
			Initialize();
		}
	}
	
	protected virtual void Initialize()
	{
		// optional override
	}
	
	public void ClearEvaluated()
	{
		Evaluated = 0;
	}
	public void SetEvaluated(bool flag)
	{
		Evaluated = flag ? +1 : -1;
	}
	
	public virtual string GetDisplayName()
	{
		//System.Reflection.MethodInfo mi = GetType().GetMethod("GetDisplayName");
		//if (mi != null)
		//{
		//	return (string)mi.Invoke(null,null);
		//}
		return "?";
	}

    
	
	public void AddInput(GConnectorBase c)
	{
		if (c != null)
		{
			if (c.Parent == null)
			{
				if (m_inputs == null)
				{
					m_inputs = new List<GConnectorBase>();
				}
				c.IsInput = true;
				m_inputs.Add(c);
				c.Parent =this;
			}
		}
	}

	private void AddOutput(GConnectorBase c)
	{
		if (c != null)
		{
			if (c.Parent == null)
			{
				if (m_outputs == null)
				{
					m_outputs = new List<GConnectorBase>();
				}
				c.IsInput = false;
				m_outputs.Add(c);
				c.Parent =this;
			}
		}
	}
	
	public GConnectorBase NewInput(string name, string type)
	{
        GConnectorBase c = new GConnectorBase();
		c.Name = name;
		c.Type = type;
		AddInput(c);
		return c;
	}
	
	public GConnectorBase NewOutput(string name, string type)
	{
        GConnectorBase c = new GConnectorBase();
		c.Name = name;
		c.Type = type;
		AddOutput(c);
		return c;
	}
	
	public GParameterBase NewParameter(string key, GraphValueType type, string value)
	{
		GParameterBase p = new GParameterBase();
		p.Key = key;
		p.Value = value;
		p.Type = type;
		if (m_parameters == null)
		{
			m_parameters = new List<GParameterBase>();
		}
		m_parameters.Add(p);
		return p;
	}
	
	public Rect GetRect(bool left, int index)
	{
		if (index >= 0)
		{
			if (left)
			{
				return new Rect(Position.x+Border,Position.y+TitleHeight+index*LineHeight,R+1,R+1);
			}
			else // right
			{
				return new Rect(Position.x+Size.x-R-Border,Position.y+TitleHeight+index*LineHeight,R+1,R+1);
			}
		}
		return new Rect(0,0,0,0);
	}
	
	public Rect GetInputRect(GraphDataBase graph, int index)
	{
		if (Inputs != null)
		{
			if (index >= 0 && index < Inputs.Count)
			{
                bool isLeft = (GraphDataBase.Style.RightToLeft == false);
				return GetRect(isLeft,index);
			}
		}
		return new Rect(0,0,0,0);
	}

	public Rect GetOutputRect(GraphDataBase graph, int index)
	{
		if (Outputs != null)
		{
			if (index >= 0 && index < Outputs.Count)
			{                
                bool isLeft = (GraphDataBase.Style.RightToLeft == true);
				return GetRect(isLeft,index);
			}
		}
		return new Rect(0,0,0,0);
	}
	
	public GConnectorBase PickConnector(GraphDataBase graph, Vector2 pos)
	{
		//Debug.Log("PickConnector = "+pos.ToString());
		if (Inputs != null)
		{
			for (int i=0; i<Inputs.Count; ++i)
			{
				Rect r = GetInputRect(graph,i);
				if (r.Contains(pos))
				{
					//Debug.Log("Contains = "+i+" rect="+r.ToString());
					return Inputs[i];
				}
			}
		}
		if (Outputs != null)
		{
			for (int i=0; i<Outputs.Count; ++i)
			{
				Rect r = GetOutputRect(graph,i);
				if (r.Contains(pos))
				{
					//Debug.Log("Contains = "+i+" rect="+r.ToString());
					return Outputs[i];
				}
			}
		}
		return null;
	}
	
	public virtual void OnDraw(Rect r)
	{
		// Override for custom drawing on node panel.
	}

	public GConnectorBase GetOutputConnector(int index)
	{
		if (Outputs != null)
		{
			if (index >= 0 && index < Outputs.Count)
			{
				return Outputs[index];
			}
		}
		return null;
	}
	
    public virtual void RebuildConnections()
    {
    }

    public virtual void RebuildParams()
    {
    }

    public bool IsConnected()
    {
        if (Inputs != null)
        {
            foreach (GConnectorBase c in Inputs)
            {
                if (c.Link != null && c.Link.Count > 0)
                    return true;
            }
        }
        if (Outputs != null)
        {
            foreach (GConnectorBase c in Outputs)
            {
                if (c.Link != null && c.Link.Count > 0)
                    return true;
            }
        }

        return false;
    }

    public virtual bool IsValid()
    {
        return true;
    }

}

