// iPhone gyroscope-controlled camera demo v0.3 8/8/11
// Perry Hoberman <hoberman@bway.net>
// Directions: Attach this script to main camera.
// Note: Unity Remote does not currently support gyroscope. 

private var gyroBool : boolean;
private var gyro : Gyroscope;
private var rotFix : Quaternion;
private var started : boolean;
private var scaleX : float;
private var scaleY : float;
public var grid : GameObject;
private var gridOn : boolean;
private var gridTimer : float;
private var timerActive : boolean;
private var offsetFromStart : Quaternion;
private var hasGrid : boolean;
private var multipleRotation : Quaternion[];
private var prevRot : Quaternion;
private var prevRot2 : Quaternion;
private var addAmount : int;

function Start() {
	
	started = false;
	scaleX = Screen.width / 800.0f;
	scaleY = Screen.height / 500.0f;
	gridOn = false;
	gridTimer = 0.0f;
	timerActive = false;
	multipleRotation = new Quaternion[3];
	if(!grid) 
	{
		hasGrid = false;
	}
	else {
		hasGrid = true;
	}
	
	if(hasGrid) {
		grid.SetActive(false);
	}
	
	prevRot = Quaternion.identity;
	prevRot2 = Quaternion.identity;
	var originalParent = transform.parent; // check if this transform has a parent
	var camParent = new GameObject ("camParent"); // make a new parent
	camParent.transform.position = transform.position; // move the new parent to this transform position
	transform.parent = camParent.transform; // make this transform a child of the new parent
	camParent.transform.parent = originalParent; // make the new parent a child of the original parent
	
	gyroBool = Input.isGyroAvailable;
	
	if (gyroBool) {
		
		gyro = Input.gyro;
		gyro.enabled = true;
		offsetFromStart = gyro.attitude;
//		if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
//			camParent.transform.eulerAngles = Vector3(90,90,0);
//		} else if (Screen.orientation == ScreenOrientation.Portrait) {
//			camParent.transform.eulerAngles = Vector3(90,180,0);
//		}
//		
//		if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
//			rotFix = Quaternion(0,0,0.7071,0.7071);
//		} else if (Screen.orientation == ScreenOrientation.Portrait) {
//			rotFix = Quaternion(0,0,1,0);
//		}
		//Screen.sleepTimeout = 0;
	} else {
		print("NO GYRO");
	}
	
}

function OnGUI() {
	GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX,scaleY, 1));		
	GUI.depth = 7;
	
	if(!started)
	{
		offsetFromStart = gyro.attitude;
		offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
		started = true;
	}	
	
	if(GUI.Button(Rect(0, 400, 100, 100), "setGyro"))
	{ 
		if(timerActive) {
			gridOn = false;
		} else {
			offsetFromStart = gyro.attitude;
			gridOn = true;
				
		}
		gridTimer = 5.0f;
		
	}
	else if(Event.current.type == EventType.Repaint)
	{
		if(gridOn)
		{
			timerActive = true;
		} else
		{
			gridTimer = 0.0f;
			timerActive = false;
		}
	}	
}

function Update () {
	if (gyroBool) {
	
		var camRot : Quaternion = gyro.attitude;
		
		prevRot2 = prevRot;
		prevRot = camRot;
		//camRot = camRot;
		//camRot = Quaternion.Inverse(camRot);
		//camRot = camRot * Quaternion.EulerRotation(-90,90,90);
		//Global variable which holds the amount of rotations which 

		multipleRotation[0] = prevRot2;
		multipleRotation[1] = prevRot;
		multipleRotation[2] = camRot;
	
	//need to be averaged.
	//int addAmount = 0;

	//Global variable which represents the additive quaternion
	var addedRotation = Quaternion.identity;

	//The averaged rotational value
	var averageRotation : Quaternion;

	//multipleRotations is an array which holds all the quaternions
	//which need to be averaged.
	//Quaternion[] multipleRotations new Quaternion[totalAmount];

	//Loop through all the rotational values.
	var singleRotation : Quaternion;
	
	var w : float;
    var x : float;
    var y : float;
    var z : float;

	addAmount = 0;
	for(var i=0; i<multipleRotation.Length; i++){
		singleRotation = multipleRotation[i];
   		//Temporary values
    	

    	//Amount of separate rotational values so far

    	addAmount++;
    	var addDet = 1.0f / addAmount;
    	addedRotation.w += singleRotation.w;
    	w = addedRotation.w * addDet;
    	addedRotation.x += singleRotation.x;
    	x = addedRotation.x * addDet;
   		addedRotation.y += singleRotation.y;
    	y = addedRotation.y * addDet;
    	addedRotation.z += singleRotation.z;
    	z = addedRotation.z * addDet; 

    	//Normalize. Note: experiment to see whether you

    	//can skip this step.

   		var D = 1.0f / (w*w + x*x + y*y + z*z);
    	w *= D;
    	x *= D;
    	y *= D;
    	z *= D;
    
    	//The result is valid right away, without 
    	//first going through the entire array.
    	averageRotation = new Quaternion(x, y, z, w);
	}		
		if(timerActive && gridOn)
		{
			gridTimer -= Time.deltaTime;
			UnityEngine.Debug.Log("Camera: Grid timer is: " + gridTimer.ToString());
			if(gridTimer < 0.0f)
			{
				gridOn = false;
				timerActive = false;
			}
		}
		
		if(hasGrid) {
			grid.SetActive(gridOn);
		}
		averageRotation = Quaternion.Inverse(averageRotation * Quaternion.Inverse(offsetFromStart));
		transform.localRotation = averageRotation;
	}
}
