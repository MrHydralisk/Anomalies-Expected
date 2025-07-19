using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class Building_AEBloodLakeExit : Building_AEPocketMapExit
    {
        public AE_BloodLakeExtension ExtBloodLake => extBloodLakeCached ?? (extBloodLakeCached = def.GetModExtension<AE_BloodLakeExtension>());
        private AE_BloodLakeExtension extBloodLakeCached;

        private BloodLakeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = Map?.GetComponent<BloodLakeMapComponent>() ?? null);
        private BloodLakeMapComponent mapComponentCached;
        public Building_AEBloodLake entranceBuilding => entrance as Building_AEBloodLake;

        public override string EnterString => "AnomaliesExpected.BloodLake.Exit".Translate(Label);
        public override string EnteringString => "AnomaliesExpected.BloodLake.Entering".Translate(Label);

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
