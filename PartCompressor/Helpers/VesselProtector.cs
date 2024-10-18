using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PartCompressor
{
    public class VesselProtector : IDisposable
    {
        private readonly Vessel vessel;

        public VesselProtector(Vessel vessel, bool autosave = true, string autosaveName = "__autosave")
        {
            Debug.Log("[VesselProtector] Protecting vessel from physics!!!");
            ScreenMessages.PostScreenMessage("Protecting vessel from physics!!!");
            FlightDriver.SetPause(true, true);

            if (autosave)
            {
                Debug.Log("[VesselProtector] autosaving!!!");
                GamePersistence.SaveGame(HighLogic.CurrentGame,
                    autosaveName,
                    HighLogic.SaveFolder,
                    SaveMode.OVERWRITE);
            }

            this.vessel = vessel;

            Debug.Log("[VesselProtector] GoOnRails!!!");
            vessel.GoOnRails();
        }

        public void Dispose()
        {
            Debug.Log("[VesselProtector] GoOffRails!!!");
            vessel.GoOffRails();

            Debug.Log("[VesselProtector] Disabling vessel protection!!!");
            ScreenMessages.PostScreenMessage("Disabling vessel protection!!!");
            FlightDriver.SetPause(false, true);
        }
    }
}
