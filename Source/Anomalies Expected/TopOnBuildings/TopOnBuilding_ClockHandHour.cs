using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class TopOnBuilding_ClockHandHour : TopOnBuilding_Clockwork
    {
        public IntVec3 target;

        public List<RectInt> PossibleLocations
        {
            get
            {
                if (possibleLocations.NullOrEmpty())
                {
                    Map map = compObelisk_Clockwork.parent.Map;
                    IntVec2 locationAmount = new IntVec2(Mathf.FloorToInt(map.Size.x / compObelisk_Clockwork.Props.sizeLocation), Mathf.FloorToInt(map.Size.z / compObelisk_Clockwork.Props.sizeLocation));
                    possibleLocations = new List<RectInt>();
                    IntVec2 locationSize = new IntVec2(Mathf.FloorToInt(map.Size.x / locationAmount.x), Mathf.FloorToInt(map.Size.z / locationAmount.z));
                    IntVec2 locationEdge = new IntVec2(map.Size.x, map.Size.z);
                    for (int i = locationAmount.x - 1; i >= 0; i--)
                    {
                        locationEdge.z = map.Size.z;
                        for (int j = locationAmount.z - 1; j >= 0; j--)
                        {
                            IntVec2 pos = locationEdge - locationSize;
                            RectInt rect = new RectInt(pos.x, pos.z, locationSize.x, locationSize.z);
                            possibleLocations.Add(rect);
                            locationEdge.z -= locationSize.z;
                        }
                        locationEdge.x -= locationSize.x;
                    }
                }
                return possibleLocations;
            }
        }
        public List<RectInt> possibleLocations;

        public TopOnBuilding_ClockHandHour(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }

        public override void OnTimerEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            target = IntVec3.Invalid;
            List<RectInt> targetLocations = PossibleLocations.ToList();
            while (!targetLocations.NullOrEmpty())
            {
                RectInt rect = targetLocations.RandomElement();
                targetLocations.Remove(rect);
                if (rect.Contains(new Vector2Int(position.x, position.y)))
                {
                    continue;
                }
                if (CellFinder.TryFindRandomCell(map, delegate (IntVec3 newLoc)
                {
                    foreach (IntVec3 pos in GenAdj.CellsOccupiedBy(newLoc, Rot4.North, compObelisk_Clockwork.parent.def.size))
                    {
                        if (!GenGrid.InBounds(pos, map, 2) || pos.Fogged(map) || !pos.Standable(map) || !pos.GetAffordances(map).Contains(compObelisk_Clockwork.parent.def.terrainAffordanceNeeded))
                        {
                            return false;
                        }
                    }
                    return true;
                }, out target))
                {
                    Vector3 vector = (target.ToVector3Shifted() - position.ToVector3Shifted()).Yto0().normalized;
                    CurRotation = vector.ToAngleFlat();
                    break;
                }
            }
        }

        public override void OnWarmupEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            if (target != IntVec3.Invalid)
            {
                TargetInfo targetInfoFrom = new TargetInfo(position, map);
                SoundDefOfLocal.Psycast_Skip_Exit.PlayOneShot(targetInfoFrom);
                FleckMaker.Static(targetInfoFrom.Cell, targetInfoFrom.Map, FleckDefOf.PsycastSkipInnerExit/*, Props.teleportationFleckRadius*/);
                TargetInfo targetInfoTo = new TargetInfo(position, map);
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(targetInfoTo);
                FleckMaker.Static(targetInfoTo.Cell, targetInfoFrom.Map, FleckDefOf.PsycastSkipFlashEntry/*, Props.teleportationFleckRadius*/);
                compObelisk_Clockwork.parent.Position = target;
            }
        }
    }
}
