using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_FleshmassMutation : HediffComp
    {
        public HediffCompProperties_FleshmassMutation Props => (HediffCompProperties_FleshmassMutation)props;

        public HediffDef hediffToAdd;
        public int hediffLevel = -1;

        public override string CompLabelInBracketsExtra => $"{hediffToAdd?.label ?? "---"} {Mathf.Round(parent.Severity * 100 / parent.def.maxSeverity)}%";

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.TickManager.TicksGame % 250 == 0 && parent.Severity >= parent.def.maxSeverity)
            {
                Mutation();
            }
        }

        protected virtual void Mutation()
        {
            if (ModsConfig.AnomalyActive)
            {
                Pawn pawn = parent.pawn;

                Hediff firstHediffOfDef = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == hediffToAdd && (parent.Part == null || h.Part == parent.Part));
                if (firstHediffOfDef == null)
                {
                    Hediff hediff = pawn.health.AddHediff(hediffToAdd, parent.Part);
                    if (hediff is Hediff_Level hediff_Level && hediffLevel > -1)
                    {
                        hediff_Level.SetLevelTo(hediffLevel);
                    }
                }
                else if (firstHediffOfDef is Hediff_Level && !(firstHediffOfDef is Hediff_GroupedLevel))
                {
                    ((Hediff_Level)firstHediffOfDef).ChangeLevel(1);
                }

                Messages.Message("AnomaliesExpected.Fleshmass.FleshmassMutation.Message".Translate(pawn.Label, hediffToAdd?.label ?? "---"), pawn, MessageTypeDefOf.NeutralEvent);
                parent.pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompExposeData()
        {
            Scribe_Defs.Look(ref hediffToAdd, "hediffToAdd");
            Scribe_Values.Look(ref hediffLevel, "hediffLevel", -1);
        }
    }
}
