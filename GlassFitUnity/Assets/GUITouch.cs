using UnityEngine;
using System.Collections;

public class GUITouch : MonoBehaviour {
	public int touchCount = 0;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
        foreach (Touch touch in Input.touches) {
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                touchCount++;
        }
		
		if(touchCount > 0) {
			
	}
}
}