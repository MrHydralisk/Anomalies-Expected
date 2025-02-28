using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilityIceCrystalDestabilization : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public int damAmount = -1;
        public int armorPenetration = -1;
        public HediffDef ignoreWithHediffDef;
        public int minTargets = 1;
        public int minTargetsWEffect = 3;

        public CompProperties_AbilityIceCrystalDestabilization()
        {
            compClass = typeof(CompAbilityEffect_IceCrystalDestabilization);
        }
    }
}
