using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompUseEffect_ReplaceHediff : CompUseEffect
    {
        public CompProperties_UseEffectReplaceHediff Props => (CompProperties_UseEffectReplaceHediff)props;

        public override void DoEffect(Pawn user)
        {
            Hediff hediffFrom = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDefFrom);
            Hediff hediffTo = HediffMaker.MakeHediff(Props.hediffDefTo, user);
            float effect = Props.hediffDefTo.initialSeverity;
            if (Props.severity > 0)
            {
                effect = Props.severity;
            }
            else if (hediffFrom != null && Props.severityTransfere > 0)
            {
                effect = hediffFrom.Severity * Props.severityTransfere;
            }
            hediffTo.Severity = effect;
            user.health.RemoveHediff(hediffFrom);
            user.health.AddHediff(hediffTo);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!p.health.hediffSet.HasHediff(Props.hediffDefFrom))
            {
                return "MissingMaterials".Translate(Props.hediffDefFrom.label);
            }
            if (p.health.hediffSet.HasHediff(Props.hediffDefTo))
            {
                return "AlreadyHasHediff".Translate(Props.hediffDefTo.label);
            }
            return true;
        }
    }
}
