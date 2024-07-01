using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompUseEffect_RemoveHediff : CompUseEffect
    {
        public CompProperties_UseEffecRemoveHediff Props => (CompProperties_UseEffecRemoveHediff)props;

        public override void DoEffect(Pawn user)
        {
            HealthUtility.AdjustSeverity(user, Props.hediffDef, -Props.severity);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Props.requireHediffToUse && !p.health.hediffSet.HasHediff(Props.hediffDef))
            {
                return "MissingMaterials".Translate(Props.hediffDef.label);
            }
            return true;
        }
    }
}
