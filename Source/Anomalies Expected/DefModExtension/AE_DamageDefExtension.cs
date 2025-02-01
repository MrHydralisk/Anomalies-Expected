using Verse;

namespace AnomaliesExpected
{
    public class AE_DamageDefExtension : DefModExtension
    {
        public SimpleCurve DamageMultiplierCurve = new SimpleCurve();
        public float DamageMultiplierForNonPoweredBuildings = 1;
        public float DamageMultiplierForPoweredBuildings = 1;
        public HediffDef AdditionalHediff;
    }
}
