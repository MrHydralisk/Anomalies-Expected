using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class HediffComp_SnowBlockCompressor : HediffComp
    {
        private const float Radius = 6.9f;
        private Color color = new Color(0.31f, 0.69f, 0.835f, 1);

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(250))
            {
                if (TryExtinguish())
                {
                    SoundInfo soundInfo = new TargetInfo(Pawn.Position, Pawn.Map);
                    soundInfo.volumeFactor *= 0.2f;
                    soundInfo.pitchFactor *= 0.5f;
                    SoundDefOfLocal.Explosion_Stun.PlayOneShot(soundInfo);
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(Pawn.Position, Radius, true))
                    {
                        FleckMaker.ThrowExplosionCell(cell, Pawn.Map, FleckDefOf.AirPuff, color);
                    }
                    //FleckMaker.WaterSplash(Pawn.Position.ToVector3Shifted(), Pawn.Map, Radius * 6f, 20f);
                }
                TryPushCold();
            }
        }
        public bool TryExtinguish()
        {
            bool succeed = false;
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, Radius, true))
            {
                if (thing is Fire fire && !fire.DestroyedOrNull())
                {
                    fire.Destroy();
                    succeed = true;
                }
            }
            return succeed;
        }

        public bool TryPushCold()
        {
            if (Pawn.Spawned && Pawn.AmbientTemperature > -273f)
            {
                GenTemperature.PushHeat(Pawn, Pawn.BodySize * -50f);
                return true;
            }
            return false;
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
