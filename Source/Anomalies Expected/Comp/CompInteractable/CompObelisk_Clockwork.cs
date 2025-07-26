using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class CompObelisk_Clockwork : CompInteractable
    {
        public int radius = 3;

        public void StartAgingBeam()
        {
            Map map = parent.Map;
            Vector3 target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position.ToVector3Shifted() ?? (parent.Position.ToVector3Shifted() + Vector3.forward);
            Vector3 vector = (target - parent.Position.ToVector3Shifted()).Yto0().normalized;
            List<IntVec3> beamPoints = GenSight.BresenhamCellsBetween(parent.Position, parent.Position + (vector * map.Size.Magnitude).ToIntVec3());
            int pointsToCompare = radius * radius * 4;
            List<IntVec3> hitPoints = new List<IntVec3>();
            foreach(IntVec3 beamPoint in beamPoints)
            {
                if (!beamPoint.InBounds(map))
                {
                    break;
                }
                List<IntVec3> radialPoints = GenRadial.RadialCellsAround(beamPoint, radius, true).Where((IntVec3 iv) => iv.InBounds(map)).ToList();
                List<IntVec3> hitPointsLast = hitPoints.Skip(Math.Max(hitPoints.Count() - pointsToCompare, 0)).ToList();
                radialPoints = radialPoints.Except(hitPointsLast).ToList();
                hitPoints.AddRange(radialPoints);
            }
            GenExplosion.DoExplosion(parent.Position, map, map.Size.Magnitude, DamageDefOf.Bomb, parent, damAmount: 1, overrideCells: hitPoints, propagationSpeed: 3);
        }

        public void StartAgingZone()
        {
            Map map = parent.Map;
            GenExplosion.DoExplosion(map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position ?? parent.Position, map, 15, DamageDefOf.Bomb, parent, damAmount: 1, propagationSpeed: 3);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                if (gizmo is Command_Action command_Action)
                {
                    command_Action.hotKey = KeyBindingDefOf.Misc6;
                }
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        StartAgingBeam();
                    },
                    defaultLabel = "Dev: Start Aging Beam",
                    defaultDesc = "Shooting aging beam"
                };
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        StartAgingZone();
                    },
                    defaultLabel = "Dev: Start Aging Zone",
                    defaultDesc = "Explode againg zone"
                };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            //ShootBeam();
        }
    }
}
