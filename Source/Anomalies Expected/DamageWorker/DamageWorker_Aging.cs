using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_Aging : DamageWorker
    {
        private static List<Thing> thingsToAffect = new List<Thing>();

        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            AE_DamageDefExtension damageDefExtension = dinfo.Def.GetModExtension<AE_DamageDefExtension>();
            if (damageDefExtension != null)
            {
                if (!damageDefExtension.isDealDamageToSelf && thing == dinfo.Instigator)
                {
                    return new DamageResult();
                }
                long ticksAged = Mathf.CeilToInt(3600000L * dinfo.Amount);
                if (thing is Pawn pawn)
                {
                    float OrganicMult = pawn.def.race.IsFlesh ? 1 : damageDefExtension.DamageMultiplierForNonOrganic;
                    DamageResult damageResult = new DamageResult();
                    foreach (BodyPartRecord bodyPartRecord in pawn.RaceProps.body.AllParts)
                    {
                        if (bodyPartRecord.coverageAbs > 0f && !pawn.health.hediffSet.PartIsMissing(bodyPartRecord))
                        {
                            Hediff hediff = HediffMaker.MakeHediff(def.hediff, pawn, bodyPartRecord);
                            hediff.Severity = bodyPartRecord.def.GetMaxHealth(pawn) * (OrganicMult * dinfo.Amount / 400);
                            pawn.health.AddHediff(hediff, bodyPartRecord, dinfo, damageResult);
                        }
                    }
                    pawn.ageTracker.AgeBiologicalTicks += ticksAged;
                    return damageResult;
                }
                else
                {
                    if (thing is Plant plant)
                    {
                        float daysAged = ticksAged / 60000f;
                        float growDays = plant.def.plant.growDays;
                        plant.Age += (int)ticksAged;
                        plant.Growth = Mathf.Min(growDays, daysAged) / growDays;
                    }
                    dinfo.SetAmount(thing.MaxHitPoints * (dinfo.Amount / 100));
                }
            }
            return base.Apply(dinfo, thing);
        }
    }
}
