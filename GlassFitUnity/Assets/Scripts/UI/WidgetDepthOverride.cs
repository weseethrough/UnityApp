using System;
using UnityEngine;
using RaceYourself.Models;

namespace RaceYourself
{
    /// <summary>
    /// All Widget Containers are cloned on Panel instantiation with the same depth. Widget Containers each have a UIPanel component,
    /// which have a depth. This being the top-level component, this depth value is considered ahead of the depth of all elements of a
    /// Panel. When multiple Widget Containers are cloned with the same depth, NGUI prioritises based on instance IDs instead, with
    /// unpredictable results (see UIPanel.CompareFunc()).
    /// 
    /// When multiple Panels are layered on top of one another, associate a WidgetDepthOverride with an arbitrary GameObject in each
    /// Panel, assigning an appropriate depth to each.
    /// </summary>
    public class WidgetDepthOverride : MonoBehaviour
    {
        private Log log = new Log("WidgetDepthOverride");

        public int depth;

        // FIXME works fine for challenge/invite sent, but for inappbackbutton, depth doesn't appear to be updated in editor...
        // even though in code it looks fine! What's happening?!
        public void Start()
        {
            FlowState flowState = FlowStateMachine.GetCurrentFlowState();

            if (flowState is Panel)
            {
                Panel panel = (Panel) flowState;
                UIPanel uiPanel = panel.physicalWidgetRoot.GetComponent<UIPanel>();
                uiPanel.depth = depth;
            }
            else
            {
                log.error("WidgetDepthOverride attached to a FlowState that isn't a Panel.");
            }
        }
    }
}
