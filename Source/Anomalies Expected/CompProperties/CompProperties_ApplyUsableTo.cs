using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_ApplyUsableTo : CompProperties_Usable
    {
        public JobDef applyToJob;

        [MustTranslate]
        public string useToLabel;

        public CompProperties_ApplyUsableTo()
        {
            compClass = typeof(CompApplyUsableTo);
        }
    }
}
