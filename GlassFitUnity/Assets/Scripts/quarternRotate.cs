using UnityEngine;

 

public class quarternRotate : MonoBehaviour

{

	
    private Quaternion   gyroOrientation;

	private Quaternion _targetCorrection1 = Quaternion.identity;

	private Quaternion _compassOrientation1 = Quaternion.identity;
	
	private bool started = false;

	public Quaternion offset  = Quaternion.identity;
    

    void Start()

    {

        Input.gyro.enabled = true;

        Input.compass.enabled = true;
		
		
	  Quaternion gyroOrientation1 = Quaternion.Euler (-90, 0, 0) * Input.gyro.attitude;// * Quaternion.Euler(0, 0, 90);



		offset =  this.transform.rotation; //_correction1 *  Quaternion.Inverse(gyroOrientation1);
        //transform.rotation = _correction *  Quaternion.Inverse(gyroOrientation);

    }

    	void OnGUI()
	{
		if(!started)
		{
			offset = gyroOrientation;
			offset = Quaternion.Euler(0, offset.eulerAngles.y, 0);
			started = true;
		}
		
		if(GUI.Button (new Rect(0, Screen.height - 100, 100, 100), "setGyro"))
		{ 
			offset = gyroOrientation;
			offset = Quaternion.Euler(0, offset.eulerAngles.y, 0);
		}
	}

    void Update()

    {
		
		if(this.camera.fieldOfView == 10)
		AutoFade.LoadLevel(1, 1, 1, Color.black);


        gyroOrientation = Quaternion.Euler (-90, 0, 0) * Input.gyro.attitude;// * Quaternion.Euler(0, 0, 90);

    
        Quaternion halfway =  (offset * Quaternion.Inverse(gyroOrientation)) ;

        
        this.transform.rotation =   halfway;

    }

}