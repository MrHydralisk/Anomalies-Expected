using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Building_AEBloodLakeExit : Building_AEMapPortal
    {
        private static readonly CachedTexture ExitPitGateTex = new CachedTexture("UI/Commands/ExitPitGate");
        protected override Texture2D EnterTex => ExitPitGateTex.Texture;

        private static readonly CachedTexture ViewSurfaceTex = new CachedTexture("UI/Commands/ViewUndercave");

        public AE_BloodLakeExtension ExtBloodLake => extBloodLakeCached ?? (extBloodLakeCached = def.GetModExtension<AE_BloodLakeExtension>());
        private AE_BloodLakeExtension extBloodLakeCached;

        private BloodLakeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = Map?.GetComponent<BloodLakeMapComponent>() ?? null);
        private BloodLakeMapComponent mapComponentCached;
        public Building_AEBloodLake entranceBuilding => mapComponent?.Entrance;

        public override string EnterCommandString => "EnterPitGate".Translate();

        public override bool AutoDraftOnEnter => true;

        public Building_AE terminal => mapComponent?.Terminal;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (IntVec3 item in GenAdj.OccupiedRect(Position, Rot4.North, def.Size))
            {
                if (GenGrid.InBounds(item, Map))
                {
                    FilthMaker.TryMakeFilth(item, Map, ExtBloodLake.filthDef, ExtBloodLake.filthThickness);
                }
            }
            foreach (IntVec3 item in GenAdj.OccupiedRect(Position, Rot4.North, def.Size + IntVec2.Two))
            {
                if (GenGrid.InBounds(item, Map))
                {
                    FilthMaker.TryMakeFilth(item, Map, ThingDefOf.Filth_Blood, 1);
                }
            }
            base.Destroy(mode);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if ((StudyUnlocks?.NextIndex ?? 2) >= 2)
            {
                new LookTargets(terminal).Highlight();
            }
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            if (Find.CurrentMap == base.Map)
            {
                SoundDefOf.TraversePitGate.PlayOneShot(this);
            }
            else if (Find.CurrentMap == entranceBuilding.Map)
            {
                SoundDefOf.TraversePitGate.PlayOneShot(entranceBuilding);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                defaultLabel = "ViewSurface".Translate(),
                defaultDesc = "ViewSurfaceDesc".Translate(),
                icon = ViewSurfaceTex.Texture,
                action = delegate
                {
                    CameraJumper.TryJumpAndSelect(entranceBuilding);
                }
            };
        }

        public override Map GetOtherMap()
        {
            return entranceBuilding.Map;
        }

        public override IntVec3 GetDestinationLocation()
        {
            return entranceBuilding.Position;
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            //int study = StudyUnlocks?.NextIndex ?? 4;
            //BloodLakeSummonHistory summonHistory = SummonHistories.FirstOrDefault();
            //if (summonHistory != null)
            //{
            //    int ticksLeft = summonHistory.tickNextSummon - Find.TickManager.TicksGame;
            //    if (ticksLeft < 2500)
            //    {
            //        inspectStrings.Add("Less than hour");
            //    }
            //    else if (ticksLeft < 15000)
            //    {
            //        inspectStrings.Add("Low density");
            //    }
            //    else if (ticksLeft < 60000)
            //    {
            //        inspectStrings.Add("High density");
            //    }
            //    else
            //    {
            //        inspectStrings.Add("Almost solid");
            //    }
            //}
            //if (study > 0)
            //{
            //    inspectStrings.Add("AnomaliesExpected.BeamTarget.Indicator".Translate(beamNextCount, ExtBloodLake.beamMaxCount).RawText);
            //    if (study > 1)
            //    {
            //        inspectStrings.Add("AnomaliesExpected.BeamTarget.State".Translate(beamTargetState == BeamTargetState.Searching ? "AnomaliesExpected.BeamTarget.StateSearching".Translate() : "AnomaliesExpected.BeamTarget.StateActivating".Translate()).RawText);
            //    }
            //    if (beamTargetState == BeamTargetState.Activating)
            //    {
            //        if (ParentHolder is MinifiedThing)
            //        {
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState + ExtBloodLake.ticksWhenCarried - Find.TickManager.TicksGame, ExtBloodLake.ticksWhenCarried).ToStringTicksToPeriodVerbose()).RawText);
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.ButtonHold".Translate().RawText);
            //        }
            //        else
            //        {
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState - Find.TickManager.TicksGame, 0).ToStringTicksToPeriodVerbose()).RawText);
            //        }
            //    }
            //}
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
