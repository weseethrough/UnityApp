using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FriendDisplay : MonoBehaviour {
	
	private Texture2D loading;
	private Texture wwwFriend;
	private WWW displayPic = null;
	
	private Friend[] friendList;
	private Vector2 startTouch = new Vector2(0, 0);
	private float curTouchDist = 0;
	private int currentFriend = 0;
	private float syncTime = 0;
	private bool imageFetched = false;
	
	private UITexture tex;
	
	// Use this for initialization
	void Start () {
		loading = (Texture2D)Resources.Load("loading", typeof(Texture2D));
		if(loading == null) {
			UnityEngine.Debug.Log("Friend Display: Loading texture not loaded");
		}
		tex = GetComponent<UITexture>();
		tex.material.mainTexture = loading;
		Platform.Instance.syncToServer();
		syncTime = Time.time;
		friendList = Platform.Instance.Friends();
		List<Friend> filtered = new List<Friend>();
		foreach (Friend friend in friendList) {
			if (friend.userId.HasValue || friend.hasGlass) filtered.Add(friend);
		}
		friendList = filtered.ToArray();
		DataVault.Set("screen_name", "Loading Screen Name...");
		UnityEngine.Debug.Log("Friend Display: started");
	}
	
	// Update is called once per frame
	void Update () {
		
//		if(Time.time > syncTime + 60) 
//		{
//			Platform.Instance.syncToServer();
//		}
		
		if(friendList.Length == 0) {
			DataVault.Set("screen_name", "Ha ha, you have no friends!");
		} else {
			if(!imageFetched){
				UnityEngine.Debug.Log("URL is: " + friendList[currentFriend].image);
				WWW displayLink = new WWW(friendList[currentFriend].image);
				if (displayLink != null){
					while(!displayLink.isDone) {
						if (displayLink.error != null) {
							tex.material.mainTexture = loading;
							UnityEngine.Debug.Log(displayLink.error);
							UnityEngine.Debug.Log("Friend Display: error getting display");
						} 
							//UnityEngine.Debug.Log("Friend Display: Loading Image...");
					} 
				
					tex.mainTexture = displayLink.texture;
					imageFetched = true;
					UnityEngine.Debug.Log("Friend Display: image obtained!");
					
				}
			}
			DataVault.Set("screen_name", friendList[currentFriend].name);
			int userId = 0;
			if (friendList[currentFriend].userId.HasValue) userId = friendList[currentFriend].userId.Value;
			DataVault.Set("current_friend", userId);
		}
		
		if(Input.touchCount == 1) 
		{
			if(Input.GetTouch(0).phase == TouchPhase.Began) {
				startTouch = Input.touches[0].position;
				UnityEngine.Debug.Log("Friend Display: Touch started");
			}
			
			if(Input.GetTouch(0).phase == TouchPhase.Moved) {
				curTouchDist = Input.GetTouch(0).position.x - startTouch.x;
				UnityEngine.Debug.Log("Friend Display: Touch moved");
			}
			
			if(Input.GetTouch(0).phase == TouchPhase.Ended) {
				UnityEngine.Debug.Log("Friend Display: Touch Ended");
				if((curTouchDist > (float)Screen.width / 3.0f) && currentFriend < friendList.Length - 1) {
					currentFriend ++;
					
					imageFetched = false;
				} else if((curTouchDist < (float)-Screen.width / 3.0f) && currentFriend > 0) {
					currentFriend--;
					imageFetched = false;
				}
				curTouchDist = 0;
			}
		}
	}
}
