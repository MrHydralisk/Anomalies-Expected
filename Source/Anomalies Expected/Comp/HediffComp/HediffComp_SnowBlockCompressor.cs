using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class HediffComp_SnowBlockCompressor : HediffComp
    {
        public HediffCompProperties_SnowBlockCompressor Props => (HediffCompProperties_SnowBlockCompressor)props;

        private int TickNextExtinguish;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(250))
            {
                TryExtinguish();
                TryPushCold();
            }
        }

        public void TryExtinguish()
        {
            if (Find.TickManager.TicksGame < TickNextExtinguish)
            {
                return;
            }
            bool succeed = false;
            List<IntVec3> cells = GenRadial.RadialCellsAround(Pawn.Position, Props.radius, true).ToList();
            List<Thing> things = cells.SelectMany((IntVec3 iv3) => Pawn.Map.thingGrid.ThingsListAtFast(iv3)).ToList();
            foreach (Thing thing in things)
            {
                if (thing is Fire fire && !fire.DestroyedOrNull())
                {
                    fire.Destroy();
                    succeed = true;
                }
            }
            if (succeed)
            {
                TickNextExtinguish = Find.TickManager.TicksGame + Props.ticksBetweenExtinguish;
                SoundInfo soundInfo = new TargetInfo(Pawn.Position, Pawn.Map);
                soundInfo.volumeFactor *= 0.2f;
                soundInfo.pitchFactor *= 0.5f;
                Props.soundDef.PlayOneShot(soundInfo);
                foreach (IntVec3 cell in cells)
                {
                    FleckMaker.ThrowExplosionCell(cell, Pawn.Map, Props.fleckDef, Props.color);
                }
            }
        }

        public void TryPushCold()
        {
            if (Pawn.Spawned && Pawn.AmbientTemperature > -273f)
            {
                GenTemperature.PushHeat(Pawn, Pawn.BodySize * Props.heatPushedPerBodySize);
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

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref TickNextExtinguish, "TickNextEmerge", -1);
        }
    }
}
