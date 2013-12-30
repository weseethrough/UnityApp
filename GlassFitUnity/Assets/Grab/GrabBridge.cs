using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class GrabBridge : MonoBehaviour {

	private static bool isInitialized = false;
	
	#pragma warning disable 0414
	private static string unity_version = "Unity_2.1.16";
	#pragma warning restore 0414

	public enum Grab_Gateway {
		LOCAL,
		DEV,
		STAGING,
		LIVE,
	}

	/* Public interface for using inside C# / JS code */

	public static void Start(string clientstart) {
		Start(clientstart, Grab_Gateway.LIVE);
	}
	
	public static void Start(string clientstart, Grab_Gateway url) {
		if(isInitialized) {
			//Already initialized this throw exception or just return?
			return;
		}
		
		//call native code
		NativeStart(clientstart, url);
	}

	public static void CustomEvent(string name, JSONObject data)
	{
		NativeCustomEvent(name, data);
	}
	public static void FirstLogin(string userid)
	{
		NativeFirstLogin(userid);
	}
	public static void OnPause()
	{
		NativeOnPause();
	}
	public static void OnQuit()
	{
		NativeOnQuit();
	}
	public static void OnResume()
	{
		NativeOnResume();
	}
	public static void ToggleLog(bool enable)
	{
		NativeToggleLog(enable);
	}
	//orderData is a JSONObject
	public static void VerifyPurchase(string signature, JSONObject orderData, string price, string currency)
	{
		NativeVerifyPurchase(signature, orderData, price, currency);
	}

	
	/* Interface to native implementations */
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _initWithSecret(string clientstart);
	[DllImport ("__Internal")]
	private static extern void _initWithSecretUrl(string clientstart, string url);
	
	[DllImport ("__Internal")]
	private static extern void _appendGAV(string append);
	
	[DllImport ("__Internal")]
	private static extern void _customEvent(string key, string val);
	
	[DllImport ("__Internal")]
	private static extern void _firstLogin(string userid);

	[DllImport ("__Internal")]
	private static extern void _toggleLog();

	[DllImport ("__Internal")]
	private static extern void _sessionStart();
#endif

	private static void NativeStart(string clientstart, Grab_Gateway debugUrl) {
		
#if UNITY_EDITOR
#elif UNITY_ANDROID
	//interface to the android sdk's jar file
	//only works on a real device ... will not work in UNITY_EDITOR (it throws error about JNI not found)

		using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
			using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
				using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab")) {
					int iap_version = cls_Plugin.GetStatic<int>("INAPPBILLING_NONE");
					cls_Plugin.CallStatic("appendGAV", unity_version);
					cls_Plugin.CallStatic("init", obj_Activity, clientstart, iap_version);
					cls_Plugin.CallStatic("handleStart");
				}
			}
		}
#elif UNITY_IPHONE
		_appendGAV(unity_version);
		_initWithSecret(clientstart);
		_sessionStart();
#endif
		return;
	}

	private static void NativeCustomEvent(string name, JSONObject data) {
		Debug.Log(name);
		Debug.Log(data.ToString());
#if UNITY_EDITOR
#elif UNITY_ANDROID
		using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab")) {
			cls_Plugin.CallStatic("_customEvent", name, data.ToString());
		}
#elif UNITY_IPHONE
		_customEvent(name, data.ToString());
#endif
		return;
	}
	
	private static void NativeFirstLogin(string userid) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
		using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab")) {
			cls_Plugin.CallStatic("firstLogin", userid);
		}
#elif UNITY_IPHONE
		_firstLogin(userid);
#endif
		return;
	}
	
	private static void NativeOnPause() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
		using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab")) {
			cls_Plugin.CallStatic("handlePause");
		}
#elif UNITY_IPHONE
#endif
		return;
	}

	private static void NativeOnQuit() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
		using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab"))  {
			cls_Plugin.CallStatic("handleStop");
		}
#elif UNITY_IPHONE
#endif
		return;
	}

	private static void NativeOnResume() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
		using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab")) {
			cls_Plugin.CallStatic("handleResume");
		}
#elif UNITY_IPHONE
#endif
		return;
	}

	private static void NativeToggleLog(bool enable) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
		using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab")) {
			cls_Plugin.CallStatic("toggleLog", enable);
		}
#elif UNITY_IPHONE
		_toggleLog();
#endif
		return;
	}
	
	private static void NativeVerifyPurchase(string signature, JSONObject orderData, string price, string currency) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
		using (AndroidJavaClass cls_Plugin = new AndroidJavaClass("com.grab.Grab.Grab")) {
			cls_Plugin.CallStatic("_verifyPurchase", signature, orderData.ToString(), price, currency);
		}
#elif UNITY_IPHONE
#endif
		return;
	}

}
