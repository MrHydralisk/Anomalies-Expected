using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class CompObelisk_Clockwork : CompInteractable
    {
        public new CompPropertiesObelisk_Clockwork Props => (CompPropertiesObelisk_Clockwork)props;

        public int radius = 3;

        public Dictionary<TopOnBuildingStructureTypes, TopOnBuilding> topOnBuildings;

        public override void PostPostMake()
        {
            base.PostPostMake();
            topOnBuildings = new Dictionary<TopOnBuildingStructureTypes, TopOnBuilding>();
            foreach (TopOnBuildingStructure structure in Props.topOnBuildingStructures)
            {
                topOnBuildings.Add(structure.type, new TopOnBuilding(structure));
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            topOnBuildings = new Dictionary<TopOnBuildingStructureTypes, TopOnBuilding>(); // Temp
            foreach (TopOnBuildingStructure structure in Props.topOnBuildingStructures) // Temp
            {
                topOnBuildings.Add(structure.type, new TopOnBuilding(structure));
            }

            if (topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandSecond, out TopOnBuilding clockHandSecond))
            {
                clockHandSecond.onTimerEnd = delegate { StartAgingBeam(); };
            }
            if (topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandMinute, out TopOnBuilding clockHandMinute))
            {
                clockHandMinute.onTimerEnd = delegate { StartAgingZone(); };
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            foreach (TopOnBuilding topOnBuilding in topOnBuildings.Values)
            {
                topOnBuilding.DrawAt(drawLoc);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            foreach (TopOnBuilding topOnBuilding in topOnBuildings.Values)
            {
                topOnBuilding.Tick();
            }
        }

        public void StartAgingBeam()
        {
            Map map = parent.Map;
            Vector3 target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position.ToVector3Shifted() ?? (parent.Position.ToVector3Shifted() + Vector3.forward);
            Vector3 vector = (target - parent.Position.ToVector3Shifted()).Yto0().normalized;
            if(topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandSecond, out TopOnBuilding clockHandSecond))
            {
                clockHandSecond.CurRotation = vector.ToAngleFlat();
            }
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
            IntVec3 target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position ?? (parent.Position + IntVec3.North);
            Vector3 vector = (target.ToVector3Shifted() - parent.Position.ToVector3Shifted()).Yto0().normalized;
            if (topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandMinute, out TopOnBuilding clockHandMinute))
            {
                clockHandMinute.CurRotation = vector.ToAngleFlat();
            }
            GenExplosion.DoExplosion(target, map, 15, DamageDefOf.Bomb, parent, damAmount: 1, propagationSpeed: 3);
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
