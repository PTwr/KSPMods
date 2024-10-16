using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PartCompressor
{
    public partial class OmNomNom : PartModule
    {
        const string CompressedSubVesselsConfigNodeName = "CompressedSubVesselsConfigNode";

        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Vector3 originalPosition;
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Quaternion originalRotation;
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Quaternion originalPartRotation;

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

        [KSPEvent(guiActive = true, guiName = "Compress2 first child", active = true, guiActiveEditor = true)]
        public void Compress2()
        {
            using (vessel.Protect(true, "__BeforeCompress"))
            {
                var firstChild = this.part.children.FirstOrDefault();

                if (firstChild is null)
                {
                    print("No child!");
                    vessel.GoOffRails();
                    return;
                }

                firstChild.decouple();

                var newVessel = firstChild.vessel;

                originalPosition = newVessel.transform.position;
                originalRotation = this.vessel.transform.rotation;

                var vesselNode = new ConfigNode("VESSEL");
                ProtoVessel pVessel = newVessel.BackupVessel();
                pVessel.Save(vesselNode);

                vesselNodes.Add(vesselNode);

                ChangeFakeMass(newVessel.totalMass);

                newVessel.Die();
            }
        }

        [KSPEvent(guiActive = true, guiName = "Compress first child", active = true, guiActiveEditor = true)]
        public void Compress()
        {
            FlightDriver.SetPause(true);

            GamePersistence.SaveGame(HighLogic.CurrentGame,
                "__BeforeCompress",
                HighLogic.SaveFolder,
                SaveMode.OVERWRITE);

            print("Putting vesson On Rails");
            vessel.GoOnRails();

            var firstChild = this.part.children.FirstOrDefault();

            if (firstChild is null)
            {
                print("No child!");
                vessel.GoOffRails();
                return;
            }    

            originalPartRotation = firstChild.orgRot;

            print("Vessel mass before decoupling");
            print(this.vessel.totalMass);

            print("Decoupling");
            firstChild.decouple();

            var newVessel = firstChild.vessel;
            print(newVessel.vesselName);

            originalPosition = newVessel.transform.position;
            originalRotation = this.vessel.transform.rotation;

            //originalPartRotation

            ///////////////////////

            var vesselNode = new ConfigNode("VESSEL");
            ProtoVessel pVessel = newVessel.BackupVessel();
            pVessel.Save(vesselNode);

            print("Vessel mass after decoupling");
            print(this.vessel.totalMass);
            print(this.vessel.GetTotalMass());
            print(this.vessel.RevealMass());
            print(this.vessel.totalMass);

            print("setting fake mass equivalent to");
            print(newVessel.totalMass);
            print(newVessel.GetTotalMass());
            print(newVessel.RevealMass());
            print(newVessel.totalMass);

            ChangeFakeMass(newVessel.totalMass);

            print("removing packed parts");
            newVessel.Die();

            print("Vessel mass after FakeMass");
            print(this.vessel.totalMass);
            vessel.GoOffRails();

            FlightDriver.SetPause(false);
        }

        [KSPEvent(guiActive = true, guiName = "Decompress2 first child", active = true, guiActiveEditor = true)]
        public void Decompress2()
        {
            using (vessel.Protect(true, "__BeforeDecompress"))
            using (vessel.TemporarilyRotate(originalRotation))
            {
                if (!vesselNodes.Any()) return;
                var vesselNode = vesselNodes.First();


                ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(vesselNode);
                var lastDetached = addedVessel.vesselRef;

                lastDetached.Load();
                lastDetached.GoOnRails();

                lastDetached.SetPosition(originalPosition);


                var detachedMass = lastDetached.totalMass;

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
        [KSPEvent(guiActive = true, guiName = "Decompress first child", active = true, guiActiveEditor = true)]
        public void Decompress()
        {
            FlightDriver.SetPause(true);

            GamePersistence.SaveGame(HighLogic.CurrentGame,
                "__BeforeDecompress",
                HighLogic.SaveFolder,
                SaveMode.OVERWRITE);

            if (!vesselNodes.Any()) return;

            var vesselNode = vesselNodes.First();

            vessel.GoOnRails();

            //place vessel into orientiation it was in during decoupling
            print("aaa");
            var currentRotation = this.vessel.transform.rotation;
            this.vessel.SetRotation(originalRotation);

            print("aaa");
            ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(vesselNode);
            var lastDetached = addedVessel.vesselRef;
            print("aaa");

            lastDetached.Load();
            lastDetached.GoOnRails();

            ///////////////////////

            //TODO learn Quaternion math :D

            //place child in pre-decoupling orientation and position
            lastDetached.SetPosition(originalPosition);
            //lastDetached.SetRotation(originalPartRotation);
            //lastDetached.SetRotation(originalRotation);
            //originalRotation.Inverse();
            print("aaa");

            print(lastDetached);
            print(lastDetached.rootPart);
            print(lastDetached.rootPart.partInfo.title);

            var detachedMass = lastDetached.totalMass;

            var partsToAttach = lastDetached.Parts.ToList();

            lastDetached.rootPart.Couple(this.part);

            //restore current rotation :)
            this.vessel.SetRotation(currentRotation);
            print("aaa");

            vesselNode = null;

            foreach (var part in partsToAttach)
            {
                part.AllowAutoStruts();
                part.autoStrutMode = Part.AutoStrutMode.Root;
                part.UpdateAutoStrut();
            }

            ChangeFakeMass(-detachedMass);

            vessel.GoOffRails();

            vesselNodes.Remove(vesselNode);

            if (!vesselNodes.Any())
            {
                //cleanup whats left after floating point innacuracy from +/-
                RemFakeMass();
            }

            FlightDriver.SetPause(false);
        }
    }
}
