using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_Freezing : DamageWorker_Frostbite
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            AE_DamageDefExtension damageDefExtension = dinfo.Def.GetModExtension<AE_DamageDefExtension>();
            if (damageDefExtension != null)
            {
                Log.Message($"!{damageDefExtension.isDealDamageToFriendly} && ({thing.Faction != null} && {dinfo.Instigator.Faction != null} && !{thing.Faction.HostileTo(dinfo.Instigator.Faction)})");
                if (!damageDefExtension.isDealDamageToFriendly && (thing.Faction != null && dinfo.Instigator.Faction != null && !thing.Faction.HostileTo(dinfo.Instigator.Faction)))
                {
                    return new DamageResult();
                }
                if (thing is Pawn pawn)
                {
                    Log.Message($"!{damageDefExtension.isDealDamageToDowned} && {pawn.DeadOrDowned}");
                    if ((!damageDefExtension.isDealDamageToDowned && pawn.DeadOrDowned))
                    {
                        return new DamageResult();
                    }
                    float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin);
                    dinfo.SetAmount(dinfo.Amount * damageDefExtension.DamageMultiplierCurve.Evaluate(statValue));
                    if (damageDefExtension.AdditionalHediff != null)
                    {
                        HealthUtility.AdjustSeverity(pawn, damageDefExtension.AdditionalHediff, 0.5f);
                    }
                }
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
            }
            return base.Apply(dinfo, thing);
        }
    }
}
