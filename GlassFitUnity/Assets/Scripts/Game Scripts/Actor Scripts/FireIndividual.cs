using UnityEngine;
using System.Collections;

public class FireIndividual : MonoBehaviour {
	
	public float scaleFrequency = 1.0f;
	public float scaleAmplitude = 1.75f;
	public float moveFrequency = 0.5f;
	public float moveAmplitude = 100f;
	public float frequencyVariance = 0.2f;
	public float scaleVariance = 1.0f;
	public float moveVariance = 1.0f;
	
	public float baseScale = 1;
	/// <summary>
	/// Start this instance. Pick randomised variations for frequency and amplitudes
	/// </summary>
	void Start () {
		//pick random amplitudes/frequencies
		scaleFrequency = scaleFrequency + Random.Range(-frequencyVariance, frequencyVariance);
		scaleAmplitude = scaleAmplitude + Random.Range(-scaleVariance, scaleVariance);
		moveFrequency = moveFrequency + Random.Range(-frequencyVariance, frequencyVariance);
		moveAmplitude = moveAmplitude + Random.Range (-moveVariance, moveVariance);
	}
	
/// <summary>
/// Update this instance. perturb position of the child mesh. Perturb scale of this container object (so we can scale from the bottom, not the centre)
/// </summary>
	public virtual void Update () {
		//perturb based on frequencies
		
		
		
		transform.FindChild("FireMesh").transform.localPosition = new Vector3( 	moveAmplitude * Mathf.Sin(Time.time/moveFrequency) + Random.Range(-moveVariance,moveVariance),
											0,
											Random.Range(-moveVariance, moveVariance));
		float scaleY =  scaleAmplitude * (1.0f +  scaleVariance * Mathf.Sin(Time.time) + Random.Range(-scaleVariance, scaleVariance) );
		//transform.localScale = new Vector3( baseScale, baseScale * scaleY, baseScale);
		transform.localScale = new Vector3(baseScale, baseScale * scaleY, baseScale);
		
		
	
	}
}
