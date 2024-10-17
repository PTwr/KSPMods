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
        public static VesselTemporaryPosition TemporarilyPosition(this Vessel vessel, Vector3 position)
        {
            return new VesselTemporaryPosition(vessel, position);
        }
        public static VesselTemporaryPosition TemporarilyPositionAtZero(this Vessel vessel)
        {
            return new VesselTemporaryPosition(vessel, Vector3.zero);
        }
    }
}
