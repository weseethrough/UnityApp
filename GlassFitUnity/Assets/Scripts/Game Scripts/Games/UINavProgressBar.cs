using UnityEngine;
using System.Collections;

public class UINavProgressBar : MonoBehaviour {
	
	public int numPages = 2;
	public int currentPage = 0;			//0 base index
	public bool show = true;
	
	//fractional position of the progress indicator, since it will move around.
	private float currentPageFractional = 0.0f;
	
	const float driftSpeed = 0.2f;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//move towards dest
		if(Mathf.Abs(currentPageFractional - currentPage) < 0.2f)
		{
			//snap there
			currentPageFractional = currentPage;
		}
		else
		{
			//drift closer
			if(currentPageFractional < currentPage) { currentPageFractional += driftSpeed; }
			else { currentPageFractional -= driftSpeed; }
		}
	}
	
	void OnGUI () {
		if(show)
		{
			//draw progress bar in its pos
			float segmentWidth = (Screen.width / numPages);
			float segmentHeight = 10.0f;
			float posLeft = segmentWidth * currentPageFractional;
					
			Rect progressRect = new Rect(posLeft, Screen.height - segmentHeight, segmentWidth, segmentHeight);
			
			Texture tex = Resources.Load("tint_green", typeof(Texture)) as Texture;
			GUI.DrawTexture(progressRect, tex);
		}
	}
}
