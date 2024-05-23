using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_UseEffectReplaceHediff : CompProperties_UseEffectAddHediff
    {
        public HediffDef hediffDefFrom;

        public HediffDef hediffDefTo;

        public float severity = -1f;

        public float severityTransfere = -1f;

        public CompProperties_UseEffectReplaceHediff()
        {
            compClass = typeof(CompUseEffect_ReplaceHediff);
        }
    }
}
