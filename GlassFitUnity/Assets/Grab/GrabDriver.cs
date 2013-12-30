using UnityEngine;
using System.Collections;

public class GrabDriver : MonoBehaviour {
	
	public string app_secret = "foo";
	
	void Start () {
		//use this to enable debug output
		GrabBridge.ToggleLog(true);

		//this is all that's required to use the SDK
		GrabBridge.Start(app_secret);

		//optionally, you can send more analytics if you'd like
		SendOptionalAnalytics();
	}

	//optionally, you can send more analytics if you'd like
	void SendOptionalAnalytics() {
		Debug.Log("GrabDriver.SendAnalytics");

		string userid="lex";
		GrabBridge.FirstLogin(userid);
		
		JSONObject customEventError1  = new JSONObject();
		customEventError1.AddField("grabEventName", "dontdothis");

		GrabBridge.CustomEvent("error1", customEventError1);

		JSONObject customEventError2 = new JSONObject();
		JSONObject errorObj = new JSONObject();
		errorObj.AddField("nested","object");
		customEventError2.AddField("objectfield", errorObj);

		GrabBridge.CustomEvent("error2", customEventError2);

		string key="levelUp";
		int val=60;
		JSONObject validCustomEvent = new JSONObject();
		validCustomEvent.AddField(key, val);
		GrabBridge.CustomEvent("Cheevo", validCustomEvent);



		// type hints example
		JSONObject typeHintEvent = new JSONObject();
		// integer
		typeHintEvent.AddField("playerBerries_int", "97");
		// float
		typeHintEvent.AddField("playerStrength_float", "4.329");
		// seconds
		typeHintEvent.AddField("playerLevelDuration_sec", "42");
		// milliseconds
		typeHintEvent.AddField("playerDurationCloaked_ms", "2336");
		// money
		typeHintEvent.AddField("playerGold_money", "58");
		// no type hint specified, so you will see each data point on the graph
		typeHintEvent.AddField("playerLevel", "60");
		// no type hint specified, so you will see each data point on the graph
		typeHintEvent.AddField("playerClass", "warrior");
		GrabBridge.CustomEvent("battleFinished", typeHintEvent);
		
		
		string signature="foo";
		//http://developer.android.com/google/play/billing/billing_integrate.html
/*		string orderData = "{" + 
			"\"orderId\":\"12999763169054705758.1371079406387615\"," +
			"\"packageName\":\"com.example.app\"," +
			"\"productId\":\"exampleSku\"," +
			"\"purchaseTime\":1345678900000," +
			"\"purchaseState\":0," +
			"\"developerPayload\":\"bGoa+V7g/yqDXvKRqq+JTFn4uQZbPiQJo4pf9RzJ\"," +
			"\"purchaseToken\":\"rojeslcdyyiapnqcynkjyyjh\"" +
	 	"}";*/
	 	JSONObject orderData = new JSONObject();
	 	orderData.AddField("orderId", "12999763169054705758.1371079406387615");
	 	orderData.AddField("packageName", "com.example.app");
	 	orderData.AddField("productId", "exampleSku");
	 	orderData.AddField("purchaseTime", 1345678900000);
	 	orderData.AddField("purchaseState", 0);
	 	orderData.AddField("developerPayload", "bGoa+V7g/yqDXvKRqq+JTFn4uQZbPiQJo4pf9RzJ");
	 	orderData.AddField("purchaseToken", "rojeslcdyyiapnqcynkjyyjh");
	 	
		string price="1.00";

		
		GrabBridge.VerifyPurchase(signature, orderData, price, "USD");
	}
	
	void OnApplicationPause(bool pauseStatus) {
		if(pauseStatus) {
			GrabBridge.OnPause();
		} else {
			GrabBridge.OnResume();
		}
	}
	
	void OnApplicationQuit() {
		GrabBridge.OnQuit();
	}
	
	// Make this game object and all its transform children
	// survive when loading a new scene.
	void Awake () {
		DontDestroyOnLoad(transform.gameObject);
	}
}
