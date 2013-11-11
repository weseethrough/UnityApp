using UnityEngine;
using System.Collections;

public class GameSelect : MonoBehaviour {
public Quaternion offsetFromStart;
	public Quaternion camFromStart;
	public Quaternion planarOffset;
	private bool nextPage = false;
	private bool headTurnR = false;
	private bool headTurnL = false;
	private bool start = true;
	
	private int currentPage = 1;
	private float currentAngle = 0;
	
	private float leftLimit = 270;
	private float rightLimit = 90;
	private float leftMid = 335;
	private float rightMid = 25;
	private float endPoint = 90;
	private float midPoint = 25;
	private float pageTimer = 0;
	
	private float startTimer = 1;
	private bool started = false;
	private float offsetY = 0;
	
	// Slider
	private float turnSlider;
	private float deltaAngle;
	private Rect sliderBox;
	float test = 0;
	
	// Use this for initialization
	void Start () {
		// you can use the API directly:
		//Sensor.Activate(Sensor.Type.RotationVector);
		//Slider
		sliderBox = new Rect((Screen.width/2), 200, 300, 100);
		
		// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
		SensorHelper.ActivateRotation();
		offsetFromStart = SensorHelper.rotation;
		camFromStart = transform.rotation;
		
		currentAngle = SensorHelper.rotation.eulerAngles.y;
		
		//SensorHelper.TryForceRotationFallback(RotationFallbackType.RotationQuaternion);
		useGUILayout = false;
	}
	
	
	void OnGUI()
	{
		if(GUI.Button (new Rect(Screen.width-120, Screen.height-100, 100, 100), "Reset"))
		{ 
			offsetFromStart = SensorHelper.rotation;
			
			currentAngle = SensorHelper.rotation.eulerAngles.y;
			
			offsetY = 0 - SensorHelper.rotation.eulerAngles.y;
			
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
						
		}
		
		GUIStyle lab = new GUIStyle(GUI.skin.label);
		lab.fontSize = 32;
		
		GUI.Label(new Rect(0, 0, 300, 300), SensorHelper.rotation.eulerAngles.y.ToString(), lab);
		GUI.Label(new Rect(Screen.width/2, 0, 300, 300), currentAngle.ToString(),lab);
		GUI.Label(new Rect(Screen.width-300, 0, 300, 300), deltaAngle.ToString(), lab);
		GUI.Label(new Rect(0, Screen.height-300, 200, 200), leftLimit.ToString(), lab);
		GUI.Label(new Rect(Screen.width/4, Screen.height-300, 200, 200), leftMid.ToString(), lab);
		GUI.Label(new Rect(Screen.width/2, Screen.height-300, 200, 200), rightLimit.ToString(), lab);
		GUI.Label(new Rect((Screen.width/4)* 3, Screen.height-300, 200, 200), rightMid.ToString(), lab);
		
		
		switch(currentPage) {
		case 1:
			GUI.Label(new Rect(0, Screen.height/2, 300, 300), "Page 1",lab);
			break;
			
		case 2:
			GUI.Label(new Rect(Screen.width/2, Screen.height/2, 300, 300), "Page 2",lab);
			break;
			
		case 3:
			GUI.Label(new Rect(Screen.width-300, Screen.height/2, 300, 300), "Page 3",lab);
			break;
		default:
			GUI.Label(new Rect(Screen.width/2, Screen.height/2, 300, 300), "Something Wrong!", lab);
			break;
		};
		
	}
		
	// Update is called once per frame
	void Update () {
		
		startTimer -= Time.deltaTime;
		
		if(startTimer <= 0 && !started)
		{
			currentAngle = SensorHelper.rotation.eulerAngles.y;	
			offsetY = 0 - SensorHelper.rotation.eulerAngles.y;
			started = true;		
		}
		
		deltaAngle = SensorHelper.rotation.eulerAngles.y + offsetY;
		if(deltaAngle < 0)
		{
			deltaAngle += 360;
		}
		
		if(deltaAngle > rightMid && deltaAngle < rightLimit)
		{
			headTurnR = true;
		} 
		
		if(deltaAngle < leftMid && deltaAngle > leftLimit) 
		{
			headTurnL = true;	
		}
		
		if(headTurnR && deltaAngle < rightMid && deltaAngle > 0 && currentPage < 3)
		{
			currentPage++;
			headTurnR = false;
		} 
		
		if(headTurnL && deltaAngle > leftMid && deltaAngle < 360 && currentPage > 1)
		{
			currentPage--;
			headTurnL = false;
		}
		
		if(headTurnL && headTurnR)
		{
			headTurnL = false;
			headTurnR = false;
		}		
	}
}
