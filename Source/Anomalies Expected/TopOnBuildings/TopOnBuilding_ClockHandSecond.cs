using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding_ClockHandSecond : TopOnBuilding_Clockwork
    {
        public IntVec3 target;

        public TopOnBuilding_ClockHandSecond() : base()
        {
        }

        public TopOnBuilding_ClockHandSecond(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }

        public override void OnTimerEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position ?? map.mapPawns.AllPawns.FirstOrDefault((Pawn p) => p.Spawned && !p.DeadOrDowned)?.Position ?? (position + IntVec3.North);
            Vector3 vector = (target.ToVector3Shifted() - position.ToVector3Shifted()).Yto0().normalized;
            CurRotation = vector.ToAngleFlat();
            base.OnTimerEnd();
            if (AEMod.Settings.NotifyClockworkHandSecond)
            {
                Messages.Message("AnomaliesExpected.ObeliskClockwork.HandSecond.Aiming".Translate(), new TargetInfo(Obelisk_Clockwork.Position, Obelisk_Clockwork.Map), MessageTypeDefOf.NegativeEvent);
            }
        }

        public override void OnWarmupEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            Vector3 vector = (target.ToVector3Shifted() - position.ToVector3Shifted()).Yto0().normalized;
            List<IntVec3> beamPoints = GenSight.BresenhamCellsBetween(position, position + (vector * map.Size.Magnitude).ToIntVec3());
            int pointsToCompare = compObelisk_Clockwork.Props.radius * compObelisk_Clockwork.Props.radius * 4;
            List<IntVec3> hitPoints = new List<IntVec3>();
            foreach (IntVec3 beamPoint in beamPoints)
            {
                if (!beamPoint.InBounds(map))
                {
                    break;
                }
                List<IntVec3> radialPoints = GenRadial.RadialCellsAround(beamPoint, compObelisk_Clockwork.Props.radius, true).Where((IntVec3 iv) => iv.InBounds(map)).ToList();
                List<IntVec3> hitPointsLast = hitPoints.Skip(Math.Max(hitPoints.Count() - pointsToCompare, 0)).ToList();
                radialPoints = radialPoints.Except(hitPointsLast).ToList();
                hitPoints.AddRange(radialPoints);
            }
            GenExplosion.DoExplosion(position, map, map.Size.Magnitude, compObelisk_Clockwork.Props.damageDef, compObelisk_Clockwork.parent, damAmount: Mathf.RoundToInt(compObelisk_Clockwork.Props.damageAmountWave * (isActive ? compObelisk_Clockwork.Props.DamageMultActive : 1)), overrideCells: hitPoints, propagationSpeed: 3, weapon: compObelisk_Clockwork.parent.def);
            compObelisk_Clockwork.StudyUnlocks.UnlockStudyNoteManual(0);
            base.OnWarmupEnd();
        }

        public override void OnAwaken()
        {
            Messages.Message("AnomaliesExpected.ObeliskClockwork.HandSecond.Awaken".Translate(), new TargetInfo(Obelisk_Clockwork.Position, Obelisk_Clockwork.Map), MessageTypeDefOf.NeutralEvent);
            base.OnAwaken();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref target, "target");
        }
    }
}
