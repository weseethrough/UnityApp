using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GlassPanel : Panel 
{

    bool lookingUp = false;
    float screenBackDelay = 0.0f;

	public GlassPanel() {}
    public GlassPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    public override string GetDisplayName()
    {
        base.GetDisplayName();

        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "GlassPanel: " + gName.Value;
        }
        return "GlassPanel: UnInitialzied";
    }

    public override void EnterStart()
    {
        base.EnterStart();

        Transform data = GameObjectUtils.FindChildInTree(physicalWidgetRoot.transform, "ExtraData");
        if (data != null)
        {
            data.gameObject.SetActive(false);
        }
        
        data = GameObjectUtils.FindChildInTree(physicalWidgetRoot.transform, "Distance");
        if (data != null)
        {
            data.gameObject.SetActive(true);
            Vector3 pos = data.transform.localPosition;
            pos.y = 100;
            data.transform.localPosition = pos;
            
        }         
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (Platform.Instance.OnGlass())
        {
            PlayerOrientation p = Platform.Instance.GetPlayerOrientation();

            Quaternion q = p.AsQuaternion();
            Vector3 rot = q.eulerAngles;

            float angle = 360.0f - rot.x;

            if (angle > 25 && angle < 180 )
            {
                if (!lookingUp)
                {
                    lookingUp = true;
                    Transform data = GameObjectUtils.FindChildInTree(physicalWidgetRoot.transform, "ExtraData");
                    if (data != null)
                    {
                        data.gameObject.SetActive(true);
                    }

                    data = GameObjectUtils.FindChildInTree(physicalWidgetRoot.transform, "Distance");
                    if (data != null)
                    {
                        data.gameObject.SetActive(true);
                        Vector3 pos = data.transform.localPosition;
                        pos.y = 0;
                        data.transform.localPosition = pos;
                    }
                }
                screenBackDelay = 3000.0f;
            }
            else
            {
                screenBackDelay -= 30.0f;
                if (lookingUp && screenBackDelay <= 0)
                {
                    lookingUp = false;
                    Transform data = GameObjectUtils.FindChildInTree(physicalWidgetRoot.transform, "ExtraData");
                    if (data != null)
                    {
                        data.gameObject.SetActive(false);
                    }

                    data = GameObjectUtils.FindChildInTree(physicalWidgetRoot.transform, "Distance");
                    if (data != null)
                    {
                        data.gameObject.SetActive(true);
                        Vector3 pos = data.transform.localPosition;
                        pos.y = 100;
                        data.transform.localPosition = pos;
                    }
                }                
            }
        }
    }

    public void Open()
    {

    }

    /// <summary>
    /// Function to trigger animation on specified clip
    /// </summary>
    /// <param name="go">game object containing animation component among children. Only one is expected</param>
    /// <param name="animName">namination name existing on animation component</param>
    /// <param name="direction">direction which animation should be animated to</param>
    /// <param name="requireReset">do you want to reset anmiation before play?</param>    
    /// <returns></returns>
    private void Play(GameObject go, string animName, AnimationOrTween.Direction direction, bool requireReset)
    {
        if (go == null) return;
        Animation anim = go.GetComponentInChildren<Animation>();
        if (anim == null) return;

        ActiveAnimation activeAnim = ActiveAnimation.Play(anim, animName, direction);

        if (activeAnim != null)
        {            

            if (requireReset) activeAnim.Reset();
        }
    }

}
