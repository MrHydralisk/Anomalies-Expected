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
        public Vector3 vector;

        public TopOnBuilding_ClockHandSecond(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }

        public override void OnTimerEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            Vector3 target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position.ToVector3Shifted() ?? (position.ToVector3Shifted() + Vector3.forward);
            vector = (target - position.ToVector3Shifted()).Yto0().normalized;
            CurRotation = vector.ToAngleFlat();
        }

        public override void OnWarmupEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            List<IntVec3> beamPoints = GenSight.BresenhamCellsBetween(position, position + (vector * map.Size.Magnitude).ToIntVec3());
            int pointsToCompare = compObelisk_Clockwork.radius * compObelisk_Clockwork.radius * 4;
            List<IntVec3> hitPoints = new List<IntVec3>();
            foreach (IntVec3 beamPoint in beamPoints)
            {
                if (!beamPoint.InBounds(map))
                {
                    break;
                }
                List<IntVec3> radialPoints = GenRadial.RadialCellsAround(beamPoint, compObelisk_Clockwork.radius, true).Where((IntVec3 iv) => iv.InBounds(map)).ToList();
                List<IntVec3> hitPointsLast = hitPoints.Skip(Math.Max(hitPoints.Count() - pointsToCompare, 0)).ToList();
                radialPoints = radialPoints.Except(hitPointsLast).ToList();
                hitPoints.AddRange(radialPoints);
            }
            GenExplosion.DoExplosion(position, map, map.Size.Magnitude, DamageDefOf.Bomb, compObelisk_Clockwork.parent, damAmount: 1, overrideCells: hitPoints, propagationSpeed: 3);
        }
    }
}
