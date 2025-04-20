using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class AEEntityIncidents
    {
        public EntityCodexEntryDef entityCodexEntryDef;
        public List<IncidentDef> incidentDefs = new List<IncidentDef>();

        public AEEntityIncidents(EntityCodexEntryDef entityCodexEntryDef, List<IncidentDef> incidentDefs = null)
        {
            this.entityCodexEntryDef = entityCodexEntryDef;
            if (incidentDefs != null)
            {
                this.incidentDefs = incidentDefs;
            }
            else
            {
                incidentDefs = new List<IncidentDef>();
            }
        }
    }
}