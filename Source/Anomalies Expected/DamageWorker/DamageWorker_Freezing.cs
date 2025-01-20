using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_Freezing : DamageWorker_Frostbite
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            AE_DamageDefExtension damageDefExtension = dinfo.Def.GetModExtension<AE_DamageDefExtension>();
            if (thing is Pawn pawn && damageDefExtension != null)
            {
                float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin);
                dinfo.SetAmount(dinfo.Amount * damageDefExtension.DamageMultiplierCurve.Evaluate(statValue));
            }
            return base.Apply(dinfo, thing);
        }
    }
}
