using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class CompObelisk_Clockwork : CompInteractable
    {
        public new CompPropertiesObelisk_Clockwork Props => (CompPropertiesObelisk_Clockwork)props;

        public int radius = 3;
        public int sizeLocation = 60;

        public Dictionary<TopOnBuildingStructureTypes, TopOnBuilding_Clockwork> topOnBuildings;

        public override void PostPostMake()
        {
            base.PostPostMake();
            topOnBuildings = new Dictionary<TopOnBuildingStructureTypes, TopOnBuilding_Clockwork>();
            foreach (TopOnBuildingStructure structure in Props.topOnBuildingStructures)
            {
                topOnBuildings.Add(structure.type, (TopOnBuilding_Clockwork)Activator.CreateInstance(structure.topOnBuildingClass, new object[] { structure }));
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            topOnBuildings = new Dictionary<TopOnBuildingStructureTypes, TopOnBuilding_Clockwork>(); // Temp
            foreach (TopOnBuildingStructure structure in Props.topOnBuildingStructures) // Temp
            {
                TopOnBuilding_Clockwork topOnBuilding = (TopOnBuilding_Clockwork)Activator.CreateInstance(structure.topOnBuildingClass, new object[] { structure });
                topOnBuilding.compObelisk_Clockwork = this;
                topOnBuildings.Add(structure.type, topOnBuilding);
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            foreach (TopOnBuilding_Clockwork topOnBuilding in topOnBuildings.Values)
            {
                topOnBuilding.DrawAt(drawLoc);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            foreach (TopOnBuilding_Clockwork topOnBuilding in topOnBuildings.Values)
            {
                topOnBuilding.Tick();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                if (gizmo is Command_Action command_Action)
                {
                    command_Action.hotKey = KeyBindingDefOf.Misc6;
                }
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        if (topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandSecond, out TopOnBuilding_Clockwork clockHandSecond))
                        {
                            clockHandSecond.tickTillFullRotation = 1;
                        }
                    },
                    defaultLabel = "Dev: Start Aging Beam",
                    defaultDesc = "Shooting aging beam"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        if (topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandMinute, out TopOnBuilding_Clockwork clockHandMinute))
                        {
                            clockHandMinute.tickTillFullRotation = 1;
                        }
                    },
                    defaultLabel = "Dev: Start Aging Zone",
                    defaultDesc = "Explode againg zone"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        if (topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandHour, out TopOnBuilding_Clockwork clockHandHour))
                        {
                            clockHandHour.tickTillFullRotation = 1;
                        }
                    },
                    defaultLabel = "Dev: Start Teleportation",
                    defaultDesc = "Teleport to new location"
                };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {

        }
    }
}
