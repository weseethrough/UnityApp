using UnityEngine;
using System.Collections;

/// <summary>
/// HUD controller.
/// Interface for showing/hiding HUD elements.
/// Responsible for 
/// </summary>
public class HUDController : MonoBehaviour {
	
	private GameObject aheadBox = null;
	private GameObject aheadUnits = null;
	private GameObject timeBox = null;
	private GameObject timeUnits = null;
	private GameObject distanceBox = null;
	private GameObject distanceUnits = null;
	private GameObject paceBox = null;
	private GameObject paceUnits = null;
	private GameObject caloriesBox = null;
	private GameObject caloriesUnits = null;
	private GameObject pointsBox = null;
	private GameObject pointsUnits = null;
	
	bool shouldShowInstrumentation = true;
	bool shouldShowAheadBox = true;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void setInstrumentationVisible(bool visible) {
		shouldShowInstrumentation = visible;
	}
	
	protected void findLabelAndSetVisible(string name, ref GameObject localVar, bool visible)
	{
		if(localVar == null)
		{
			//find it
			localVar = GameObject.Find(name);
		}
		

		if(localVar != null)
		{
			//if we found it, set its activeness
			localVar.SetActive(visible);
		}
		else
		{
			if(visible) 
			{
				UnityEngine.Debug.LogWarning("HUD: Trying to make element	visible without a reference to it stored");
			}
		}
	}
	
	/// <summary>
	/// Public interface to set the instrumentation visible.
	/// </summary>
	/// <param name='visible'>
	/// Visible.
	/// </param>
	public void SetInstrumentationVisible(bool visible) {
		shouldShowInstrumentation = visible;	
	}
		
	//update element visibilities. Internal function called later on to set visibility.
	protected void UpdateInstrumentationVisible() {
		//UnityEngine.Debug.Log("HUD: trying to set visibility of instrumentation to :" + shouldShowInstrumentation);

		findLabelAndSetVisible("TimeBox", ref timeBox, shouldShowInstrumentation);
		findLabelAndSetVisible("TimeUnits_Laligned", ref timeUnits, shouldShowInstrumentation);
		findLabelAndSetVisible("DistanceBox", ref distanceBox, shouldShowInstrumentation);
		findLabelAndSetVisible("DistanceUnits_Laligned", ref distanceUnits, shouldShowInstrumentation);
		findLabelAndSetVisible("PaceBox", ref paceBox, shouldShowInstrumentation);
		findLabelAndSetVisible("PaceUnits", ref paceUnits, shouldShowInstrumentation);
		findLabelAndSetVisible("CaloriesBox", ref caloriesBox, shouldShowInstrumentation);
		findLabelAndSetVisible("CalsUnits", ref caloriesUnits, shouldShowInstrumentation);
		findLabelAndSetVisible("PointsBox", ref pointsBox, shouldShowInstrumentation);
		findLabelAndSetVisible("PointsUnits", ref pointsUnits, shouldShowInstrumentation);
	}
	
	public void setAheadBoxVisible(bool visible) {
		shouldShowAheadBox = visible;
	}
	
	protected void UpdateAheadBoxVisible() {
		findLabelAndSetVisible("AheadBox", ref aheadBox, shouldShowAheadBox);
	}
	
	public void OnGUI()
	{
		UpdateInstrumentationVisible();
		UpdateAheadBoxVisible();
	}
		
}
