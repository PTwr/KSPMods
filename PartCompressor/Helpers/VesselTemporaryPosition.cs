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

            originalPosition = vessel.transform.position;
            vessel.SetPosition(position);
        }
        public void Dispose()
        {
            vessel.SetPosition(originalPosition);
        }
    }
}
