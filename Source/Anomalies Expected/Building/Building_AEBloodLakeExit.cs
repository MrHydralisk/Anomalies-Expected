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

        public override string EnterCommandString => "AnomaliesExpected.BloodLake.Enter".Translate();

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
                defaultLabel = "AnomaliesExpected.BloodLake.ViewSubMap.Label".Translate(),
                defaultDesc = "AnomaliesExpected.BloodLake.ViewSubMap.Desc".Translate(),
                icon = ViewSurfaceTex.Texture,
                action = delegate
                {
                    CameraJumper.TryJumpAndSelect(entranceBuilding);
                }
            };
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        mapComponent.TrySpawnWaveFromUndergroundNest(Map.mapPawns.FreeColonistsAndPrisonersSpawned);
                    },
                    defaultLabel = "Dev: Force spawn fleshbeasts",
                    defaultDesc = "Force spawn wave from Underground Nest"
                };
            }
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
            inspectStrings.Add("AnomaliesExpected.BloodLake.Density".Translate(657));
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
