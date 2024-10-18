using System;
using UnityEngine;

namespace PartCompressor
{
    public class VesselTemporaryRotation : IDisposable
    {
        private readonly Vessel vessel;
        private readonly Quaternion originalRotation;

        public VesselTemporaryRotation(Vessel vessel, Quaternion rotation)
        {
            this.vessel = vessel;

            Debug.Log("[VesselTemporaryRotation] Storing original rotation!!!");
            originalRotation = vessel.transform.rotation;
            Debug.Log("[VesselTemporaryRotation] Setting temporary rotation!!!");
            vessel.SetRotation(rotation);
        }
        public void Dispose()
        {
            Debug.Log("[VesselTemporaryRotation] Restoring original rotation!!!");
            vessel.SetRotation(originalRotation);
        }
    }
}
