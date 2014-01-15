using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//store a list of junctions
public class TrainTrackController {

	protected List<TrainTrackJunction> junctionList;
	int currentJunction = -1;
	
	// Use this for initialization
	void Start () {
		//populate the list of junctions	
	}
	
	// Update is called once per frame
	void Update () {
		//if we're at a junction
		if(currentJunction != -1)
		{
			TrainTrackJunction j = junctionList[currentJunction];
				
		}
		
	}

}
