using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_SnowArmy : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(250))
            {
                float temp = -Pawn.AmbientTemperature * Pawn.BodySize;
                if (temp < 0)
                {
                    parent.Severity = 1;
                }
                else
                {
                    parent.Severity = Mathf.Max(2, temp);
                }
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (dinfo.Def != DamageDefOf.Burn && Pawn.IsBurning())
            {
                dinfo.SetAmount(2f * dinfo.Amount);
            }
        }
    }
}
