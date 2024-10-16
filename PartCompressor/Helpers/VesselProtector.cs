using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartCompressor
{
    public class VesselProtector : IDisposable
    {
        private readonly Vessel vessel;

        public VesselProtector(Vessel vessel, bool autosave = true, string autosaveName = "__autosave")
        {
            if (autosave)
            {
                GamePersistence.SaveGame(HighLogic.CurrentGame,
                    autosaveName,
                    HighLogic.SaveFolder,
                    SaveMode.OVERWRITE);
            }

            this.vessel = vessel;

            ScreenMessages.PostScreenMessage("Protecting vessel from physics!!!");
            FlightDriver.SetPause(true, true);

            vessel.GoOnRails();
        }

        public void Dispose()
        {
            vessel.GoOffRails();

            ScreenMessages.PostScreenMessage("Disabling vessel protection!!!");
            FlightDriver.SetPause(false, true);
        }
    }
}
