using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_EntityDatabaseAnomaly : CompProperties
    {
        public FloatRange incidentActiveDelayHoursRange;
        public SoundDef soundActivate;
        public FleckDef fleckOnUsed;
        public float fleckOnUsedScale;
        public List<EntityCodexEntryDef> entityCodexEntryDefsBlacklist = new List<EntityCodexEntryDef>();

        public CompProperties_EntityDatabaseAnomaly()
        {
            compClass = typeof(Comp_EntityDatabaseAnomaly);
        }
    }
}
