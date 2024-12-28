using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Building_AEChristmasTreeExit : Building_AEMapPortal
    {
        private static readonly CachedTexture ExitPitGateTex = new CachedTexture("UI/Commands/ExitPitGate");
        protected override Texture2D EnterTex => ExitPitGateTex.Texture;

        private static readonly CachedTexture ViewSurfaceTex = new CachedTexture("UI/Commands/ViewUndercave");

        private ChristmasTreeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = Map?.GetComponent<ChristmasTreeMapComponent>() ?? null);
        private ChristmasTreeMapComponent mapComponentCached;
        public Building_AEChristmasTree entranceBuilding => mapComponent?.Entrance;

        public override string EnterCommandString => "AnomaliesExpected.BloodLake.Enter".Translate();

        public override bool AutoDraftOnEnter => true;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
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
