using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class CompObelisk_Clockwork : CompInteractable, IActivity
    {
        public new CompPropertiesObelisk_Clockwork Props => (CompPropertiesObelisk_Clockwork)props;
        public CompActivity ActivityComp => activityInt ?? (activityInt = parent.TryGetComp<CompActivity>());
        private CompActivity activityInt;

        public Dictionary<TopOnBuildingStructureTypes, TopOnBuilding_Clockwork> topOnBuildings;

        public override void PostPostMake()
        {
            base.PostPostMake();
            topOnBuildings = new Dictionary<TopOnBuildingStructureTypes, TopOnBuilding_Clockwork>();
            foreach (TopOnBuildingStructure structure in Props.topOnBuildingStructures)
            {
                TopOnBuilding_Clockwork topOnBuilding = (TopOnBuilding_Clockwork)Activator.CreateInstance(structure.topOnBuildingClass, new object[] { structure });
                topOnBuilding.compObelisk_Clockwork = this;
                topOnBuildings.Add(structure.type, topOnBuilding);
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
                            clockHandSecond.ticksTillFullRotation = 1;
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
                            clockHandMinute.ticksTillFullRotation = 1;
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
                            clockHandHour.ticksTillFullRotation = 1;
                        }
                    },
                    defaultLabel = "Dev: Start Teleportation",
                    defaultDesc = "Teleport to new location"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        if (topOnBuildings.TryGetValue(TopOnBuildingStructureTypes.ClockHandActivity, out TopOnBuilding_Clockwork clockHandActivity))
                        {
                            clockHandActivity.ticksTillFullRotation = 1;
                        }
                    },
                    defaultLabel = "Dev: Toggle Active State",
                    defaultDesc = "Toggle Active State"
                };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {

        }

        public void OnActivityActivated()
        {
            Log.Message($"OnActivityActivated");
        }

        public void OnPassive()
        {
            Log.Message($"OnPassive");
        }

        public bool ShouldGoPassive()
        {
            return false;
        }

        public bool CanBeSuppressed()
        {
            return true;
        }

        public bool CanActivate()
        {
            return false;
        }

        public string ActivityTooltipExtra()
        {
            return null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

        }
    }
}
