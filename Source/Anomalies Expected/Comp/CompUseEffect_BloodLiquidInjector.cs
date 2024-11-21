using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class CompUseEffect_BloodLiquidInjector : CompUseEffect
    {
        public CompProperties_UseEffectBloodLiquidInjector Props => (CompProperties_UseEffectBloodLiquidInjector)props;

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!p.health.hediffSet.hediffs.Any((Hediff h) => (h.TendableNow() && (h is Hediff_Injury || h is Hediff_MissingPart)) || h.def == HediffDefOf.BloodLoss))
            {
                return "AbilityCannotCastNoHealableInjury".Translate(p.Named("PAWN")).Resolve().StripTags() ?? "";
            }
            return true;
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            Hediff firstHediffOfDef = usedBy.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (firstHediffOfDef != null)
            {
                firstHediffOfDef.Severity -= Props.bloodLossOffset;
            }
            List<Hediff> hediffs = usedBy.health.hediffSet.hediffs.Where((Hediff h) => h.TendableNow()).OrderByDescending((Hediff h) => h.BleedRate).ThenByDescending((Hediff h) => h.Severity).ToList();
            for (int i = 0; i < hediffs.Count() && i < Props.amountOfInjuries; i++)
            {
                Hediff hediff = hediffs[i];
                if ((hediff is Hediff_Injury || hediff is Hediff_MissingPart) && hediff.TendableNow())
                {
                    hediff.Tended(Props.tendQualityRange.RandomInRange, Props.tendQualityRange.TrueMax, 1);
                }
            }
            if (Props.sideEffectHediff != null)
            {
                HealthUtility.AdjustSeverity(usedBy, Props.sideEffectHediff, Props.sideEffectHediffSeverityAdd);
            }
        }
    }
}
