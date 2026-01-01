using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<TopOnBuilding_Clockwork> topOnBuildings;

        public override void PostPostMake()
        {
            base.PostPostMake();
            topOnBuildings = new List<TopOnBuilding_Clockwork>();
            foreach (TopOnBuildingStructure structure in Props.topOnBuildingStructures)
            {
                TopOnBuilding_Clockwork topOnBuilding = (TopOnBuilding_Clockwork)Activator.CreateInstance(structure.topOnBuildingClass, new object[] { structure });
                topOnBuilding.Obelisk_Clockwork = parent;
                topOnBuildings.Add(topOnBuilding);
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            foreach (TopOnBuilding_Clockwork topOnBuilding in topOnBuildings)
            {
                topOnBuilding.DrawAt(drawLoc);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            foreach (TopOnBuilding_Clockwork topOnBuilding in topOnBuildings)
            {
                topOnBuilding.Tick();
            }
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (parent.HitPoints - dinfo.Amount <= 0)
            {
                absorbed = true;
                parent.HitPoints = parent.MaxHitPoints;
                Notify_HitPointsExhausted();
            }
        }

        public void Notify_HitPointsExhausted()
        {
            if (ActivityComp.IsActive)
            {
                ActivityComp.EnterPassiveState();
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
                        TopOnBuilding_Clockwork clockHandSecond = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandSecond);
                        if (clockHandSecond != null)
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
                        TopOnBuilding_Clockwork clockHandMinute = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandMinute);
                        if (clockHandMinute != null)
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
                        TopOnBuilding_Clockwork clockHandHour = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandHour);
                        if (clockHandHour != null)
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
                        TopOnBuilding_Clockwork clockHandActivity = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandActivity);
                        if (clockHandActivity != null)
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
            parent.HitPoints = parent.MaxHitPoints;
            SoundDefOf.VoidNode_Explode.PlayOneShotOnCamera();
            EffecterDefOf.VoidNodeDisrupted.SpawnMaintained(parent, parent.Map);
        }

        public void OnPassive()
        {
            EffecterDefOf.VoidStructureActivated.Spawn(parent, parent.Map);
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
            Scribe_Collections.Look(ref topOnBuildings, "topOnBuildings", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (TopOnBuilding topOnBuilding in topOnBuildings)
                {
                    TopOnBuildingStructure structure = Props.topOnBuildingStructures.FirstOrDefault((TopOnBuildingStructure tobs) => topOnBuilding != null && tobs.type == topOnBuilding.type);
                    if (structure != null)
                    {
                        topOnBuilding.topOnBuildingStructure = structure;
                    }
                    else
                    {
                        Log.Error($"Couldn't find TopOnBuildingStructure for {(topOnBuilding == null ? "---" : topOnBuilding.type)}");
                    }
                }
            }
        }
    }
}
