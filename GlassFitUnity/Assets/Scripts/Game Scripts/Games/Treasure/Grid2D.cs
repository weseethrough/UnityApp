using UnityEngine;
using System.Collections;

public class Grid2D : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Platform.Instance.GetPlayerOrientation().SetAutoReset(false);
	}
	
	// Update is called once per frame
	void Update () {
		float yaw = Platform.Instance.GetPlayerOrientation().AsNorthReference(); //* (360 / (Mathf.PI * 2));
			
		UnityEngine.Debug.Log("Treasure: yaw is " + yaw.ToString("f2"));
			
		//transform.rotation = Quaternion.Euler(new Vector3(0, yaw, 0));
		
		Vector2 forward = new Vector2(-Camera.main.transform.forward.x, -Camera.main.transform.forward.z);
		
		Vector2 uvOffset = renderer.material.mainTextureOffset;
		
		uvOffset += forward * (Platform.Instance.LocalPlayerPosition.Pace * Time.deltaTime);
		
		renderer.material.mainTextureOffset = uvOffset;
	}
}
