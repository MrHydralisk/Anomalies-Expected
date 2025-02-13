using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_AddInjuryFleshmass : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            AE_DamageDefExtension damageDefExtension = dinfo.Def.GetModExtension<AE_DamageDefExtension>();
            if ((!damageDefExtension.isDealDamageToFriendly && (thing.Faction != null && dinfo.Instigator.Faction != null && !thing.Faction.HostileTo(dinfo.Instigator.Faction))) || ((thing is Pawn pawn) && !damageDefExtension.isDealDamageToDowned && pawn.DeadOrDowned))
            {
                return new DamageResult();
            }
            return base.Apply(dinfo, thing);
        }
    }
}
