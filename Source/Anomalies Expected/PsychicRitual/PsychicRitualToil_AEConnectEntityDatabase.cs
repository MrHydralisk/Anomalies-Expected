using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualToil_AEConnectEntityDatabase : PsychicRitualToil
    {
        private PsychicRitualRoleDef invokerRole;
        private SimpleCurve psychicShockChanceFromQualityCurve;
        public IncidentDef selectedIncidentDef;

        protected PsychicRitualToil_AEConnectEntityDatabase() : base()
        {
        }

        public PsychicRitualToil_AEConnectEntityDatabase(PsychicRitualRoleDef invokerRole, SimpleCurve psychicShockChanceFromQualityCurve)
        {
            this.invokerRole = invokerRole;
            this.psychicShockChanceFromQualityCurve = psychicShockChanceFromQualityCurve;
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            if (pawn != null)
            {
                ApplyOutcome(psychicRitual, pawn);
            }
        }

        private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker)
        {
            PsychicRitualDef_AEConnectEntityDatabase psychicRitualDef__AEConnectEntityDatabase = (PsychicRitualDef_AEConnectEntityDatabase)psychicRitual.def;
            Map map = psychicRitual.Map;
            IncidentParms incidentParms3 = StorytellerUtility.DefaultParmsNow(selectedIncidentDef.category, map);
            incidentParms3.bypassStorytellerSettings = true;
            Find.Storyteller.incidentQueue.Add(selectedIncidentDef, Find.TickManager.TicksGame + Mathf.RoundToInt(psychicRitualDef__AEConnectEntityDatabase.incidentDelayHoursRange.RandomInRange * 2500f), incidentParms3);
            TaggedString text = "VoidProvocationCompletedText".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL")) + "\n\n" + ("VoidProvocationSucceeded").Translate();
            if (Rand.Chance(psychicRitualDef__AEConnectEntityDatabase.psychicShockChanceFromQualityCurve.Evaluate(psychicRitual.PowerPercent)))
            {
                text += "\n\n" + "VoidProvocationDarkPsychicShock".Translate(invoker.Named("INVOKER"));
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.DarkPsychicShock, invoker);
                int duration = Mathf.RoundToInt(psychicRitualDef__AEConnectEntityDatabase.darkPsychicShockDurarionHoursRange.RandomInRange * 2500f);
                hediff.TryGetComp<HediffComp_Disappears>()?.SetDuration(duration);
                invoker.health.AddHediff(hediff);
            }
            Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label).CapitalizeFirst(), text, LetterDefOf.NeutralEvent);
            foreach (Pawn item2 in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                if (item2.needs.mood.thoughts.memories.NumMemoriesOfDef(ThoughtDefOf.VoidCuriosity) > 0)
                {
                    item2.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.VoidCuriosity);
                    item2.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.VoidCuriositySatisfied);
                }
            }
            Find.Anomaly.hasPerformedVoidProvocation = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
            Scribe_Deep.Look(ref psychicShockChanceFromQualityCurve, "psychicShockChanceFromQualityCurve");
            Scribe_Defs.Look(ref selectedIncidentDef, "selectedIncidentDef");
        }
    }
}
