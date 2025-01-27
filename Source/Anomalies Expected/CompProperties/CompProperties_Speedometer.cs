using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_Speedometer : CompProperties_Interactable
    {
        public int tickPerAction = 60000;
        public HediffDef AccelerationHediffDef;
        public HediffDef DecelerationHediffDef;
        public CompProperties_Speedometer()
        {
            compClass = typeof(Comp_Speedometer);
        }
    }
}
