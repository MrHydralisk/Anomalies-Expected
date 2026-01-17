using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class TopOnBuilding_ClockHandMinute : TopOnBuilding_Clockwork
    {
        public IntVec3 target;

        public TopOnBuilding_ClockHandMinute() : base()
        {
        }

        public TopOnBuilding_ClockHandMinute(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }

        public override void OnTimerEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            target = IntVec3.Invalid;
            float radius = (compObelisk_Clockwork.Props.MinuteHandZoneDef.CompDefFor<Comp_ZoneOfEffect>() as CompProperties_ZoneOfEffect).radius;
            int score = int.MinValue;
            List<Pawn> freePawns = map.mapPawns.FreeColonistsSpawned.Where((Pawn p) => !p.DeadOrDowned).ToList();
            foreach (Pawn pawn in freePawns)
            {
                int curScore = freePawns.Count((Pawn p) => p.PositionHeld.DistanceTo(pawn.PositionHeld) < radius);
                if (score < curScore)
                {
                    score = curScore;
                    target = pawn.PositionHeld;
                }
            }
            if (target == IntVec3.Invalid)
            {
                target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position ?? map.mapPawns.AllPawns.FirstOrDefault((Pawn p) => p.Spawned && !p.DeadOrDowned)?.Position ?? (position + IntVec3.North);
            }
            Vector3 vector = (target.ToVector3Shifted() - position.ToVector3Shifted()).Yto0().normalized;
            CurRotation = vector.ToAngleFlat();
            base.OnTimerEnd();
            topOnBuildingStructure.SoundOnAiming?.PlayOneShotOnCamera();
            if (AEMod.Settings.NotifyClockworkHandMinute)
            {
                Messages.Message("AnomaliesExpected.ObeliskClockwork.HandMinute.Aiming".Translate(), new TargetInfo(Obelisk_Clockwork.Position, Obelisk_Clockwork.Map), MessageTypeDefOf.NegativeEvent);
            }
        }

        public override void OnWarmupEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            Thing zone = ThingMaker.MakeThing(compObelisk_Clockwork.Props.MinuteHandZoneDef);
            GenSpawn.Spawn(zone, target, compObelisk_Clockwork.parent.Map);
            Comp_ZoneOfEffect zoneComp = zone.TryGetComp<Comp_ZoneOfEffect>();
            if (zoneComp != null)
            {
                zoneComp.StartAttack(compObelisk_Clockwork.parent, compObelisk_Clockwork.parent.def);
                zoneComp.damageMult = isActive ? compObelisk_Clockwork.Props.DamageMultActive : 1f;
            }
            compObelisk_Clockwork.StudyUnlocks.UnlockStudyNoteManual(1);
            base.OnWarmupEnd();
        }

        public override void OnAwaken()
        {
            Messages.Message("AnomaliesExpected.ObeliskClockwork.HandMinute.Awaken".Translate(), new TargetInfo(Obelisk_Clockwork.Position, Obelisk_Clockwork.Map), MessageTypeDefOf.NeutralEvent);
            base.OnAwaken();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref target, "target");
        }
    }
}
