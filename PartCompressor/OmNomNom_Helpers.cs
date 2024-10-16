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
                var resourceConfig = new ConfigNode("RESOURCE");

                resourceConfig.AddValue("name", "__CompressedPartsMassEquivalent");
                resourceConfig.AddValue("amount", mass);
                resourceConfig.AddValue("isVisible", true);

                res = this.part.AddResource(resourceConfig);
            }
            else
            {
                print("Reusing existing resource for FakeMass");
                res.amount = +mass;
            }
        }
        protected void RemFakeMass()
        {
            this.part.RemoveResource("__CompressedPartsMassEquivalent");
        }
    }
}
#endif