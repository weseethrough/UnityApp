using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GameSelectPanel : HexPanel 
{

	public GameSelectPanel() {}
    public GameSelectPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    public override void EnterStart()
    {
        GConnector activityExit = Outputs.Find(r => r.Name == "ActivityExit");
        GConnector unlockExit = Outputs.Find(r => r.Name == "UnlockExit");

        GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;

        //generate some buttons
        for(int i=0; i<10; i++)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.buttonName = "Button"+i;
            hbd.column = 1+ (int)(i / 5);
            hbd.row = -2 + (i % 5);
            hbd.imageName = "activity_run";
            hbd.locked = UnityEngine.Random.Range(0.0f,1.0f) > 0.5f ? true : false;

            buttonData.Add(hbd);

            GConnector gc = NewOutput(hbd.buttonName, "Flow");
            gc.EventFunction = "GoToCustomExit";
            /*if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? true : false)
            {                
                if (activityExit.Link.Count > 0)
                {
                    gComponent.Data.Connect(gc, activityExit.Link[0]); 
                }
            }
            else if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? true : false)
            {                
                if (unlockExit.Link.Count > 0)
                {
                    gComponent.Data.Connect(gc, unlockExit.Link[0]); 
                }
            }
            else if (gComponent != null)*/
            {
                foreach (GNode node in gComponent.Data.Nodes)
                {
                    GParameter gName = node.Parameters.Find(r => r.Key == "Name");
                    if (gName.Value == "TargetScreen" && node.Inputs.Count > 0)
                    {
                        GConnector enter = node.Inputs[0];
                        gComponent.Data.Connect(gc, enter);                        
                    }
                }
                
            }
            
          //  gc.Link.Add()

        }

        //addConnections

        

        base.EnterStart();   
    }
}
