using UnityEngine;

namespace PartCompressor
{
    public static class ExtensionMethods
    {
        public static VesselProtector Protect(this Vessel vessel, bool autosave = true, string autosaveName = "__autosave")
        {
            return new VesselProtector(vessel, autosave, autosaveName);
        }
        public static VesselTemporaryRotation TemporarilyRotate(this Vessel vessel, Quaternion rotation)
        {
            return new VesselTemporaryRotation(vessel, rotation);
        }
    }
}
