using UnityEngine;
using System.Collections;
using System;

public class PageIndicatorController : MonoBehaviour {
	protected int currentPage = 0;
	public int numPages = 1;
	
	float xSpacing = 60.0f;
	float indicatorHeight = 0.0f;
	
	GameObject spriteOn;
	
	// Use this for initialization
	void Start () {
		//load number of pages from datavault	- now exposed to flow
		// Actually, now the MultiPanelChild will tell us how many children there are, via the dataVault.
		try {
				UnityEngine.Debug.Log("Paging: attempting to retrieve number of pages");
				numPages = (int)DataVault.Get("numberOfPages");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("PageIndicator: couldn't retrieve number of pages from datavault");
			numPages = 6;
		}
		
		spriteOn = GameObject.Find("PageIndicator_on");
		if(spriteOn)
		{
			indicatorHeight = spriteOn.transform.localPosition.y;
		}
		else { UnityEngine.Debug.Log("PageIndicator: couldn't find on widget"); }
		
		//clone off element as needed and position
		for(int i=0; i<numPages; i++)
		{
			GameObject spriteOffi = GameObject.Find("PageIndicator_off" + i);
			//calculate position
			Vector2 pos = new Vector2(PosForIndex(i), indicatorHeight);
			spriteOffi.transform.localPosition = pos;
		}
		
		//initialise current page
		DataVault.Set("currentPageIndex",0);
		
		UpdateCurrentPage();
		
	}
	
	protected float PosForIndex(int i)
	{
		float parametricPos = i- (numPages -1) * 0.5f;
		return parametricPos * xSpacing;
	}
	
	// Update is called once per frame
	void Update () {
		//update current page
		UpdateCurrentPage();
	}
	
	protected void UpdateCurrentPage() {
		//look up current page in datavault
		try {
			currentPage = (int)DataVault.Get("currentPageIndex");
		} catch( Exception e ) {
			UnityEngine.Debug.Log("PageIndicator: couldn't retrieve current page from datavault");
			currentPage = 0;
		}
			

		//move selected indicator to position
		spriteOn = GameObject.Find("PageIndicator_on");
		spriteOn.transform.localPosition = new Vector2(PosForIndex(currentPage), indicatorHeight);
	}
	
	public void GhostProgressBeyondPage(int lastAllowedPage)
	{
		for(int i=lastAllowedPage+1; i<numPages; i++)
		{
			GameObject indicator = GameObject.Find("PageIndicator_off" + i);
			UISprite sprite = indicator.GetComponent("UISprite") as UISprite;
			sprite.color = new Color(0.9f, 0.9f, 0.9f);
		}
	}
	
	public void UnGhostAllPages()
	{
		for(int i=0; i<numPages; i++)
		{
			GameObject indicator = GameObject.Find("PageIndicator_off" + i);
			UISprite sprite = indicator.GetComponent("UISprite") as UISprite;
			sprite.color = UnityEngine.Color.black;
		}
	}
	
}
