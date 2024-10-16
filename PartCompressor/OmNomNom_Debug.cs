using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PartCompressor
{
    public partial class OmNomNom_Debug : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Vector3 originalPosition;
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Quaternion originalRotation;
        [KSPField(isPersistant = true, guiActive = true)]
        UnityEngine.Quaternion originalPartRotation;

        private Vessel _DEBUG_lastDetached;
        private ConfigNode _DEBUG_vesselNode;

        [KSPEvent(guiActive = true, guiName = "Pause", active = true, guiActiveEditor = true)]
        public void Pause()
        {
            ScreenMessages.PostScreenMessage("Pause!!!");
            FlightDriver.SetPause(true, true);
        }
        [KSPEvent(guiActive = true, guiName = "Resume", active = true, guiActiveEditor = true)]
        public void Resume()
        {
            ScreenMessages.PostScreenMessage("Resume!!!");
            FlightDriver.SetPause(false, true);
        }

        [KSPEvent(guiActive = true, guiName = "AddFakeMass", active = true, guiActiveEditor = true)]
        public void AddFakeMass()
        {
            var res = this.part.Resources.Get("__CompressedPartsMassEquivalent");

            if (res is null)
            {

                //this.part.partInfo =  new AvailablePart(this.part.partInfo);
                var resourceConfig = new ConfigNode("RESOURCE");

                resourceConfig.AddValue("name", "__CompressedPartsMassEquivalent");
                resourceConfig.AddValue("amount", 1000);
                resourceConfig.AddValue("isVisible", true);

                res = this.part.AddResource(resourceConfig);
            }
            else
            {
                res.amount += 1000;
            }

        }
        [KSPEvent(guiActive = true, guiName = "VesselPosRot", active = true, guiActiveEditor = true)]
        public void VesselPosRot()
        {

            print("---------------------------");
            print(this.vessel.transform.position);
            print(this.vessel.transform.rotation);
            print("---------------------------");
        }

        [KSPEvent(guiActive = true, guiName = "PosRotInfo", active = true, guiActiveEditor = true)]
        public void PosRotInfo()
        {
            print("---------------------------");
            print(this.part.attPos);
            print(this.part.attPos0);
            print(this.part.attRotation);
            print(this.part.attRotation0);
            print(this.part.initRotation);
            print(this.part.orgPos);
            print(this.part.orgRot);
            print(this.part.transform.rotation);
            print("---------------------------");
        }

        [KSPEvent(guiActive = true, guiName = "Detach child", active = true, guiActiveEditor = true)]
        public void DetachChild()
        {
            print(1);
            var firstChild = this.part.children.FirstOrDefault();

            //print(2);
            //originalPosition = firstChild.orgPos;
            //originalRotation = firstChild.orgRot;

            print(3);
            firstChild.decouple();

            print(4);
            var newVessel = _DEBUG_lastDetached = firstChild.vessel;

            print(4);
            print(this.vessel.vesselName);
            print(newVessel.vesselName);

            print(5);
            originalPosition = _DEBUG_lastDetached.transform.position;
            originalRotation = _DEBUG_lastDetached.transform.rotation;
        }

        [KSPEvent(guiActive = true, guiName = "Pack child", active = true, guiActiveEditor = true)]
        public void PackChild()
        {
            print(1);
            _DEBUG_vesselNode = new ConfigNode("VESSEL");
            print(2);
            ProtoVessel pVessel = _DEBUG_lastDetached.BackupVessel();
            print(3);
            print(pVessel);
            pVessel.Save(_DEBUG_vesselNode);

            //var node = this.vesselNode.node("omnomnom", vesselNode);
            //print(node);

            print(4);
            _DEBUG_lastDetached.Die();

            print(5);
            _DEBUG_lastDetached = null;

            //vesselNode = null;
        }

        [KSPEvent(guiActive = true, guiName = "UnPack child", active = true, guiActiveEditor = true)]
        public void UnPackChild()
        {

            print("unpack attempt");
            ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(_DEBUG_vesselNode);

            //ProtoVessel addedVessel = new ProtoVessel(vesselNode, HighLogic.CurrentGame);

            _DEBUG_lastDetached = addedVessel.vesselRef;

            _DEBUG_lastDetached.Load();

            print(addedVessel);
            print(addedVessel.vesselRef);
            print(addedVessel.vesselRef.rootPart);

           
            print(addedVessel.rootIndex);
            print(_DEBUG_lastDetached.Parts.Count);
            print(_DEBUG_lastDetached.parts.Count);

            print(_DEBUG_lastDetached.Parts[addedVessel.rootIndex].partInfo.title);

            print(addedVessel.vesselRef.rootPart.partInfo.title);
        }

        [KSPEvent(guiActive = true, guiName = "Reatach child", active = true, guiActiveEditor = true)]
        public void Devour4()
        {

            print(_DEBUG_lastDetached.rootPart.partInfo.title);

            //TODO learn Quaternion math :D

            //place vessel into orientiation it was in during decoupling
            var currentRotation = this.vessel.transform.rotation;
            this.vessel.SetRotation(originalRotation);

            //place child in pre-decoupling orientation and position
            _DEBUG_lastDetached.SetPosition(originalPosition);
            //lastDetached.SetRotation(originalRotation);


            _DEBUG_lastDetached.rootPart.Couple(this.part);

            //restore current rotation :)
            //this.vessel.SetRotation(currentRotation);
        }
    }
}