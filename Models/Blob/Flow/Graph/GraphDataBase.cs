using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RaceYourself.Models.Blob;

// The actual Graph data class that contains the list of nodes and connections.
// Not a ScriptableObject or it would not be expanded in the Inspector window
[Serializable]
public class GraphDataBase : ISerializable
{
    public enum ConnectorDirection
    {
        In,
        Out,
        Both
    }

	static public GStyle Style = new GStyle();    
	
	[HideInInspector]
	public List<GNodeBase> Nodes;
	
	// List of source (output) connectors for the graph.
	[HideInInspector]
	public List<GConnectorBase> Connections;

	[HideInInspector]
	public uint IdNext; // factory for unique node identifiers

    public string Name = "";
	
	public bool Empty
	{
		get { return Nodes.Count == 0; }
	}
	
	public GraphDataBase()
	{
		IdNext = 1;		
		Nodes = new List<GNodeBase>();
		Connections = new List<GConnectorBase>();
	}
	
	public void Add(GNodeBase node)
	{
		if (node != null)
		{
			//Debug.Log("Add");
			node.Id = IdNext++;
			Nodes.Add(node);
		}
	}
	
	public bool IsConnected(GConnectorBase a, GConnectorBase b)
	{
		if (a != null && b != null)
		{
			if (a.Parent != null && b.Parent != null)
			{
				// Consistent order for connections
				if (a.Parent.Id > b.Parent.Id)
				{
					GConnectorBase swap = a;
					a = b;
					b = swap;
				}
				
				foreach (GConnectorBase c in Connections)
				{
					if (c == a && c.Link.Contains(b))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool Connect(GConnectorBase a, GConnectorBase b)
	{
		if (a != null && b != null)
		{
			if (a.Parent != null && b.Parent != null)
			{				
				// Do not allow (recursive) connections to self!
				if (a.Parent != b.Parent)
				{
					GConnectorBase src,dst;
					src = a.IsInput ? b : a;
					dst = a.IsInput ? a : b;
                    if (!src.Link.Contains(dst)) src.Link.Add(dst);
                    if (!dst.Link.Contains(src)) dst.Link.Add(src);                    
					Connections.Add(src);
				
					return true;
				}				
			}
		}
		Debug.LogWarning("Connect failed.");
		return false;
	}
	
	public GNodeBase PickNode(Vector2 pos)
	{
		foreach(GNodeBase node in Nodes)
		{
			float x0 = node.Position.x;
			float y0 = node.Position.y;
			float x1 = x0 + node.Size.x;
			float y1 = y0 + node.Size.y;
			if (pos.x >= x0 && pos.y >= y0 && pos.x <= x1 && pos.y <= y1)
			{
				return node;
			}
		}
		return null;
	}

    /// <summary>
    /// Allows to find node which is on deepest position under mouse cursor. We select least parental state possible
    /// </summary>
    /// <param name="pos">mouse position</param>
    /// <returns>deepest child state under mouse pointer</returns>
    public FlowStateBase PickDeepStateNode(Vector2 pos, GNodeBase ignore)
    {
        FlowStateBase fs = null;
        int depth = -1;

        foreach (FlowStateBase node in Nodes)
        {
            if (ignore == node) continue;

            float x0 = node.Position.x;
            float y0 = node.Position.y;
            float x1 = x0 + node.Size.x;
            float y1 = y0 + node.Size.y;
            if (pos.x >= x0 && pos.y >= y0 && pos.x <= x1 && pos.y <= y1)
            {
                int nodeDepth = 0;
                FlowStateBase walker = node;
                while (walker.parent != null)
                {
                    walker = walker.parent;
                    nodeDepth ++;
                }
                if (nodeDepth > depth)
                {
                    fs = node;
                    depth = nodeDepth;
                }                
            }
        }
        return fs;
    }

    public bool Disconnect(GConnectorBase source)
    {
        if (source != null && source.Link != null)
        {
            //disconnect self from others
            foreach (GConnectorBase c in source.Link)
            {
                DisconnectSingleDirection(c, source);
                if (Connections.Contains(c))
                {
                    Connections.Remove(c);
                }
            }

            source.Link.Clear();
            return true;
        }
        return false;
    }

    public bool Disconnect(GConnectorBase source, GConnectorBase target)
    {
        return DisconnectSingleDirection(source, target) && DisconnectSingleDirection(target, source);
    }

    private bool DisconnectSingleDirection(GConnectorBase source, GConnectorBase target)
	{
		if (source != null && source.Link != null)
		{
            if (source.Link.Contains(target))
            {
                source.Link.Remove(target);
                if (Connections.Contains(source))
                {
                    Connections.Remove(source);
                }
            }            
            return true;
		}
		return false;
	}

    public void Disconnect(GNodeBase node)
    {
        Disconnect(node, ConnectorDirection.Both);
    }

	public void Disconnect(GNodeBase node, ConnectorDirection direction)
	{
		if (node != null)
		{
			if ((direction == ConnectorDirection.In ||
                direction == ConnectorDirection.Both)
                && node.Inputs != null)
			{
				foreach (GConnectorBase it in node.Inputs)
				{
					Disconnect(it);
				}
			}
            if ((direction == ConnectorDirection.Out ||
                direction == ConnectorDirection.Both)
                && node.Outputs != null)
			{
				foreach (GConnectorBase it in node.Outputs)
				{
					Disconnect(it);
				}
			}
		}
	}
	
	public bool Remove(GNodeBase node)
	{
		if (node != null)
		{
			Disconnect(node);
            FlowStateBase fs = node as FlowStateBase;
            if (fs != null && fs.parent != null)
            {
                fs.parent.RemoveChild(fs);
                fs.parent = null;
            }
			bool ok = this.Nodes.Remove(node);
			if (!ok) Debug.LogWarning("Remove failed?");
			node.Id = 0; // clear the identifier
			return ok;
		}
		return false;
	}

    public void ClearGraphData()
    {
        IdNext = 1;        
        Nodes = new List<GNodeBase>();
        Connections = new List<GConnectorBase>();
    }

    public GraphDataBase(SerializationInfo info, StreamingContext ctxt)
	{        
        this.Nodes          = (List<GNodeBase>)info.GetValue("Nodes", typeof(List<GNodeBase>));
        this.Connections    = (List<GConnectorBase>)info.GetValue("Connections", typeof(List<GConnectorBase>));
        this.IdNext         = (uint)info.GetValue("IdNext", typeof(uint));
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{     
        info.AddValue("Nodes", this.Nodes);
        info.AddValue("Connections", this.Connections);
        info.AddValue("IdNext", this.IdNext);
   	}
}
