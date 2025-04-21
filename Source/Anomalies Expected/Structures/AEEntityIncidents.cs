using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class AEEntityIncidents
    {
        public EntityCodexEntryDef entityCodexEntryDef;
        public List<IncidentDef> incidentDefs = new List<IncidentDef>();
        public bool isCanFireNow;
        public bool isFiredTooRecently;
        public bool isCannotBeProvoked => entityCodexEntryDef.provocationIncidents.NullOrEmpty();
        private int lastUpdateTick;

        public AEEntityIncidents(EntityCodexEntryDef entityCodexEntryDef, Map map = null)
        {
            this.entityCodexEntryDef = entityCodexEntryDef;
            if (map != null)
            {
                UpToDate(map);
            }
        }

        public void UpToDate(Map map)
        {
            if (lastUpdateTick == Find.TickManager.TicksGame)
            {
                return;
            }
            if (isCannotBeProvoked)
            {
                return;
            }
            isFiredTooRecently = true;
            incidentDefs = new List<IncidentDef>();
            foreach (IncidentDef incidentDef in entityCodexEntryDef.provocationIncidents)
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);
                incidentParms.bypassStorytellerSettings = true;
                isFiredTooRecently = isFiredTooRecently && incidentDef.Worker.FiredTooRecently(incidentParms.target);
                if (incidentDef.Worker.ChanceFactorNow(incidentParms.target) > 0 && incidentDef.Worker.CanFireNow(incidentParms))
                {
                    incidentDefs.Add(incidentDef);
                }
            }
            isCanFireNow = !incidentDefs.NullOrEmpty();
            lastUpdateTick = Find.TickManager.TicksGame;
        }
    }
}