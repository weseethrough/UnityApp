using UnityEngine;

 

public class quarternRotate : MonoBehaviour

{

    private double _lastCompassUpdateTime = 0;

    private Quaternion _correction = Quaternion.identity;
	
	private Quaternion _correction1 = Quaternion.identity;
	
    private Quaternion _targetCorrection = Quaternion.identity;
	
	private Quaternion _targetCorrection1 = Quaternion.identity;

    private Quaternion _compassOrientation = Quaternion.identity;
    
	private Quaternion _compassOrientation1 = Quaternion.identity;
	
	public Quaternion offset  = Quaternion.identity;
    

    void Start()

    {

        Input.gyro.enabled = true;

        Input.compass.enabled = true;
		
		
	  Quaternion gyroOrientation1 = Quaternion.Euler (-90, 0, 0) * Input.gyro.attitude;// * Quaternion.Euler(0, 0, 90);

            // Work out an orientation based primarily on the compass

            Vector3 gravity1 = Input.gyro.gravity.normalized;

            Vector3 flatNorth1 = Input.compass.rawVector - 

                Vector3.Dot(gravity1, Input.compass.rawVector) * gravity1;

            _compassOrientation1 = Quaternion.Euler (180, 0, 0) * Quaternion.Inverse(Quaternion.LookRotation(flatNorth1, -gravity1)); //* Quaternion.Euler (0, 0, 90);

            

            // Calculate the target correction factor

            _targetCorrection1 = _compassOrientation1 * Quaternion.Inverse(gyroOrientation1);
            _correction1 = _targetCorrection1;

      

        // Easy bit :)
		offset =  this.transform.rotation; //_correction1 *  Quaternion.Inverse(gyroOrientation1);
        //transform.rotation = _correction *  Quaternion.Inverse(gyroOrientation);

    }

    

    void Update()

    {

        // The gyro is very effective for high frequency movements, but drifts its 

        // orientation over longer periods, so we want to use the compass to correct it.

        // The iPad's compass has low time resolution, however, so we let the gyro be

        // mostly in charge here.

        

        // First we take the gyro's orientation and make a change of basis so it better 

        // represents the orientation we'd like it to have

        Quaternion gyroOrientation = Quaternion.Euler (-90, 0, 0) * Input.gyro.attitude;// * Quaternion.Euler(0, 0, 90);

    

        // See if the compass has new data

        if (Input.compass.timestamp > _lastCompassUpdateTime)

        {

            _lastCompassUpdateTime = Input.compass.timestamp;

        

            // Work out an orientation based primarily on the compass

            Vector3 gravity = Input.gyro.gravity.normalized;

            Vector3 flatNorth = Input.compass.rawVector - 

                Vector3.Dot(gravity, Input.compass.rawVector) * gravity;

            _compassOrientation = Quaternion.Euler (180, 0, 0) * Quaternion.Inverse(Quaternion.LookRotation(flatNorth, -gravity)); //* Quaternion.Euler (0, 0, 90);

            

            // Calculate the target correction factor

            _targetCorrection = _compassOrientation * Quaternion.Inverse(gyroOrientation);

        }

        

        // Jump straight to the target correction if it's a long way; otherwise, slerp towards it very slowly

       if (Quaternion.Angle(_correction, _targetCorrection) > 15)

            _correction = _targetCorrection;

        else

            _correction = Quaternion.Slerp(_correction, _targetCorrection, 0.09f);

       Quaternion halfway =  ( _correction *    Quaternion.Inverse(gyroOrientation)) ;

        // Easy bit :)

        this.transform.rotation =  offset * halfway;

    }

}