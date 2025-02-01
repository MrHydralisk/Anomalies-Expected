using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_FleshmassRestoration : HediffComp
    {
        private bool isSurgicallyRemoved;

        public override void CompPostPostRemoved()
        {
            if (parent.Severity < parent.def.maxSeverity && Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfLocal.AE_FleshmassOrganogenesisCore) != null)
            {
                HediffDef hediffDef = parent.def;
                BodyPartRecord bodyPart = parent.Part;
                int hediffLevel = -1;
                if (parent is Hediff_Level hediff_Level)
                {
                    hediffLevel = hediff_Level.level;
                }
                if (hediffDef == HediffDefOfLocal.Hediff_AEFleshmassPartRestoration)
                {
                    HediffComp_FleshmassMutation hediffComp_FleshmassMutationParent = parent.GetComp<HediffComp_FleshmassMutation>();
                    if (hediffComp_FleshmassMutationParent == null)
                    {
                        Log.Error("hediffComp_FleshmassMutationParent not found for HediffComp_FleshmassRestoration");
                    }
                    else
                    {
                        hediffDef = hediffComp_FleshmassMutationParent.hediffToAdd;
                        hediffLevel = hediffComp_FleshmassMutationParent.hediffLevel;
                    }
                }
                Pawn.health.RestorePart(parent.Part, checkStateChange: false);
                base.CompPostPostRemoved();
                HediffWithComps hediff = Pawn.health.AddHediff(HediffDefOfLocal.Hediff_AEFleshmassPartRestoration, bodyPart) as HediffWithComps;
                HediffComp_FleshmassMutation hediffComp_FleshmassMutation = hediff.GetComp<HediffComp_FleshmassMutation>();
                if (hediffComp_FleshmassMutation == null)
                {
                    Log.Error("HediffComp_FleshmassMutation not found for HediffComp_FleshmassRestoration");
                }
                else
                {
                    hediffComp_FleshmassMutation.hediffToAdd = hediffDef;
                    hediffComp_FleshmassMutation.hediffLevel = hediffLevel;
                }
            }
        }

        public override void Notify_SurgicallyRemoved(Pawn surgeon)
        {
            isSurgicallyRemoved = true;
            base.Notify_SurgicallyRemoved(surgeon);
        }
    }
}
