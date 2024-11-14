using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
