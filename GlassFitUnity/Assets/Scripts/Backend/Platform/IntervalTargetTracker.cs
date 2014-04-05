using UnityEngine;
using System.Collections;

public class IntervalTargetTracker : TargetController {
    
    private float initialZ;
    private float speed;

    private float speedMinsPerKmInternal;

    public float speedMinsPerKm {
        get
        {
            return speedMinsPerKmInternal;
        }
        set
        {
            speedMinsPerKmInternal = value;
            speed = UnitsHelper.KmPaceToSpeed(speedMinsPerKmInternal);
            if (anim != null)
            {
                anim.SetFloat("Speed", speed);
            }
        }
    }

    private float pacemakerDistanceTravelled = 0f;

    private Animator anim;
    
    // Use this for initialization
    void Start ()
    {
        initialZ = gameObject.transform.position.z;
        speed = UnitsHelper.KmPaceToSpeed(speedMinsPerKm);
        anim = GetComponent<Animator>();
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