using UnityEngine;
using System.Collections;
using System;
using RaceYourself;

public class DistanceMilestone : WorldObject {

	private int target = 500;
	
	// 3DText box. 
	public GameObject textObject;
	
	// Text Mesh for 3D Text.
	private TextMesh textMesh;
	
	/// <summary>
	/// Obtains the text mesh
	/// </summary>
	public override void Start () 
	{	
		// Get initial text mesh component.
		textMesh = textObject.GetComponent<TextMesh>();
		textMesh.text = UnitsHelper.SiDistance(target);

		base.Start();
		setRealWorldDist(target);
	}
	
	/// <summary>
	/// Updates the position of the markers
	/// </summary>
	public override void Update () 
	{
		// Get current distance travelled.
		double distance = Platform.Instance.LocalPlayerPosition.Distance;
		
		// If current distance is higher than target set the new text and position.
		if(distance > target + 50) 
		{
			target +=500;
			setRealWorldDist(target);
			textMesh.text = UnitsHelper.SiDistance(target);
		}

		base.Update();
	}

}

