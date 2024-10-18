//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.PlayerLoop;

//namespace PartCompressor
//{
//    public partial class OmNomNom_Debug : PartModule
//    {
//        UnityEngine.Vector3 originalPosition;
//        UnityEngine.Quaternion originalRotation;
//        UnityEngine.Quaternion originalPartRotation;

//        private Vessel _DEBUG_lastDetached;
//        private ConfigNode _DEBUG_vesselNode;

//        [KSPEvent(guiActive = true, guiName = "VesselPosRot", active = true, guiActiveEditor = true)]
//        public void VesselPosRot()
//        {

//            print("---------------------------");
//            print(this.vessel.transform.position);
//            print(this.vessel.transform.rotation);
//            print("---------------------------");
//        }

//        [KSPEvent(guiActive = true, guiName = "PosRotInfo", active = true, guiActiveEditor = true)]
//        public void PosRotInfo()
//        {
//            print("---------------------------");
//            print(this.part.attPos);
//            print(this.part.attPos0);
//            print(this.part.attRotation);
//            print(this.part.attRotation0);
//            print(this.part.initRotation);
//            print(this.part.orgPos);
//            print(this.part.orgRot);
//            print(this.part.transform.rotation);
//            print("---------------------------");
//        }

//        [KSPEvent(guiActive = true, guiName = "Detach child", active = true, guiActiveEditor = true)]
//        public void DetachChild()
//        {
//            var firstChild = this.part.children.FirstOrDefault();

//            firstChild.decouple();

//            var newVessel = _DEBUG_lastDetached = firstChild.vessel;

//            print(this.vessel.vesselName);
//            print(newVessel.vesselName);

//            originalPosition = _DEBUG_lastDetached.transform.position;
//            originalRotation = _DEBUG_lastDetached.transform.rotation;
//        }

//        [KSPEvent(guiActive = true, guiName = "Pack child", active = true, guiActiveEditor = true)]
//        public void PackChild()
//        {
//            _DEBUG_vesselNode = new ConfigNode("VESSEL");
//            ProtoVessel pVessel = _DEBUG_lastDetached.BackupVessel();
//            pVessel.Save(_DEBUG_vesselNode);

//            _DEBUG_lastDetached.Die();

//            _DEBUG_lastDetached = null;
//        }

//        [KSPEvent(guiActive = true, guiName = "UnPack child", active = true, guiActiveEditor = true)]
//        public void UnPackChild()
//        {

//            print("unpack attempt");
//            ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(_DEBUG_vesselNode);

//            _DEBUG_lastDetached = addedVessel.vesselRef;

//            _DEBUG_lastDetached.Load();

//            print(addedVessel);
//            print(addedVessel.vesselRef);
//            print(addedVessel.vesselRef.rootPart);

           
//            print(addedVessel.rootIndex);
//            print(_DEBUG_lastDetached.Parts.Count);
//            print(_DEBUG_lastDetached.parts.Count);

//            print(_DEBUG_lastDetached.Parts[addedVessel.rootIndex].partInfo.title);

//            print(addedVessel.vesselRef.rootPart.partInfo.title);
//        }

//        [KSPEvent(guiActive = true, guiName = "Reatach child", active = true, guiActiveEditor = true)]
//        public void Devour4()
//        {
//            print(_DEBUG_lastDetached.rootPart.partInfo.title);

//            //TODO learn Quaternion math :D

//            //place vessel into orientiation it was in during decoupling
//            var currentRotation = this.vessel.transform.rotation;
//            this.vessel.SetRotation(originalRotation);

//            //place child in pre-decoupling orientation and position
//            _DEBUG_lastDetached.SetPosition(originalPosition);
//            //lastDetached.SetRotation(originalRotation);


//            _DEBUG_lastDetached.rootPart.Couple(this.part);

//            //restore current rotation :)
//            this.vessel.SetRotation(currentRotation);
//        }
//    }
//}