using UnityEngine;
using System.Collections;

public class PlayerPositionController : PositionController {
    // Update is called once per frame
    public override void Update () {
        //only update positions if we're currently tracking
        if(!Platform.Instance.LocalPlayerPosition.IsTracking)
        { return; }
        if(worldObject == null)
        { return; }

        worldObject.setRealWorldDist((float) Platform.Instance.LocalPlayerPosition.Distance);

        // TODO: map latitude/longitude/altitude to world coordinate space.
        //worldObject.setRealWorldPos(Platform.Instance.LocalPlayerPosition.Position);

        worldObject.setRealWorldSpeed(Platform.Instance.LocalPlayerPosition.Pace);
        
        base.Update();
    }
}
