﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Building_AEChristmasTree : Building_AEMapPortal
    {
        public AE_BloodLakeExtension ExtBloodLake => extBloodLakeCached ?? (extBloodLakeCached = def.GetModExtension<AE_BloodLakeExtension>());
        private AE_BloodLakeExtension extBloodLakeCached;

        public Map subMap;
        public ChristmasTreeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = subMap?.GetComponent<ChristmasTreeMapComponent>() ?? null);
        private ChristmasTreeMapComponent mapComponentCached;
        public Building_AEChristmasTreeExit exitBuilding => mapComponent?.Exit;

        private bool isBeenEntered;
        public bool isBeenExited;
        private bool isReadyToEnter => (StudyUnlocks?.NextIndex ?? 1) >= 1;
        public bool isSubMapExist => subMap != null && Find.Maps.IndexOf(subMap) >= 0;

        public override string EnterString => "AnomaliesExpected.ChristmasStockings.Enter".Translate(Label);
        public override string EnteringString => "AnomaliesExpected.ChristmasStockings.Entering".Translate(Label);

        public override bool AutoDraftOnEnter => true;
        public bool isCreatedMap;

        public int NewYearTick;
        public int NextNewYearTick => Mathf.CeilToInt(Find.TickManager.TicksGame / 3600000f) * 3600000;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (isSubMapExist)
            {
                mapComponent.DestroySubMap();
            }
            base.Destroy(mode);
        }

        protected override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame > NewYearTick && isCreatedMap)
            {
                isCreatedMap = false;
                Messages.Message("AnomaliesExpected.ChristmasStockings.NewYear".Translate(Label), this, MessageTypeDefOf.PositiveEvent);
            }
        }

        public void GenerateSubMap()
        {
            if (def.portal.pocketMapGenerator != null)
            {
                subMap = PocketMapUtility.GeneratePocketMap(new IntVec3(def.portal.pocketMapSize, 1, def.portal.pocketMapSize), def.portal.pocketMapGenerator, null, Map);
                isCreatedMap = true;
                mapComponentCached = subMap?.GetComponent<ChristmasTreeMapComponent>() ?? null;
                NewYearTick = NextNewYearTick;
                Find.LetterStack.ReceiveLetter("AnomaliesExpected.ChristmasStockings.LetterEnter.Label".Translate(), "AnomaliesExpected.ChristmasStockings.LetterEnter.Text".Translate(), LetterDefOf.ThreatSmall, new TargetInfo(exitBuilding));
            }
        }

        public override bool IsEnterable(out string reason)
        {
            reason = "";
            if (!isReadyToEnter)
            {
                reason = "AnomaliesExpected.ChristmasStockings.Reason.CantEnterA".Translate();
                return false;
            }
            if (isCreatedMap && !isSubMapExist)
            {
                reason = "AnomaliesExpected.ChristmasStockings.Reason.CantEnterB".Translate();
                return false;
            }
            return true;
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            if (!isBeenEntered)
            {
                isBeenEntered = true;
                StudyUnlocks.UnlockStudyNoteManual(1);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action command_Action && command_Action.icon == EnterTex && (!isReadyToEnter || (isCreatedMap && !isSubMapExist)))
                {
                    continue;
                }
                yield return gizmo;
            }
            if (isSubMapExist)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AnomaliesExpected.BloodLake.ViewSubMap.Label".Translate(),
                    defaultDesc = "AnomaliesExpected.BloodLake.ViewSubMap.Desc".Translate(),
                    icon = ViewSubMapTex.Texture,
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
                if (isSubMapExist)
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
            if (!isSubMapExist)
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
            Scribe_Values.Look(ref isBeenEntered, "isBeenEntered", defaultValue: false);
            Scribe_Values.Look(ref isBeenExited, "isBeenExited", defaultValue: false);
            Scribe_Values.Look(ref isCreatedMap, "isCreatedMap");
            Scribe_Values.Look(ref NewYearTick, "NewYearTick");
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            if (isCreatedMap && !isSubMapExist)
            {
                int diff = NewYearTick - Find.TickManager.TicksGame;
                inspectStrings.Add("AnomaliesExpected.ChristmasStockings.Tree.TimeTillNewYear".Translate(diff.ToStringTicksToPeriod()));
            }
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
