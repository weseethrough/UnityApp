using UnityEngine;
using System.Collections;

public class CongratsDisplay : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
//		DataVault.Set("distance", Platform.Instance.Distance());
//		DataVault.Set("time", Platform.Instance.Time());
//		DataVault.Set("calories", Platform.Instance.Calories());
//		DataVault.Set("points", Platform.Instance.GetCurrentPoints());
		DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.OpeningPointsBalance());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
