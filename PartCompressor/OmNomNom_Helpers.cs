#if DEBUG
using System.Linq;

namespace PartCompressor
{
    public partial class OmNomNom
    {
        protected void ChangeFakeMass(double mass)
        {

            var res = this.part.Resources.Get("__CompressedPartsMassEquivalent");

            if (res is null)
            {
                print("Creating resource for FakeMass");
                print($"Setting fake mass to: {mass}");
                print(-1);

                var resourcDef = PartResourceLibrary.Instance.GetDefinition("__CompressedPartsMassEquivalent");
                var resourcDef2 = PartResourceLibrary.Instance.GetDefinition("__CompressedPartsMassEquivalent");
                print(resourcDef == resourcDef2);

                this.part.Resources.Add("__CompressedPartsMassEquivalent", mass, mass, false, false, true, true, PartResource.FlowMode.None);
                print(4);

            }
            else
            {
                print("$Reusing existing resource for FakeMass");
                print($"Changing fake mass from {res.amount} by {mass} to: {res.amount + mass}");
                mass+=res.amount;
                this.part.RemoveResource("__CompressedPartsMassEquivalent");
                res = this.part.Resources.Add("__CompressedPartsMassEquivalent", mass, mass, false, false, true, true, PartResource.FlowMode.None);
                print($"Updated resource amount: {res.amount}");
            }
        }
        protected void RemFakeMass()
        {
            this.part.RemoveResource("__CompressedPartsMassEquivalent");
        }
    }
}
#endif