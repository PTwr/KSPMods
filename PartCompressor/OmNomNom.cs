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
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Vector3 originalPosition;
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Quaternion originalRotation;
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Quaternion originalPartRotation;
        //[KSPField(isPersistant = true)]
        ConfigNode vesselNode;dd

        Vessel lastDetached;

        public override void OnSave(ConfigNode node)
        {
            if (vesselNode != null)
                node.SetNode("omnomnom", vesselNode, true);
            else if (node.HasNode("omnomnom"))
                node.RemoveNode("omnomnom");
            base.OnSave(node);
        }
        public override void OnLoad(ConfigNode node)
        {
            if (node.HasNode("omnomnom"))
                vesselNode = node.GetNode("omnomnom");
            base.OnLoad(node);
        }

        [KSPEvent(guiActive = true, guiName = "Compress first child V2", active = true, guiActiveEditor = true)]
        public void CompressV2()
        {
            var firstChild = this.part.children.FirstOrDefault();
        }

        [KSPEvent(guiActive = true, guiName = "Compress first child", active = true, guiActiveEditor = true)]
        public void Compress()
        {
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

            vesselNode = new ConfigNode("VESSEL");
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

            SetFakeMass(newVessel.totalMass);

            print("removing packed parts");
            newVessel.Die();

            print("Vessel mass after FakeMass");
            print(this.vessel.totalMass);
            vessel.GoOffRails();
        }

        [KSPEvent(guiActive = true, guiName = "Decompress first child", active = true, guiActiveEditor = true)]
        public void Decompress()
        {
            GamePersistence.SaveGame(HighLogic.CurrentGame,
                "__BeforeDecompress",
                HighLogic.SaveFolder,
                SaveMode.OVERWRITE);

            if (vesselNode == null) return;

            vessel.GoOnRails();

            //place vessel into orientiation it was in during decoupling
            print("aaa");
            var currentRotation = this.vessel.transform.rotation;
            this.vessel.SetRotation(originalRotation);

            print("aaa");
            ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(vesselNode);
            lastDetached = addedVessel.vesselRef;
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

            RemFakeMass();

            vessel.GoOffRails();
        }
    }
}
