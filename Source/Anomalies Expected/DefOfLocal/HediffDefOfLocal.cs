using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    [DefOf]
    public static class HediffDefOfLocal
    {
        public static HediffDef Hediff_AEFatigue;
        public static HediffDef Hediff_AEForbiddenFruit;
        public static HediffDef Hediff_AEForbiddenFruitWithdrawal;
        [MayRequireAnomaly]
        public static HediffDef Hediff_AEBloodLiquidConcentration;
        [MayRequireAnomaly]
        public static HediffDef Hediff_AEFleshmassCystAssimilation;
        [MayRequireAnomaly]
        public static HediffDef Hediff_AEFleshmassTumorAssimilation;
        [MayRequireAnomaly]
        public static HediffDef AE_FleshmassBodyMutation;
    }
}