using RimWorld;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_FreezingFragile : DamageWorker_AddInjury
    {
        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
        {
            AE_DamageDefExtension damageDefExtension = def.GetModExtension<AE_DamageDefExtension>();
            if (damageDefExtension?.AdditionalHediff != null)
            {
                float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin);
                float sevOffset = damageDefExtension.DamageMultiplierCurve.Evaluate(statValue);
                Hediff firstHediffOfDef = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.Part == dinfo.HitPart && h.def == damageDefExtension.AdditionalHediff);
                if (firstHediffOfDef != null)
                {
                    dinfo.SetAmount(dinfo.Amount * (1 + firstHediffOfDef.Severity));
                    firstHediffOfDef.Severity += sevOffset;
                }
                else if (sevOffset > 0f)
                {
                    firstHediffOfDef = HediffMaker.MakeHediff(damageDefExtension.AdditionalHediff, pawn);
                    firstHediffOfDef.Severity = sevOffset;
                    pawn.health.AddHediff(firstHediffOfDef, dinfo.HitPart);
                }
            }
            FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
        }

        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            AE_DamageDefExtension damageDefExtension = def.GetModExtension<AE_DamageDefExtension>();
            if (damageDefExtension != null)
            {
                if (thing is Building building)
                {
                    float mult = 1;
                    if (building.GetComp<CompPowerTrader>()?.PowerOn ?? false)
                    {
                        mult *= damageDefExtension.DamageMultiplierForPoweredBuildings;
                    }
                    else
                    {
                        mult *= damageDefExtension.DamageMultiplierForNonPoweredBuildings;
                    }
                    dinfo.SetAmount(dinfo.Amount * mult);
                }
                if (damageDefExtension.appliedDamageDef != null)
                {
                    dinfo.Def = damageDefExtension.appliedDamageDef;
                }
            }
            return base.Apply(dinfo, thing);
        }
    }
}
