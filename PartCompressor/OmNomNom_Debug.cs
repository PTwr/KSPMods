#if DEBUG
using System.Linq;

namespace PartCompressor
{
    public partial class OmNomNom
    {
        private Vessel lastDetached;

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
            var newVessel = lastDetached = firstChild.vessel;

            print(4);
            print(this.vessel.vesselName);
            print(newVessel.vesselName);

            print(5);
            originalPosition = lastDetached.transform.position;
            originalRotation = lastDetached.transform.rotation;
        }

        [KSPEvent(guiActive = true, guiName = "Pack child", active = true, guiActiveEditor = true)]
        public void PackChild()
        {
            print(1);
            vesselNode = new ConfigNode("VESSEL");
            print(2);
            ProtoVessel pVessel = lastDetached.BackupVessel();
            print(3);
            print(pVessel);
            pVessel.Save(vesselNode);

            //var node = this.vesselNode.node("omnomnom", vesselNode);
            //print(node);

            print(4);
            lastDetached.Die();

            print(5);
            lastDetached = null;

            //vesselNode = null;
        }

        [KSPEvent(guiActive = true, guiName = "UnPack child", active = true, guiActiveEditor = true)]
        public void UnPackChild()
        {

            print("unpack attempt");
            ProtoVessel addedVessel = HighLogic.CurrentGame.AddVessel(vesselNode);

            //ProtoVessel addedVessel = new ProtoVessel(vesselNode, HighLogic.CurrentGame);

            lastDetached = addedVessel.vesselRef;

            lastDetached.Load();

            print(addedVessel);
            print(addedVessel.vesselRef);
            print(addedVessel.vesselRef.rootPart);

           
            print(addedVessel.rootIndex);
            print(lastDetached.Parts.Count);
            print(lastDetached.parts.Count);

            print(lastDetached.Parts[addedVessel.rootIndex].partInfo.title);

            print(addedVessel.vesselRef.rootPart.partInfo.title);
        }

        [KSPEvent(guiActive = true, guiName = "Reatach child", active = true, guiActiveEditor = true)]
        public void Devour4()
        {

            print(lastDetached.rootPart.partInfo.title);

            //TODO learn Quaternion math :D

            //place vessel into orientiation it was in during decoupling
            var currentRotation = this.vessel.transform.rotation;
            this.vessel.SetRotation(originalRotation);

            //place child in pre-decoupling orientation and position
            lastDetached.SetPosition(originalPosition);
            //lastDetached.SetRotation(originalRotation);


            lastDetached.rootPart.Couple(this.part);

            //restore current rotation :)
            //this.vessel.SetRotation(currentRotation);
        }
    }
}
#endif