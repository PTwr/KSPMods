using ClickThroughFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static KSP.UI.Screens.Settings.SettingsSetup;
using static UnityEngine.GUI;

namespace PartCompressor
{
    public partial class OmNomNom : PartModule
    {
        const string CompressedSubVesselsConfigNodeName = "CompressedSubVesselsConfigNode";


        List<ConfigNode> vesselNodes = new List<ConfigNode>();

        public override void OnSave(ConfigNode node)
        {
            if (vesselNodes.Any())
            {
                var config = new ConfigNode(CompressedSubVesselsConfigNodeName);

                int n = 0;
                foreach (var vesselNode in vesselNodes)
                {
                    config.AddNode($"Compressed Vessel #{n++}", vesselNode);
                }

                node.SetNode(CompressedSubVesselsConfigNodeName, config, true);
            }
            else if (node.HasNode(CompressedSubVesselsConfigNodeName))
            {
                node.RemoveNode(CompressedSubVesselsConfigNodeName);
            }

            base.OnSave(node);
        }
        public override void OnLoad(ConfigNode node)
        {
            if (node.HasNode(CompressedSubVesselsConfigNodeName))
            {
                var config = node.GetNode(CompressedSubVesselsConfigNodeName);

                vesselNodes = config.GetNodes().ToList();
            }
            base.OnLoad(node);
        }

        const string ConfigNode_OriginalPosition = "__OriginalPosition";
        const string ConfigNode_OriginalRotation = "__OriginalRotation";
        const string ConfigNode_PartNameTag = "__NameTag";
        private void CompressPart(Part part)
        {
            using (vessel.Protect(true, "__BeforeCompress"))
            using (vessel.TemporarilyPositionAtZero())
            {
                part.decouple();

                var newVessel = part.vessel;

                var originalPosition = newVessel.transform.position;
                //originalPosition += this.vessel.transform.position; //on ground current vessel is not at [0,0,0]!!!
                var originalRotation = this.vessel.transform.rotation;

                var vesselNode = new ConfigNode("COMPRESSE_PARTS");

                vesselNode.SetValue(ConfigNode_OriginalPosition, originalPosition, true);
                vesselNode.SetValue(ConfigNode_OriginalRotation, originalRotation, true);
                vesselNode.SetValue(ConfigNode_PartNameTag, part.partInfo.title, true);

                ProtoVessel pVessel = newVessel.BackupVessel();
                pVessel.Save(vesselNode);

                vesselNodes.Add(vesselNode);

                ChangeFakeMass(newVessel.GetTotalMass());

                newVessel.Die();

                print($"vesselNode name: {vesselNode.name}");
            }
        }

        private void DecompressVesselNode(ConfigNode vesselNode)
        {
            UnityEngine.Vector3 originalPosition = UnityEngine.Vector3.zero;
            UnityEngine.Quaternion originalRotation = new UnityEngine.Quaternion();

            if (!vesselNode.TryGetValue(ConfigNode_OriginalPosition, ref originalPosition) || !vesselNode.TryGetValue(ConfigNode_OriginalRotation, ref originalRotation)
                )
            {
                print("Faulty compressed vessel config!!!");
                ScreenMessages.PostScreenMessage("Faulty compressed vessel config!!!");
                return;
            }
            using (vessel.Protect(true, "__BeforeDecompress"))
            using (vessel.TemporarilyRotate(originalRotation))
            using (vessel.TemporarilyPositionAtZero())
            {
                ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(vesselNode);
                var lastDetached = addedVessel.vesselRef;

                lastDetached.Load();
                lastDetached.GoOnRails();

                lastDetached.SetPosition(originalPosition);
                //required if orbit eccentricity changes since compression
                lastDetached.SetRotation(originalRotation);

                var detachedMass = lastDetached.GetTotalMass();

                var partsToAttach = lastDetached.Parts.ToList();

                lastDetached.rootPart.Couple(this.part);

                foreach (var part in partsToAttach)
                {
                    part.AllowAutoStruts();
                    part.autoStrutMode = Part.AutoStrutMode.Root;
                    part.UpdateAutoStrut();
                }

                ChangeFakeMass(-detachedMass);

                vesselNodes.Remove(vesselNode);

                if (!vesselNodes.Any())
                {
                    //cleanup whats left after floating point innacuracy from +/-
                    RemFakeMass();
                }
            }
        }

        bool compressionRequested = false;
        bool decompressionRequested = false;

        Rect windowRect = new Rect(Screen.width / 2, Screen.height / 2, 600, 800);
        int windowID = GUIUtility.GetControlID(FocusType.Passive);
        void OnGUI()
        {
            if (compressionRequested)
            {
                GUI.skin = HighLogic.Skin;
                windowRect = ClickThruBlocker.GUILayoutWindow(windowID, windowRect, WindowFunction_Compress, "Compress parts");
            }
            else if (decompressionRequested)
            {
                GUI.skin = HighLogic.Skin;
                windowRect = ClickThruBlocker.GUILayoutWindow(windowID, windowRect, WindowFunction_Decompress, "Decompress parts");
            }
        }

        [KSPEvent(guiActive = true, guiName = "RequestCompression", active = true, guiActiveEditor = true)]
        public void RequestCompression()
        {
            compressionRequested = true;
            decompressionRequested = false;
        }

        [KSPEvent(guiActive = true, guiName = "RequestDecompression", active = true, guiActiveEditor = true)]
        public void RequestdDecompression()
        {
            compressionRequested = false;
            decompressionRequested = true;
        }

        void WindowFunction_Decompress(int windowID)
        {
            GUI.enabled = true;
            GUILayout.BeginVertical();

            if (GUILayout.Button("Cancel"))
            {
                decompressionRequested = false;
                return;
            }

            string name = null;
            foreach (var vesselNode in vesselNodes)
            {
                if (vesselNode.TryGetValue(ConfigNode_PartNameTag, ref name))
                {
                    if (GUILayout.Button(name))
                    {
                        DecompressVesselNode(vesselNode);
                        return;
                    }
                }
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        void WindowFunction_Compress(int windowID)
        {
            GUI.enabled = true;
            GUILayout.BeginVertical();

            if (GUILayout.Button("Cancel"))
            {
                compressionRequested = false;
                return;
            }

            foreach (var childPart in this.part.children)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(childPart.partInfo.title))
                {
                    CompressPart(childPart);
                    return;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
