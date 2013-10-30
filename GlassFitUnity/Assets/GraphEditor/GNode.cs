using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class GNode : ScriptableObject
{
	//[System.NonSerialized]
	//IGData m_graph; // owner
	
	public uint Id; // unique identifier within graph
	public Vector2 Position;
	public Vector2 Size;
	
	// Transient variable to safely evaluate all the nodes in the tree once.
	private int Evaluated; // 0=unknown, 1=ready, -1=false
	private bool isEvaluating;

	// Returns (true) if node's inputs/outputs have been processed.
	public bool IsEvaluated
	{
		get { return Evaluated == 1; }
	}
	
	[SerializeField]
	List<GConnector> m_inputs;
	[SerializeField]
	List<GConnector> m_outputs;

	[SerializeField]
	List<GParameter> m_parameters;
	
	//public void OnEnable() { hideFlags = HideFlags.HideAndDontSave; }
	
	public List<GConnector> Inputs
	{
        get
        {
            if (m_inputs == null) m_inputs = new List<GConnector>();
            return m_inputs; 
        }
	}
	public List<GConnector> Outputs
	{
		get 
        {
            if (m_outputs == null) m_outputs = new List<GConnector>();
            return m_outputs; 
        }
	}
	public List<GParameter> Parameters
	{
		get 
        {
            if (m_parameters == null) m_parameters = new List<GParameter>();
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
	
	public GNode()
	{
		Id = 0;
	}
	
	public bool Initialized
	{
		get { return (Size != Vector2.zero); }
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
	
	const int R=8;
	const int Border=3;
	const int TitleHeight = 24;
	const int LineHeight = 16;
	
	public void AddInput(GConnector c)
	{
		if (c != null)
		{
			if (c.Parent == null)
			{
				if (m_inputs == null)
				{
					m_inputs = new List<GConnector>();
				}
				c.IsInput = true;
				m_inputs.Add(c);
				c.Parent =this;
			}
		}
	}

	private void AddOutput(GConnector c)
	{
		if (c != null)
		{
			if (c.Parent == null)
			{
				if (m_outputs == null)
				{
					m_outputs = new List<GConnector>();
				}
				c.IsInput = false;
				m_outputs.Add(c);
				c.Parent =this;
			}
		}
	}
	
	public GConnector NewInput(string name, string type)
	{
		GConnector c= ScriptableObject.CreateInstance<GConnector>();		
		c.Name = name;
		c.Type = type;
		AddInput(c);
		return c;
	}
	
	public GConnector NewOutput(string name, string type)
	{
		GConnector c= ScriptableObject.CreateInstance<GConnector>();		
		c.Name = name;
		c.Type = type;
		AddOutput(c);
		return c;
	}
	
	public GParameter NewParameter(string key, GraphValueType type, string value)
	{
		GParameter p = new GParameter();
		p.Key = key;
		p.Value = value;
		p.Type = type;
		if (m_parameters == null)
		{
			m_parameters = new List<GParameter>();
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
	
	public Rect GetInputRect(GraphData graph, int index)
	{
		if (Inputs != null)
		{
			if (index >= 0 && index < Inputs.Count)
			{
				bool isLeft = (graph.Style.RightToLeft == false);
				return GetRect(isLeft,index);
			}
		}
		return new Rect(0,0,0,0);
	}

	public Rect GetOutputRect(GraphData graph, int index)
	{
		if (Outputs != null)
		{
			if (index >= 0 && index < Outputs.Count)
			{
				bool isLeft = (graph.Style.RightToLeft == true);
				return GetRect(isLeft,index);
			}
		}
		return new Rect(0,0,0,0);
	}
	
	public GConnector PickConnector(GraphData graph, Vector2 pos)
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

	public GConnector GetOutputConnector(int index)
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
            foreach (GConnector c in Inputs)
            {
                if (c.Link != null && c.Link.Count > 0)
                    return true;
            }
        }
        if (Outputs != null)
        {
            foreach (GConnector c in Outputs)
            {
                if (c.Link != null && c.Link.Count > 0)
                    return true;
            }
        }

        return false;
    }

	/*public void SetOutputValue(int index, System.Object result)
	{
		GConnector c = GetOutputConnector(index);
		if (c != null)
		{
			c.Result = result;
			if (c.Link != null)
			{                
				c.Link.Add().Result = result;
			}
		}
	}
	
	public Vector3 InputVector3(int index)
	{
		if (Inputs != null)
		{
			if (index < Inputs.Count)
			{
				if (Inputs[index].Result is Vector3)
				{
					return (Vector3)Inputs[index].Result;
				}
			}
		}
		return Vector3.zero;
	}

	public Vector3 OutputVector3(int index)
	{
		if (Outputs != null)
		{
			if (index < Outputs.Count)
			{
				if (Outputs[index].Result is Vector3)
				{
					return (Vector3)Outputs[index].Result;
				}
			}
		}
		return Vector3.zero;
	}
	
	public float GetParameterFloat(int index)
	{
		try
		{
			if (index >= 0 && index < NumParameters)
			{
				if (Parameters[index] != null)
				{
					float result;
					if (float.TryParse(Parameters[index].Value,out result))
					{
						return result;
					}
				}
			}
		}
		catch
		{
		}
		return 0;
	}*/
	
	// This should only be called by graph window.
	/*public bool TryEvaluate()
	{
		//Debug.Log("TryEvaluate "+GetDisplayName()+" init="+Initialized+" eval="+Evaluated+" isEval="+isEvaluating);
		if (Initialized)
		{
			if (Evaluated == 0) // Unknown
			{
				if (isEvaluating)
				{
					Debug.Log("isEvaluating already?");
					return false; // recursive links not allowed
				}
				isEvaluating = true;
				bool result = InnerEvaluate(); // evaluate input connections
				if (result)
				{
					result = Evaluate(); // process inputs to outputs
				}
				SetEvaluated(result);
				isEvaluating = false;
				return result;
			}
			return (Evaluated == 1);
		}
		return false;
	}
	
	virtual public bool Evaluate()
	{
		return false; // Override this!
	}
	
	private bool InnerEvaluate()
	{
		if (Inputs != null) // constants have no inputs
		{
			// Todo: trace inputs back up the tree once?
			foreach (GConnector c in Inputs)
			{
				if (c == null)
				{
					Debug.LogWarning("Inputs contains null reference?");
					Debug.LogWarning(c);
					return false;
				}
				if (c.Link == null)
				{
					return false;
				}
				if (c.Link.Parent == null)
				{
					Debug.LogWarning("Parent is null.");
					return false;
				}
				if (c.Link.Parent == this)
				{
					Debug.LogWarning("Recrusive!");
					return false;
				}
				//Debug.Log("Skipped = TryEvaluate");
				if (c.Link.Parent.TryEvaluate() == false)
				{
					return false;
				}
			}
		}
		return true;
	}*/
}

