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

        public TopOnBuilding_Clockwork ClockHandHour => clockHandHourCached ?? (clockHandHourCached = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandHour));
        private TopOnBuilding_Clockwork clockHandHourCached;

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
            GameComponent_AnomaliesExpected.instance.curClockworkObelisk = parent;
            if (Props.fakeSpeedometerResearch.ProgressPercent > 0.1)
            {
                Messages.Message("AnomaliesExpected.ObeliskClockwork.Message.NewObeliskResearchReset".Translate(parent.Label), parent, MessageTypeDefOf.NeutralEvent);
            }
            Find.ResearchManager.ApplyKnowledge(Props.fakeSpeedometerResearch, -Find.ResearchManager.GetKnowledge(Props.fakeSpeedometerResearch), out _);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            GameComponent_AnomaliesExpected.instance.curClockworkObelisk = null;
            if (GameComponent_AnomaliesExpected.instance.isHavingSpeedometer)
            {
                //Log.Message($"Clockwork PostDestroy Mathf.Max({GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck}, {Find.TickManager.TicksGame + 1800000})");
                GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck = Mathf.Max(GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck, Find.TickManager.TicksGame + 20000/*1800000*/);
            }
            else
            {
                //Log.Message($"Clockwork PostDestroy -1");
                GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck = -1;
            }
            base.PostDestroy(mode, previousMap);
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
            float dmg = dinfo.Amount * dinfo.Def.buildingDamageFactor * dinfo.Def.buildingDamageFactorPassable;
            foreach (TopOnBuilding_Clockwork clockHand in topOnBuildings)
            {
                clockHand.Rotate(dmg, true);
            }
            if (ClockHandHour?.isWarmup ?? false)
            {
                absorbed = true;
                return;
            }
            if (parent.HitPoints - dmg <= 0)
            {
                absorbed = true;
                parent.HitPoints = parent.MaxHitPoints;
                Notify_HitPointsExhausted();
            }
            else if (ActivityComp.IsActive)
            {
                float partOfHP = parent.MaxHitPoints * 0.75f;
                if (parent.HitPoints >= partOfHP && parent.HitPoints - dmg < partOfHP)
                {
                    Notify_MidHitPointsPassed();
                    return;
                }
                partOfHP = parent.MaxHitPoints * 0.5f;
                if (parent.HitPoints >= partOfHP && parent.HitPoints - dmg < partOfHP)
                {
                    Notify_MidHitPointsPassed();
                    return;
                }
                partOfHP = parent.MaxHitPoints * 0.25f;
                if (parent.HitPoints >= partOfHP && parent.HitPoints - dmg < partOfHP)
                {
                    Notify_MidHitPointsPassed();
                    return;
                }
            }
        }

        public void Notify_HitPointsExhausted()
        {
            if (ActivityComp.IsActive)
            {
                ActivityComp.EnterPassiveState();
            }
        }

        public void Notify_MidHitPointsPassed()
        {
            if (ClockHandHour != null)
            {
                ClockHandHour.ticksTillFullRotation = 1;
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
            yield return new Command_Action
            {
                defaultLabel = "AnomaliesExpected.ObeliskClockwork.PlaceDecoySpeedometer.Label".Translate(),
                defaultDesc = "AnomaliesExpected.ObeliskClockwork.PlaceDecoySpeedometer.Desc".Translate(),
                action = delegate
                {
                    parent.Destroy();
                },
                Disabled = !Props.fakeSpeedometerResearch.IsFinished,
                disabledReason = "AnomaliesExpected.ObeliskClockwork.PlaceDecoySpeedometer.Disabled".Translate()
            };
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
            Props.EffecterOnActive.SpawnMaintained(parent, parent.Map);
        }

        public void OnPassive()
        {
            TopOnBuilding_Clockwork clockHandActivity = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandActivity);
            if (clockHandActivity != null)
            {
                clockHandActivity.ticksTillFullRotation = clockHandActivity.topOnBuildingStructure.tickPerFullRotation;
            }
            Props.EffecterOnPassive.SpawnMaintained(parent, parent.Map);
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
