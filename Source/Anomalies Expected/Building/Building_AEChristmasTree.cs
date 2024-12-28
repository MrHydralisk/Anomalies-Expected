using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Building_AEChristmasTree : Building_AEMapPortal
    {
        private static readonly CachedTexture EnterPitGateTex = new CachedTexture("UI/Commands/EnterPitGate");
        protected override Texture2D EnterTex => EnterPitGateTex.Texture;

        private static readonly CachedTexture ViewUndercaveTex = new CachedTexture("UI/Commands/ViewUndercave");

        public AE_BloodLakeExtension ExtBloodLake => extBloodLakeCached ?? (extBloodLakeCached = def.GetModExtension<AE_BloodLakeExtension>());
        private AE_BloodLakeExtension extBloodLakeCached;

        public Map subMap;
        private ChristmasTreeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = subMap?.GetComponent<ChristmasTreeMapComponent>() ?? null);
        private ChristmasTreeMapComponent mapComponentCached;
        public Building_AEChristmasTreeExit exitBuilding => mapComponent?.Exit;

        private bool isBeenEntered;
        private bool isReadyToEnter => (StudyUnlocks?.NextIndex ?? 1) >= 1;

        public override string EnterCommandString => "AnomaliesExpected.BloodLake.Enter".Translate();

        public override bool AutoDraftOnEnter => true;
        public bool isDestroyedMap;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (subMap != null && !isDestroyedMap)
            {
                mapComponent.DestroySubMap();
            }
            base.Destroy(mode);
        }

        public void GenerateSubMap()
        {
            if (ExtBloodLake.mapGeneratorDef != null)
            {
                PocketMapParent pocketMapParent = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) as PocketMapParent;
                pocketMapParent.sourceMap = Map;
                subMap = MapGenerator.GenerateMap(new IntVec3(50, 1, 50), pocketMapParent, ExtBloodLake.mapGeneratorDef, isPocketMap: true);
                Find.World.pocketMaps.Add(pocketMapParent);
                //StudyUnlocks.UnlockStudyNoteManual(0);
            }
        }

        public override bool IsEnterable(out string reason)
        {
            reason = "";
            //if ((StudyUnlocks?.NextIndex ?? 3) < 3)
            //{
            //    reason = "AnomaliesExpected.BloodLake.Reason.CantEnterA".Translate();
            //    return false;
            //}
            //if (!isReadyToEnter)
            //{
            //    reason = "AnomaliesExpected.BloodLake.Reason.CantEnterB".Translate();
            //    return false;
            //}
            //else if (isDestroyedMap)
            //{
            //    reason = "AnomaliesExpected.BloodLake.Reason.CantEnterC".Translate();
            //    return false;
            //}
            return true;
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            if (!isBeenEntered)
            {
                isBeenEntered = true;
                Find.LetterStack.ReceiveLetter("AnomaliesExpected.BloodLake.LetterEnter.Label".Translate(), "AnomaliesExpected.BloodLake.LetterEnter.Text".Translate(), LetterDefOf.ThreatBig, new TargetInfo(exitBuilding));
            }
            if (Find.CurrentMap == base.Map)
            {
                SoundDefOf.TraversePitGate.PlayOneShot(this);
            }
            else if (Find.CurrentMap == exitBuilding.Map)
            {
                SoundDefOf.TraversePitGate.PlayOneShot(exitBuilding);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action command_Action && command_Action.icon == EnterTex && (!isReadyToEnter || isDestroyedMap))
                {
                    continue;
                }
                yield return gizmo;
            }
            if (subMap != null && !isDestroyedMap)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AnomaliesExpected.BloodLake.ViewSubMap.Label".Translate(),
                    defaultDesc = "AnomaliesExpected.BloodLake.ViewSubMap.Desc".Translate(),
                    icon = ViewUndercaveTex.Texture,
                    action = delegate
                    {
                        CameraJumper.TryJumpAndSelect(exitBuilding);
                    }
                };
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        GenerateSubMap();
                    },
                    defaultLabel = "Dev: Generate Sub Map",
                    defaultDesc = "Generate sub map for Blood Lake"
                };
                if (subMap != null && !isDestroyedMap)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            mapComponent.DestroySubMap();
                        },
                        defaultLabel = "Dev: Destroy sub map",
                        defaultDesc = "Destroy sub map"
                    };
                }
            }
        }

        public override Map GetOtherMap()
        {
            if (subMap == null)
            {
                GenerateSubMap();
            }
            return subMap;
        }

        public override IntVec3 GetDestinationLocation()
        {
            return exitBuilding?.Position ?? IntVec3.Invalid;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref subMap, "subMap");
            Scribe_Values.Look(ref isBeenEntered, "beenEntered", defaultValue: false);
            Scribe_Values.Look(ref isDestroyedMap, "isDestroyedMap");
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
