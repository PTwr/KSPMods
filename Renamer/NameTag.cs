using SaveUpgradePipeline;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kerbal;
using static UnityEngine.GUI;
using ClickThroughFix;

namespace Renamer
{
    public class NameTag : PartModule
    {
        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = true, guiName = "NameTag")]
        public string _nameTag = "";

        public NameTag()
        {
        }

        public override void OnStart(StartState state)
        {
            this.part.partInfo = new AvailablePart(this.part.partInfo);
            if (!string.IsNullOrWhiteSpace(_nameTag))
            {
                this.part.partInfo.title += "\n" + _nameTag;
            }
        }

        string tempName;
        [KSPEvent(guiActive = true, guiName = "Rename", active = true, guiActiveEditor = true)]
        public void Rename()
        {
            if (renameRequested) return;

            print(_nameTag);
            tempName = string.IsNullOrWhiteSpace(_nameTag) ? this.part.partInfo.title : _nameTag;
            renameRequested = true;
        }

        bool renameRequested = false;
        Rect renameWindowRect = new Rect(Screen.width / 2, Screen.height / 2, 600, 50);
        int windowID = GUIUtility.GetControlID(FocusType.Passive);

        void OnGUI()
        {
            if (renameRequested)
            {
                GUI.skin = HighLogic.Skin;
                renameWindowRect = ClickThruBlocker.GUILayoutWindow(windowID, renameWindowRect, WindowFunction, "Rename");
            }
        }        

        void WindowFunction(int windowID)
        {
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            tempName = GUILayout.TextField(tempName);

            if (GUILayout.Button("Yep"))
            {
                renameRequested = false;
                this.part.partInfo = new AvailablePart(this.part.partInfo);
                this.part.partInfo.title = _nameTag = tempName;
            }
            if (GUILayout.Button("Nah"))
            {
                renameRequested = false;
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true;
            GUI.DragWindow();
        }
    }
}
