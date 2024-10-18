#if DEBUG
using System.Linq;

namespace PartCompressor
{
    public partial class OmNomNom
    {
        protected void ChangeFakeMass(double mass)
        {
            print("[OmNomNom.ChangeFakeMass] Searching for existing __CompressedPartsMassEquivalent");
            var res = this.part.Resources.Get("__CompressedPartsMassEquivalent");

            if (res is null)
            {
                print("[OmNomNom.ChangeFakeMass] Creating resource for __CompressedPartsMassEquivalent");
                print($"[OmNomNom.ChangeFakeMass] Setting fake mass to: {mass}");

                this.part.Resources.Add("__CompressedPartsMassEquivalent", mass, mass, false, false, true, true, PartResource.FlowMode.None);
            }
            else
            {
                print("[OmNomNom.ChangeFakeMass] Reusing existing resource for __CompressedPartsMassEquivalent");
                print($"[OmNomNom.ChangeFakeMass] Changing __CompressedPartsMassEquivalent mass from {res.amount} by {mass} to: {res.amount + mass}");
                mass+=res.amount;
                this.part.RemoveResource("__CompressedPartsMassEquivalent");
                res = this.part.Resources.Add("__CompressedPartsMassEquivalent", mass, mass, false, false, true, true, PartResource.FlowMode.None);
                print($"[OmNomNom.ChangeFakeMass] Updated __CompressedPartsMassEquivalent amount: {res.amount}");
            }
        }
        protected void RemFakeMass()
        {
            print("[OmNomNom.ChangeFakeMass] Removing __CompressedPartsMassEquivalent");
            this.part.RemoveResource("__CompressedPartsMassEquivalent");
        }
    }
}
#endif