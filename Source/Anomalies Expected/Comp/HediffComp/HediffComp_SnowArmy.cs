using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_SnowArmy : HediffComp
    {
        public const float heatPushedPerBodySize = -10f;

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
                TryPushCold();
            }
        }

        public void TryPushCold()
        {
            if (Pawn.AmbientTemperature > -273f)
            {
                if (Pawn.Spawned)
                {
                    GenTemperature.PushHeat(Pawn, Pawn.BodySize * heatPushedPerBodySize);
                }
                else if (Pawn.SpawnedOrAnyParentSpawned)
                {
                    GenTemperature.PushHeat(Pawn.PositionHeld, Pawn.MapHeld, Pawn.BodySize * heatPushedPerBodySize);
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
