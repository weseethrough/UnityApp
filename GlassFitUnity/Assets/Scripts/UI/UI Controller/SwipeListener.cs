using UnityEngine;
using System.Collections;
using System;

public class SwipeListener : MonoBehaviour {
	
	GestureHelper.OnSwipeLeft leftHandler;
	GestureHelper.OnSwipeRight rightHandler;
	
	// Use this for initialization
	public virtual void Start () {
		leftHandler = new GestureHelper.OnSwipeLeft( () => {
			handleLeft();
		});
		GestureHelper.onSwipeLeft += leftHandler;
		
		rightHandler = new GestureHelper.OnSwipeRight( () => {
			handleRight();
		});
		GestureHelper.onSwipeRight += rightHandler;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	protected virtual void handleLeft()
	{
		if(FlowStateBase.FollowFlowLinkNamed("Back"))
		{
			int currentPage = 1;
			//increment page
			try {
				currentPage = (int)DataVault.Get("currentPageIndex");
			} catch (Exception e) {
				UnityEngine.Debug.Log("Couldn't get current page index");
				UnityEngine.Debug.LogException(e);
			}
			try {
				DataVault.Set("currentPageIndex", currentPage - 1);
			} catch (Exception e) {
				UnityEngine.Debug.Log("Couldn't set current page index");
				UnityEngine.Debug.LogException(e);
			}
			
		}
	}
	
	protected virtual void handleRight()
	{
		if(FlowStateBase.FollowFlowLinkNamed("Swipe"))
		{
			int currentPage = 1;
			//decrement page
			try {
				currentPage = (int)DataVault.Get("currentPageIndex");
			} catch (Exception e) {
				UnityEngine.Debug.LogWarning("Couldn't get current page index");
				UnityEngine.Debug.LogException(e);
			}
			try {
				DataVault.Set("currentPageIndex", currentPage + 1);
			} catch (Exception e) {
				UnityEngine.Debug.LogWarning("Couldn't set current page index");
				UnityEngine.Debug.LogException(e);
			}
		}
	}
	
	public virtual void OnDestroy()
	{
		GestureHelper.onSwipeLeft -= leftHandler;
		GestureHelper.onSwipeRight -= rightHandler;
	}
}
