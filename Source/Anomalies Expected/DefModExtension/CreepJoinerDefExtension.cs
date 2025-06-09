using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CreepJoinerDefExtension : DefModExtension
    {
        public IncidentDef incidentDef;
        public FactionDef factionDef;
        public bool canKidnap;
        public bool canTimeoutOrFlee;
        public SoundDef soundDef;
        public int ticksBeforeIncident = 2500;
        public MentalStateDef mentalStateDef;
    }
}
