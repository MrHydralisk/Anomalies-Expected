using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Building_AEChristmasTreeExit : Building_AEPocketMapExit
    {
        public ChristmasTreeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = Map?.GetComponent<ChristmasTreeMapComponent>() ?? null);
        private ChristmasTreeMapComponent mapComponentCached;
        public Building_AEChristmasTree entranceBuilding => mapComponent?.Entrance;

        public override string EnterString => "AnomaliesExpected.ChristmasStockings.Exit".Translate(Label);
        public override string EnteringString => "AnomaliesExpected.ChristmasStockings.Entering".Translate(Label);

        public override bool AutoDraftOnEnter => false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Alert_ChristmasTreeUnstable.AddTarget(this);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Alert_ChristmasTreeUnstable.RemoveTarget(this);
            base.Destroy(mode);
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            if (Find.CurrentMap == base.Map)
            {
                entranceBuilding.def.portal.traverseSound?.PlayOneShot(this);
            }
            else if (Find.CurrentMap == entranceBuilding.Map)
            {
                entranceBuilding.def.portal.traverseSound?.PlayOneShot(entranceBuilding);
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
                icon = ViewSubMapTex.Texture,
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
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
