using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_AddInjuryFleshmass : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (thing.Faction == Faction.OfEntities)
            {
                return new DamageResult();
            }
            return base.Apply(dinfo, thing);
        }
    }
}
