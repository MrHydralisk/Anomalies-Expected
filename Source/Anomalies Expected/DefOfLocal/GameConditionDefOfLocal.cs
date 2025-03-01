using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    [DefOf]
    public static class GameConditionDefOfLocal
    {
        [MayRequireAnomaly]
        public static GameConditionDef AE_BloodFog;
        [MayRequireAnomaly]
        public static GameConditionDef AE_UnnaturalCold;
        [MayRequireAnomaly]
        public static GameConditionDef AE_UnnaturalTemperature;
    }
}