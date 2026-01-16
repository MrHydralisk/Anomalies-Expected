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
        public CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;
        public CompActivity ActivityComp => activityInt ?? (activityInt = parent.TryGetComp<CompActivity>());
        private CompActivity activityInt;

        public List<TopOnBuilding_Clockwork> topOnBuildings;

        public TopOnBuilding_Clockwork ClockHandHour => clockHandHourCached ?? (clockHandHourCached = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandHour));
        private TopOnBuilding_Clockwork clockHandHourCached;

        private Sustainer passiveSustainer;
        private Sustainer activeSustainer;

        private static readonly SimpleCurve SustainerActivityCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.25f),
            new CurvePoint(0.5f, 0.5f),
            new CurvePoint(1f, 1f)
        };

        private bool isDeactivated = false;
        private int invThresholds;

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
            if (!isDeactivated)
            {
                GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck = Find.TickManager.TicksGame + 20000;
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
            if (passiveSustainer == null || passiveSustainer.Ended)
            {
                passiveSustainer = Props.SoundPassive.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
            }
            passiveSustainer.info.volumeFactor = SustainerActivityCurve.Evaluate(ActivityComp.ActivityLevel);
            passiveSustainer.Maintain();
            if (ActivityComp.IsActive)
            {
                if (activeSustainer == null || activeSustainer.Ended)
                {
                    activeSustainer = Props.SoundActive.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
                }
                activeSustainer.info.volumeFactor = 0.25f * (1 + invThresholds);
                activeSustainer.Maintain();
            }
            else
            {
                activeSustainer?.End();
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
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (ActivityComp.IsActive)
            {
                if (parent.HitPoints <= parent.MaxHitPoints * 0.25f * invThresholds)
                {
                    Notify_MidHitPointsPassed();
                    return;
                }
            }
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
        }

        public void Notify_HitPointsExhausted()
        {
            if (ActivityComp.IsActive)
            {
                StudyUnlocks.UnlockStudyNoteManual(4);
                ActivityComp.EnterPassiveState();
            }
        }

        public void Notify_MidHitPointsPassed()
        {
            invThresholds = Mathf.Max(0, invThresholds - 1);
            if (ClockHandHour != null)
            {
                ClockHandHour.ticksTillFullRotation = 1;
            }
        }

        public Thing GetSpeedometer(ThingDef speedometerDef)
        {
            Thing speedometer = null;
            foreach (IntVec3 iv3 in GenAdjFast.AdjacentCells8Way(parent.Position, parent.Rotation, parent.def.Size))
            {
                speedometer = parent.Map.thingGrid.ThingsListAtFast(iv3).FirstOrDefault((Thing t) => t.def == speedometerDef);
                if (speedometer != null)
                {
                    break;
                }
            }
            return speedometer;
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
                defaultLabel = "AnomaliesExpected.ObeliskClockwork.PlaceSpeedometer.Label".Translate(),
                defaultDesc = "AnomaliesExpected.ObeliskClockwork.PlaceSpeedometer.Desc".Translate(),
                action = delegate
                {
                    isDeactivated = true;
                    //Log.Message($"Clockwork PostDestroy -1");
                    GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck = -1;
                    GetSpeedometer(Props.SpeedometerDef).Destroy();
                    parent.Destroy();
                },
                activateSound = SoundDefOf.Tick_Tiny,
                icon = Props.SpeedometerDef.uiIcon,
                Disabled = GetSpeedometer(Props.SpeedometerDef) == null,
                disabledReason = "AnomaliesExpected.ObeliskClockwork.PlaceSpeedometer.Disabled".Translate(Props.SpeedometerDef.label)
            };
            yield return new Command_Action
            {
                defaultLabel = "AnomaliesExpected.ObeliskClockwork.PlaceDecoySpeedometer.Label".Translate(),
                defaultDesc = "AnomaliesExpected.ObeliskClockwork.PlaceDecoySpeedometer.Desc".Translate(),
                action = delegate
                {
                    isDeactivated = true;
                    //Log.Message($"Clockwork PostDestroy Mathf.Max({GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck}, {Find.TickManager.TicksGame + 1800000})");
                    GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck = Mathf.Max(GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck, Find.TickManager.TicksGame + 20000/*1800000*/);
                    GetSpeedometer(Props.DecoySpeedometerDef).Destroy();
                    parent.Destroy();
                },
                activateSound = SoundDefOf.Tick_Tiny,
                icon = Props.DecoySpeedometerDef.uiIcon,
                Disabled = !Props.fakeSpeedometerResearch.IsFinished || GetSpeedometer(Props.DecoySpeedometerDef) == null,
                disabledReason = Props.fakeSpeedometerResearch.IsFinished ? "AnomaliesExpected.ObeliskClockwork.PlaceSpeedometer.Disabled".Translate(Props.DecoySpeedometerDef.label) : "AnomaliesExpected.ObeliskClockwork.PlaceDecoySpeedometer.Disabled".Translate()
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
                if (ActivityComp.IsActive)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            Notify_HitPointsExhausted();
                        },
                        defaultLabel = "Dev: Toggle Active State",
                        defaultDesc = "Toggle Active State to Passive"
                    };
                }
                else
                {
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
                        defaultDesc = "Toggle Active State to Active"
                    };
                }
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            isDeactivated = true;
            GameComponent_AnomaliesExpected.instance.tickToSpawnClockworkCheck = -1;
            foreach (Hediff hediff in caster.health.hediffSet.hediffs)
            {
                if (!(hediff is IThingHolder thingHolder))
                {
                    continue;
                }
                ThingOwner thingOwner = thingHolder.GetDirectlyHeldThings();
                if (thingOwner.NullOrEmpty())
                {
                    continue;
                }
                if (thingOwner.FirstOrDefault().def == ThingDefOfLocal.AE_Speedometer)
                {
                    thingOwner.ClearAndDestroyContents();
                }
            }
            StudyUnlocks.UnlockStudyNoteManual(5);
            Find.TickManager.Pause();
            ThingWithComps parts = ThingMaker.MakeThing(Props.ClockworkPartDef) as ThingWithComps;
            CompAEStudyUnlocks compAEStudyUnlocks = parts.GetComp<CompAEStudyUnlocks>();
            if (compAEStudyUnlocks != null)
            {
                foreach (ChoiceLetter letter in StudyUnlocks.Letters)
                {
                    compAEStudyUnlocks.AddStudyNoteLetter(letter);
                }
            }
            bool isPlaced = GenPlace.TryPlaceThing(parts, caster.PositionHeld, caster.MapHeld, ThingPlaceMode.Near, null);
            Find.LetterStack.ReceiveLetter("AnomaliesExpected.ObeliskClockwork.Clockhand.Label".Translate(), "AnomaliesExpected.ObeliskClockwork.Clockhand.Text".Translate(), LetterDefOf.NeutralEvent, isPlaced ? parts : caster);
            caster.Destroy();
            parent.Destroy();
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
            if (!result.Accepted)
            {
                return result;
            }
            if (activateBy != null)
            {
                if (!activateBy.health.hediffSet.hediffs.Any((Hediff h) => h is IThingHolder thingHolder && thingHolder.GetDirectlyHeldThings().Any((Thing t) => t.def == Props.SpeedometerDef)))
                {
                    return "MissingMaterials".Translate(Props.SpeedometerDef.label);
                }
            }
            return true;
        }

        public void OnActivityActivated()
        {
            parent.HitPoints = parent.MaxHitPoints;
            Props.EffecterOnActive.SpawnMaintained(parent, parent.Map);
            Props.SoundOnActive?.PlayOneShotOnCamera();
            StudyUnlocks.UnlockStudyNoteManual(3);
            invThresholds = 3;
        }

        public void OnPassive()
        {
            TopOnBuilding_Clockwork clockHandActivity = topOnBuildings.FirstOrDefault((TopOnBuilding_Clockwork tob) => tob.type == TopOnBuildingStructureTypes.ClockHandActivity);
            if (clockHandActivity != null)
            {
                clockHandActivity.ticksTillFullRotation = clockHandActivity.topOnBuildingStructure.tickPerFullRotation;
            }
            Props.EffecterOnPassive.SpawnMaintained(parent, parent.Map);
            Props.SoundOnPassive?.PlayOneShotOnCamera();
            invThresholds = 0;
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
            Scribe_Values.Look(ref invThresholds, "invThresholds", 0);
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
