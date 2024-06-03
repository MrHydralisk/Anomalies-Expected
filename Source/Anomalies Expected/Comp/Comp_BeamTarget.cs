using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class Comp_BeamTarget : ThingComp
    {
        public CompProperties_BeamTarget Props => (CompProperties_BeamTarget)props;

        private int beamNextCount = 1;

        private int beamMaxCount = 1;

        public void SpawnBeams()
        {
            SpawnBeam(parent.Position);
            for (int i = 1; i < beamNextCount; i++)
            {
                if (CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, Props.beamSubRadius, delegate (IntVec3 newLoc)
                {
                    return true;
                }, out var result))
                {
                    SpawnBeam(result);
                }
                else
                {
                    SpawnBeam(parent.Position);
                }
            }
            beamNextCount = Rand.RangeInclusive(1, beamMaxCount);
            if (beamNextCount == beamMaxCount && beamNextCount < Props.beamMaxCount)
            {
                beamMaxCount = Mathf.Min(beamMaxCount + 1, Props.beamMaxCount);
            }
        }

        public void SpawnBeam(IntVec3 position)
        {
            PowerBeam obj = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, position, parent.Map);
            obj.duration = Props.beamDuration;
            obj.instigator = parent;
            obj.weaponDef = parent.def;
            obj.StartStrike();
        }

        public void SkipToRandom()
        {
            Map map = parent.Map;
            if (CellFinder.TryFindRandomCell(map, delegate (IntVec3 newLoc)
            {
                return newLoc.Walkable(map) && !newLoc.Fogged(map) && newLoc.GetFirstPawn(map) == null && newLoc.GetRoom(map) != parent.Position.GetRoom(map);
            }, out var result))
            {
                FleckMaker.Static(parent.Position, map, FleckDefOf.PsycastSkipInnerExit, 0.3f);
                FleckMaker.Static(result, map, FleckDefOf.PsycastSkipFlashEntry, 0.3f);
                Messages.Message("AnomaliesExpected.BeamTarget.LeftContainment".Translate(parent.LabelCap).RawText, this.parent, MessageTypeDefOf.NegativeEvent);
                parent.Position = result;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SpawnBeams();
                    },
                    defaultLabel = "Dev: Beam now",
                    defaultDesc = "Activate Beam"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SkipToRandom();
                    },
                    defaultLabel = "Dev: Skip now",
                    defaultDesc = "Skip to random location"
                };
            }
        }

        //public override void PostExposeData()
        //{
        //    base.PostExposeData();
        //    Scribe_Collections.Look(ref storedSeverityList, "storedSeverityList", LookMode.Value);
        //}

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            //int study = StudyUnlocks?.NextIndex ?? 4;
            //if (study > 1)
            //{
            //    MeatGrinderMood mood = currMood;
            //    inspectStrings.Add("AnomaliesExpected.MeatGrinder.Noise".Translate(mood?.noise ?? 0).RawText);
            //    if (study > 2 && (mood?.bodyPartDefs?.Count() ?? 0) > 0)
            //    {
            //        inspectStrings.Add("AnomaliesExpected.MeatGrinder.BodyParts".Translate(String.Join(", ", mood.bodyPartDefs.Select(b => b.LabelCap))).RawText);
            //    }
            //    if (study > 3 && (mood?.isDanger ?? false))
            //    {
            //        inspectStrings.Add("AnomaliesExpected.MeatGrinder.Danger".Translate().RawText);
            //    }
            //}
            inspectStrings.Add($"{beamNextCount}/{Props.beamMaxCount} Lights");
            //inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }
    }
}
