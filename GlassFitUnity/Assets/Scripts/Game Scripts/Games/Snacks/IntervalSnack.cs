using UnityEngine;
using System.Collections;

public class IntervalSnack : SnackBase {
    private GameObject paceMaker;
    private IntervalTargetTracker tracker;
    
    public AudioClip sprintClip;
    public AudioClip jogClip;
    
    private AudioSource audioSource;
    
    public override void Begin ()
    {
        UnityEngine.Debug.Log("Interval snack: started.");
        // Call the base function
        base.Begin ();
        
        //SetMainCamera(false);
        
        //SetThirdPerson(false);
        
        //SetTrack(false);
        
        StartCoroutine("PaceMake");
    }
    
    // Use this for initialization
    void Start ()
    {
        base.Start ();
        
        paceMaker = GameObject.FindGameObjectWithTag("Pacemaker");
        tracker = paceMaker.GetComponent<IntervalTargetTracker>();
        audioSource = GetComponent<AudioSource>();
    }
    
    IEnumerator PaceMake()
    {
        DataVault.Set("death_colour", UIColour.green);
        
        UnityEngine.Debug.Log("Interval snack: feeling peckish.");
        yield return new WaitForSeconds(2);

        for (uint cycle = 1; cycle <= 5; cycle++)
        {
            if (audioSource.isPlaying)
                audioSource.Stop ();
            
            DataVault.Set("snack_result_desc", "Cycle #" + cycle);
            
            DataVault.Set("snack_result", "Sprint!");
            audioSource.clip = sprintClip;
            audioSource.Play ();
            StartCoroutine( ShowBanner(3f) );
            tracker.speedMinsPerKm = 8f;
            yield return new WaitForSeconds(30);
            
            DataVault.Set("snack_result", "Jog slowly.");
            audioSource.Stop ();
            audioSource.clip = jogClip;
            audioSource.Play ();
            StartCoroutine( ShowBanner(3f) );
            tracker.speedMinsPerKm = 15f;
            yield return new WaitForSeconds(30);
            
            //            yield return Phase (cycle, "Sprint!", 30);
            //            yield return Phase (cycle, "Jog slowly.", 30);
        }
        
        UnityEngine.Debug.Log("Interval snack: no longer hungry.");
        Finish();
    }
    
    //    IEnumerator Phase(uint cycle, string label, int durationSecs)
    //    {
    //        DataVault.Set("snack_result", label);
    //        StartCoroutine( ShowBanner(3f) );
    //        yield return new WaitForSeconds(durationSecs);
    //    }
    
    // Update is called once per frame
    void Update ()
    {
        base.Update ();
        
        float pacemakerZ = paceMaker.transform.position.z;

        UpdateAhead(pacemakerZ);
    }
}