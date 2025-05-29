using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_Speedometer : CompProperties_Interactable
    {
        public int tickPerAction = 60000;
        public IntRange DecelerationIntervalRange = new IntRange(45000, 75000);
        public DamageDef deathDamageDef;
        public int deathDamagePerLevel = 35;

        public HediffDef AccelerationHediffDef;
        public HediffDef DecelerationHediffDef;
        public HediffDef ChronoDestabilizationHediffDef;
        public HediffDef ChronoComaHediffDef;

        [NoTranslate]
        public string dropTexPath;

        public List<GeneDef> destabilizationGeneDefs = new List<GeneDef>();

        public CompProperties_Speedometer()
        {
            compClass = typeof(Comp_Speedometer);
        }
    }
}
