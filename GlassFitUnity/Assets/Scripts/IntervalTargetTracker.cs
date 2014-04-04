using UnityEngine;
using System.Collections;

public class IntervalTargetTracker : TargetController {

    public float speedMinsPerKm;

    private float initialZ;
    private float speed;

    private float pacemakerDistanceTravelled = 0f;

    // Use this for initialization
	void Start ()
    {
        initialZ = gameObject.transform.position.z;
        speed = UnitsHelper.KmPaceToSpeed(speedMinsPerKm);
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Get 1D distance travelled along Z
        float playerDistanceTravelled = (float) Platform.Instance.LocalPlayerPosition.Distance;

        pacemakerDistanceTravelled += speed * Time.deltaTime;
        
        Vector3 newPos = new Vector3(
            gameObject.transform.position.x,
            gameObject.transform.position.y,
            initialZ - playerDistanceTravelled + pacemakerDistanceTravelled
        );

        gameObject.transform.position = newPos;
	}
}