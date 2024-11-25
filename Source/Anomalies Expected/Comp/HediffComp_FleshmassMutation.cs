using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_FleshmassMutation : HediffComp
    {
        public HediffCompProperties_FleshmassMutation Props => (HediffCompProperties_FleshmassMutation)props;

        public HediffDef hediffToAdd;

        public override string CompLabelInBracketsExtra => hediffToAdd?.label;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.TickManager.TicksGame % 250 == 0 && parent.Severity >= parent.def.maxSeverity)
            {
                Mutation();
            }
        }

        private void Mutation()
        {
            if (ModsConfig.AnomalyActive)
            {
                Pawn pawn = parent.pawn;
                pawn.health.AddHediff(hediffToAdd, parent.Part);
                Messages.Message("AnomaliesExpected.Misc.FleshmassMutation.Message".Translate(pawn.Label, hediffToAdd?.label ?? "---"), pawn, MessageTypeDefOf.NeutralEvent);
                parent.pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompExposeData()
        {
            Scribe_Defs.Look(ref hediffToAdd, "hediffToAdd");
        }
    }
}
