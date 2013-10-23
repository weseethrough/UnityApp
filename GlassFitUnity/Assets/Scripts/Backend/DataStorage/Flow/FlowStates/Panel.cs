using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Panel : FlowState 
{
    static public string[] InteractivePrefabs = { "UIComponents/Button" };

    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "Panel: " + gName.Value;
        }
        return "Panel: UnInitialzied";
    }

    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(250, 80);
        NewInput("Enter", "Flow");
       // NewOutput("Exit", "Flow");
        NewParameter("Type", GraphValueType.UIPrefab, ""); 
        NewParameter("Name", GraphValueType.String, "Set Panel Title");        
    }

    public override void RebuildConnections()
    {
        base.RebuildConnections();

        //Inputs.Clear();
        if (Outputs != null) Outputs.Clear();        

        GParameter gType = Parameters.Find(r => r.Key == "Type");

        SerializedNode node = GetUIPanelNames(gType.Value);
        LookForInteractiveItems(node);

        int count = Mathf.Max(Inputs.Count, Outputs.Count);

        Size.y = Mathf.Max(count * 25, 80) ;
        
    }

    private void LookForInteractiveItems(SerializedNode node)
    {
        if (node == null) return;

        foreach (string s in InteractivePrefabs)
        {
            if (node.GetPrefabName() == s)
            {
                SerializableSettings ss = node.GetSerializableSettings();
                SingleComponent sc = ss != null ? ss.GetComponent("UIBasiclabel") : null;
                StringStorageDictionary ssd = sc != null ? sc.strData : null;

                string value = ssd.Get("label");

                NewOutput(value,"Flow");
            }
        }

        for (int i = 0; i < node.subBranches.Count; i++)
        {
            LookForInteractiveItems(node.subBranches[i]);
        }
    }

    public SerializedNode GetUIPanelNames(string selectedName)
    {
        Storage s = DataStorage.GetStorage(DataStorage.BlobNames.core);
        if (s == null || s.dictionary == null)
        {           
            return null;
        }

        StorageDictionary screens = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);
        return (SerializedNode)screens.Get(selectedName);
    }
}
