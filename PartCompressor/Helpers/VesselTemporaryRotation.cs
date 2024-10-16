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

            originalRotation = vessel.transform.rotation;
            vessel.SetRotation(rotation);
        }
        public void Dispose()
        {
            vessel.SetRotation(originalRotation);
        }
    }
}
