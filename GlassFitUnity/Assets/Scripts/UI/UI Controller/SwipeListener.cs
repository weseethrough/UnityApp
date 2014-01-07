using UnityEngine;
using System.Collections;

public class SwipeListener : MonoBehaviour {
	
	GestureHelper.OnSwipeLeft leftHandler;
	GestureHelper.OnSwipeRight rightHandler;
	
	// Use this for initialization
	public virtual void Start () {
		leftHandler = new GestureHelper.OnSwipeLeft( () => {
			handleLeft();
		});
		GestureHelper.swipeLeft += leftHandler;
		
		rightHandler = new GestureHelper.OnSwipeRight( () => {
			handleRight();
		});
		GestureHelper.swipeRight += rightHandler;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	protected virtual void handleLeft()
	{
		FollowFlowLinkNamed("Back");
	}
	
	protected virtual void handleRight()
	{
		FollowFlowLinkNamed("Swipe");
	}
	
	protected void FollowFlowLinkNamed(string name)
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find( r => r.Name == name );
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
		}
		else
		{
			UnityEngine.Debug.Log("swipelistener: didn't find flow link named :" + name);
		}
	}
	
	public virtual void OnDestroy()
	{
		GestureHelper.swipeLeft -= leftHandler;
		GestureHelper.swipeRight -= rightHandler;
	}
}
