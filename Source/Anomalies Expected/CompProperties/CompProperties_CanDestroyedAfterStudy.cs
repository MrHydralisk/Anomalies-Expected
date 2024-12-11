using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_CanDestroyedAfterStudy : CompProperties_Interactable
    {
        public int minStudy = 5;
        public ResearchProjectDef DestroyUnlockResearchDef;
        public JobDef jobDef;

        public FleckDef fleckOnAnomaly;
        public float fleckOnAnomalyScale = 1;

        public string interactionProgressString;

        public List<ThingDefCountClass> requiredThings = new List<ThingDefCountClass>();

        public int DestroyMode;

        public CompProperties_CanDestroyedAfterStudy()
        {
            compClass = typeof(Comp_CanDestroyedAfterStudy);
        }
    }
}
