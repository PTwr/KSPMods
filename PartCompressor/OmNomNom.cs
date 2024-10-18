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

                print($"[OmNomNom.OnSave] Gathering info of #{vesselNodes.Count} packed part-branches");
                int n = 0;
                foreach (var vesselNode in vesselNodes)
                {
                    config.AddNode($"Compressed Vessel #{n++}", vesselNode);
                }

                print("[OmNomNom.OnSave] Saving packed part info");
                node.SetNode(CompressedSubVesselsConfigNodeName, config, true);
            }
            else if (node.HasNode(CompressedSubVesselsConfigNodeName))
            {
                print("[OmNomNom.OnSave] Cleaning up unnecessary node");
                node.RemoveNode(CompressedSubVesselsConfigNodeName);
            }

            base.OnSave(node);
        }
        public override void OnLoad(ConfigNode node)
        {
            if (node.HasNode(CompressedSubVesselsConfigNodeName))
            {
                print("[OmNomNom.OnLoad] Loading packed part info");
                var config = node.GetNode(CompressedSubVesselsConfigNodeName);

                vesselNodes = config.GetNodes().ToList();
            }
            base.OnLoad(node);
        }

        const string ConfigNode_OriginalPosition = "__OriginalPosition";
        const string ConfigNode_OriginalRotation = "__OriginalRotation";
        const string ConfigNode_OriginalPartRotation = "__OriginalPartRotation";
        const string ConfigNode_PartNameTag = "__NameTag";
        private void CompressPart(Part part)
        {
            using (vessel.Protect(true, "__BeforeCompress"))
            using (vessel.TemporarilyPositionAtZero())
            {
                var vesselNode = new ConfigNode("COMPRESSE_PARTS");

                print($"[OmNomNom.CompressPart] Storing part.orgRot of {part.orgRot}");
                vesselNode.SetValue(ConfigNode_OriginalPartRotation, part.orgRot, true);

                print($"[OmNomNom.CompressPart] Decoupling part branch");
                part.decouple();

                var newVessel = part.vessel;

                var originalPosition = newVessel.transform.position;
                var originalRotation = this.vessel.transform.rotation;

                print($"[OmNomNom.CompressPart] Storing part position of {originalPosition}");
                vesselNode.SetValue(ConfigNode_OriginalPosition, originalPosition, true);
                print($"[OmNomNom.CompressPart] Storing part rotation of {originalRotation}");
                vesselNode.SetValue(ConfigNode_OriginalRotation, originalRotation, true);
                print($"[OmNomNom.CompressPart] Storing part friendly name of {part.partInfo.title}");
                vesselNode.SetValue(ConfigNode_PartNameTag, part.partInfo.title, true);

                print($"[OmNomNom.CompressPart] BackupVessel()");
                ProtoVessel pVessel = newVessel.BackupVessel();
                pVessel.Save(vesselNode);

                vesselNodes.Add(vesselNode);

                print($"[OmNomNom.CompressPart] ChangeFakeMass()");
                ChangeFakeMass(newVessel.GetTotalMass());

                print($"[OmNomNom.CompressPart] Removing compressed parts from game");
                newVessel.Die();
            }
        }

        private void DecompressVesselNode(ConfigNode vesselNode)
        {
            UnityEngine.Vector3 originalPosition = UnityEngine.Vector3.zero;
            UnityEngine.Quaternion originalRotation = new UnityEngine.Quaternion();
            UnityEngine.Quaternion originalPartRotation = new UnityEngine.Quaternion();

            print($"[OmNomNom.DecompressVesselNode] Fetching positional data from config node");
            if (!vesselNode.TryGetValue(ConfigNode_OriginalPosition, ref originalPosition) || !vesselNode.TryGetValue(ConfigNode_OriginalRotation, ref originalRotation) || !vesselNode.TryGetValue(ConfigNode_OriginalPartRotation, ref originalPartRotation)
                )
            {
                Debug.LogError("[OmNomNom.DecompressVesselNode] Faulty compressed vessel config!");
                ScreenMessages.PostScreenMessage("Faulty compressed vessel config!!!");
                return;
            }
            using (vessel.Protect(true, "__BeforeDecompress"))
            using (vessel.TemporarilyRotate(originalRotation))
            using (vessel.TemporarilyPositionAtZero())
            {
                print("[OmNomNom.DecompressVesselNode] Recreating packed parts as a vessel");
                ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(vesselNode);
                var unpackedVessel = addedVessel.vesselRef;

                print("[OmNomNom.DecompressVesselNode] unpackedVessel.Load()");
                unpackedVessel.Load();
                print("[OmNomNom.DecompressVesselNode] unpackedVessel.GoOnRails()");
                unpackedVessel.GoOnRails();

                print("[OmNomNom.DecompressVesselNode] setting unpacked vessel position");
                unpackedVessel.SetPosition(originalPosition);
                //originalRotation is required if orbit eccentricity changes since compression
                //originalPartRotation is required if originalRotation is applied :)
                print("[OmNomNom.DecompressVesselNode] setting unpacked vessel rotation");
                unpackedVessel.SetRotation(originalRotation * originalPartRotation);

                var detachedMass = unpackedVessel.GetTotalMass();

                var partsToAttach = unpackedVessel.Parts.ToList();

                print("[OmNomNom.DecompressVesselNode] recoupling unpacked vessel to host part");
                unpackedVessel.rootPart.Couple(this.part);

                print("[OmNomNom.DecompressVesselNode] autostrutting decompressed parts");
                foreach (var part in partsToAttach)
                {
                    part.AllowAutoStruts();
                    part.autoStrutMode = Part.AutoStrutMode.Root;
                    part.UpdateAutoStrut();
                }

                print("[OmNomNom.DecompressVesselNode] removing fake mass from host part");
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

        [KSPEvent(guiActive = true, guiName = "RequestCompression", active = true, guiActiveEditor = false)]
        public void RequestCompression()
        {
            compressionRequested = true;
            decompressionRequested = false;
        }

        [KSPEvent(guiActive = true, guiName = "RequestDecompression", active = true, guiActiveEditor = false)]
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
