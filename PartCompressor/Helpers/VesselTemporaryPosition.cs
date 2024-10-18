using System;
using UnityEngine;

namespace PartCompressor
{
    public class VesselTemporaryPosition : IDisposable
    {
        private readonly Vessel vessel;
        private readonly Vector3 originalPosition;

        public VesselTemporaryPosition(Vessel vessel, Vector3 position)
        {
            this.vessel = vessel;

            Debug.Log("[VesselTemporaryPosition] Storing original position!!!");
            originalPosition = vessel.transform.position;
            Debug.Log("[VesselTemporaryPosition] Setting temporary position!!!");
            vessel.SetPosition(position);
        }
        public void Dispose()
        {
            Debug.Log("[VesselTemporaryPosition] Restoring original position!!!");
            vessel.SetPosition(originalPosition);
        }
    }
}
