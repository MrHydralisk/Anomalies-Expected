using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilitySpewLiquid : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;

        public int damAmount = -1;
        public int armorPenetration = -1;

        public float lineWidthEnd;

        public ThingDef filthDef;

        public EffecterDef sprayEffecter;

        public bool canHitFilledCells;

        public FleckDef fleckDef;

        public EffecterDef effecterDef;

        public int maintainForTicks = -1;

        public float scale = 1f;

        public CompProperties_AbilitySpewLiquid()
        {
            compClass = typeof(CompAbilityEffect_SpewLiquid);
        }
    }
}
