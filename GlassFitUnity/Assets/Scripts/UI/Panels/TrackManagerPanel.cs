using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System;

using RaceYourself.Models;

[Serializable]
public class TrackManagerPanel : MultiPanel {

	List<Track> trackList;
	GestureHelper.OnTap tapHandler = null;
	GestureHelper.OnBack backHandler = null;

	/// <summary>
	/// default constructor
	/// </summary>
	/// <returns></returns>
	public TrackManagerPanel() : base() { }
	
	/// <summary>
	/// deserialziation constructor
	/// </summary>
	/// <param name="info">seirilization info conataining class data</param>
	/// <param name="ctxt">serialization context </param>
	/// <returns></returns>
	public TrackManagerPanel(SerializationInfo info, StreamingContext ctxt)
		: base(info, ctxt)
	{
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
			return "TrackManagerPanel: " + gName.Value;
		}
		return "TrackManagerPanel: UnInitialized";
	}

	public override void EnterStart ()
	{
		trackList = (List<Track>)DataVault.Get("track_list");

		for(int i=1; i<trackList.Count; i++) {
			CloneNode(children[0], this, null, i);
		}

		tapHandler = new GestureHelper.OnTap(() => {
			SetTrack();
		});
		GestureHelper.onTap += tapHandler;

		backHandler = new GestureHelper.OnBack(() => {
			OnBack();
		});
		GestureHelper.onBack += backHandler;

		base.EnterStart ();	

		for(int i=0; i<managedChildren.Count; i++) {
			if(managedChildren[i] is MultiPanelChild ) {
				SetTrackInfo(managedChildren[i] as MultiPanelChild, i);
			}
		}		

		DataVault.Set("numberOfPages", managedChildren.Count);	
	}

	public override void Exited ()
	{
		base.Exited ();

		GestureHelper.onTap -= tapHandler;
		GestureHelper.onBack -= backHandler;
	}
	
	public void SetTrack() {
		int index = (int)DataVault.Get("currentPageIndex");
		DataVault.Set("current_track", trackList[index]);
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
		if(gConnect != null)
		{
			(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
			fs.parentMachine.FollowConnection(gConnect);
		} else 
		{
			UnityEngine.Debug.Log("TrackSelect: Connection not found");
		}
	}

	private void CloneNode(FlowState source, FlowState parent, Dictionary<uint, uint> oldToNewID, int count)
	{
		FlowState node = Activator.CreateInstance(source.GetType()) as FlowState;

		if (oldToNewID == null)
		{
			oldToNewID = new Dictionary<uint, uint>();
		}

		GraphComponent Graph = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;

		Graph.Data.Add(node);
		oldToNewID.Add(source.Id, node.Id);
		
		if (parent == null)
		{
			//node.Position = GetNewPosition(source.Size);
			node.Size = source.Size;
		}
		else
		{
			node.Position = parent.Position + (source.Position - source.parent.Position);
			node.Size = source.Size;
			parent.AddChild(node);
		}
		
		node.Parameters.Clear();
		foreach(GParameter param in source.Parameters)
		{            
			node.NewParameter(param.Key, param.Type, param.Value);
		}
		
		node.Inputs.Clear();
		foreach (GConnector entry in source.Inputs)
		{
			node.NewInput(entry.Name, entry.Type);
		}
		
		node.Outputs.Clear();
		foreach (GConnector exit in source.Outputs)
		{
			node.NewOutput(exit.Name, exit.Type);
		}
		
		foreach (FlowState child in source.children)
		{
			CloneNode(child, node, oldToNewID, count);
		}

		MultiPanelChild childNode = node as MultiPanelChild;

		//childNode.ManagedEnter();
		//childNode.parentMachine = parentMachine;

		//SetTrackInfo(childNode, count);

		source.parent.AddChild(childNode);

		//managedChildren.Add(childNode);
	}

	void SetTrackInfo(MultiPanelChild childNode, int count) {
		GameObject map = GameObjectUtils.SearchTreeByName(childNode.physicalWidgetRoot, "MapTexture");
		if(map) {
			map.GetComponent<TrackSelect>().SetTrack(trackList[count]);
		} else {
			UnityEngine.Debug.LogError("TrackManagerPanel: error finding map");
		}
	}
}
