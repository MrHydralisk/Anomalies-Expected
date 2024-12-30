using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    [DefOf]
    public static class GameConditionDefOfLocal
    {
        [MayRequireAnomaly]
        public static GameConditionDef AE_BloodFog;
        public static GameConditionDef AE_TemperatureDrop;
    }
}