using UnityEngine;

public class QuarternRotate : MonoBehaviour {
    private Quaternion gyroOrientation;

	private Quaternion _targetCorrection1 = Quaternion.identity;

	private Quaternion _compassOrientation1 = Quaternion.identity;
	
	private bool started = false;

	public Quaternion offset  = Quaternion.identity;
	private float scaleX;
	private float scaleY;
	public GameObject grid;
    private bool gridOn = false;
	private float gridTimer = 0.0f;
	private bool timerActive = false;

    void Start()
    {

        Input.gyro.enabled = true;

        Input.compass.enabled = true;
		
		
	  	Quaternion gyroOrientation1 = Quaternion.Euler (-90, 0, 0) * Input.gyro.attitude;// * Quaternion.Euler(0, 0, 90);

		grid.SetActive(false);
		scaleX = (float)Screen.width / 800.0f;
		scaleY = (float)Screen.height / 500.0f;

		offset =  this.transform.rotation; //_correction1 *  Quaternion.Inverse(gyroOrientation1);
        //transform.rotation = _correction *  Quaternion.Inverse(gyroOrientation);

    }

    void OnGUI()
	{
		if(!started)
		{
			offset = gyroOrientation;
			started = true;
		}
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX,scaleY, 1));		
		GUI.depth = 7;

		if(GUI.RepeatButton(new Rect(200, 0, 400, 250), "", GUIStyle.none))
		{ 
			if(timerActive) {
				gridOn = false;
			} else {
				offset = gyroOrientation;
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

    void Update()
    {		
//		if(this.camera.fieldOfView == 10)
//		AutoFade.LoadLevel(1, 1, 1, Color.black);

        gyroOrientation =  Quaternion.Euler(Input.gyro.attitude.eulerAngles.y, -Input.gyro.attitude.eulerAngles.x, 0);

        Quaternion halfway =  (offset * Quaternion.Inverse(gyroOrientation)) ;
		
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
		
		grid.SetActive(gridOn);
        
        this.transform.rotation =   halfway;

    }

}