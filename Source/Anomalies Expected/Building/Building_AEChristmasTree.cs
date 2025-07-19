using RimWorld;
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

        public Building_AEChristmasTreeExit exitBuilding => exit as Building_AEChristmasTreeExit;

        private bool isReadyToEnter => (StudyUnlocks?.NextIndex ?? 1) >= 1;

        public override bool isHideEntry => !isReadyToEnter || !isCanCreatedMap;

        public override string EnterString => "AnomaliesExpected.ChristmasStockings.Enter".Translate(Label);
        public override string EnteringString => "AnomaliesExpected.ChristmasStockings.Entering".Translate(Label);

        public override bool AutoDraftOnEnter => true;
        protected override DamageDef pocketMapDamageDef => DamageDefOf.Frostbite;
        public bool isCanCreatedMap = true;

        public int NewYearTick;
        public int NextNewYearTick => Mathf.CeilToInt(Find.TickManager.TicksGame / 3600000f) * 3600000;

        protected override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame > NewYearTick && !isCanCreatedMap)
            {
                isCanCreatedMap = true;
                Messages.Message("AnomaliesExpected.ChristmasStockings.NewYear".Translate(Label), this, MessageTypeDefOf.PositiveEvent);
            }
        }

        protected override Map GeneratePocketMapInt()
        {
            Map pocketMap = PocketMapUtility.GeneratePocketMap(new IntVec3(def.portal.pocketMapSize, 1, def.portal.pocketMapSize), def.portal.pocketMapGenerator, GetExtraGenSteps(), base.Map);
            isCanCreatedMap = false;
            NewYearTick = NextNewYearTick;
            Find.LetterStack.ReceiveLetter("AnomaliesExpected.ChristmasStockings.LetterEnter.Label".Translate(), "AnomaliesExpected.ChristmasStockings.LetterEnter.Text".Translate(), LetterDefOf.ThreatSmall, new TargetInfo(exitBuilding));
            return pocketMap;
        }

        public override bool IsEnterable(out string reason)
        {
            reason = "";
            if (!isReadyToEnter)
            {
                reason = "AnomaliesExpected.ChristmasStockings.Reason.CantEnterA".Translate();
                return false;
            }
            if (!isCanCreatedMap && !isPocketMapExist)
            {
                reason = "AnomaliesExpected.ChristmasStockings.Reason.CantEnterB".Translate();
                return false;
            }
            return true;
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            StudyUnlocks.UnlockStudyNoteManual(1);
        }

        public override void DestroyPocketMap()
        {
            Alert_ChristmasTreeUnstable.RemoveTarget(exitBuilding);
            base.DestroyPocketMap();
            pocketMap = null;
            StudyUnlocks.UnlockStudyNoteManual(2);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        GeneratePocketMap();
                    },
                    defaultLabel = "Dev: Generate Pocket Map",
                    defaultDesc = "Generate pocket map for Christmas Tree"
                };
                if (isPocketMapExist)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            DestroyPocketMap();
                        },
                        defaultLabel = "Dev: Destroy pocket map",
                        defaultDesc = "Destroy pocket map"
                    };
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isCanCreatedMap, "isCanCreatedMap", true);
            Scribe_Values.Look(ref NewYearTick, "NewYearTick");
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            if (!isCanCreatedMap && !isPocketMapExist)
            {
                int diff = NewYearTick - Find.TickManager.TicksGame;
                inspectStrings.Add("AnomaliesExpected.ChristmasStockings.Tree.TimeTillNewYear".Translate(diff.ToStringTicksToPeriod()));
            }
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
