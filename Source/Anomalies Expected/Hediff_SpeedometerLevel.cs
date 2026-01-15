using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Hediff_SpeedometerLevel : Hediff_Level, IThingHolder
    {
        private ThingOwner<Thing> innerContainer;
        public Thing Speedometer => innerContainer.InnerListForReading.FirstOrDefault() as ThingWithComps;
        public Comp_Speedometer SpeedometerComp => speedometerCompCached ?? (speedometerCompCached = Speedometer.TryGetComp<Comp_Speedometer>());
        private Comp_Speedometer speedometerCompCached;

        public Texture ActiveTex => activeTexCached ?? (UpdateActiveTexture());
        private Texture activeTexCached;

        public Texture2D DropTex => dropTexCached ?? (dropTexCached = ContentFinder<Texture2D>.Get(SpeedometerComp.Props.dropTexPath));
        private Texture2D dropTexCached;

        private int maxLevel;

        public bool isDestabilize
        {
            get
            {
                if (pawn.IsHashIntervalTick(2500))
                {
                    isDestabilizeCached = pawn.ageTracker.AgeBiologicalYears >= pawn.RaceProps.lifeExpectancy || pawn.ageTracker.BiologicalTicksPerTick <= 0 || pawn.IsMutant || (pawn.genes != null && SpeedometerComp.Props.destabilizationGeneDefs.Any((GeneDef gd) => pawn.genes.HasActiveGene(gd)));
                }
                return isDestabilizeCached;
            }
        }

        IThingHolder IThingHolder.ParentHolder => pawn;

        private bool isDestabilizeCached;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public Hediff_SpeedometerLevel()
        {
            innerContainer = new ThingOwner<Thing>(this, LookMode.Deep, removeContentsIfDestroyed: false);
        }

        public void AddSpeedometer(ThingWithComps speedometer)
        {
            speedometer.DeSpawn();
            innerContainer.TryAdd(speedometer);
        }

        public Texture UpdateActiveTexture()
        {
            activeTexCached = Speedometer.Graphic.MatSingleFor(Speedometer).mainTexture;
            return activeTexCached;
        }

        public override void Tick()
        {
            base.Tick();
            int ageTicks = (int)Mathf.Pow(2, level) - 1;
            if (ageTicks > 0)
            {
                pawn.ageTracker.AgeTickMothballed(ageTicks);
            }
            if (isDestabilize)
            {
                HealthUtility.AdjustSeverity(pawn, SpeedometerComp.Props.ChronoDestabilizationHediffDef, (Mathf.Pow(2, level) + 1) / 60000);
                SpeedometerComp.Notify_Destabilized(pawn);
            }
            if (pawn.IsHashIntervalTick(60000) && pawn.IsSlaveOfColony && !SlaveRebellionUtility.IsRebelling(pawn) && Rand.Chance(level / 6f))
            {
                SlaveRebellionUtility.StartSlaveRebellion(pawn, true);
            }
        }

        public override void PostRemoved()
        {
            if (!Speedometer.DestroyedOrNull())
            {
                SpeedometerComp.CooldownsStart(maxLevel);
                if (!innerContainer.TryDrop(Speedometer, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near, out var lastResultingThing))
                {
                    if (!RCellFinder.TryFindRandomCellNearWith(pawn.PositionHeld, (IntVec3 c) => c.Standable(pawn.MapHeld), pawn.MapHeld, out var result, 1))
                    {
                        Debug.LogError("Could not drop Speedometer!");
                    }
                    lastResultingThing = GenSpawn.Spawn(innerContainer.Take(Speedometer), result, pawn.MapHeld);
                }
            }
            base.PostRemoved();
        }

        public override void SetLevelTo(int targetLevel)
        {
            base.SetLevelTo(targetLevel);
            maxLevel = Mathf.Max(maxLevel, targetLevel);
            SpeedometerComp.TryUpdateUnlockedLevel(targetLevel, pawn);
            UpdateActiveTexture();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            pawn.health.RemoveHediff(this);
            GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, Mathf.Pow(1.8f, level), SpeedometerComp.Props.deathDamageDef, Speedometer, damAmount: SpeedometerComp.Props.deathDamagePerLevel * level);
            SpeedometerComp.Notify_Exploded(pawn);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            int actionTicksLeft = SpeedometerComp.TickNextAction - Find.TickManager.TicksGame;
            string TakeOff = actionTicksLeft > 0 ? "TakeOffEarly" : "TakeOff";
            yield return new Command_Action
            {
                action = delegate
                {
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    for (int i = 1; i <= def.maxSeverity; i++)
                    {
                        int levelNext = 0;
                        levelNext = i;
                        if (levelNext == level)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.Speedometer.TurnPointerCurrent".Translate(levelNext), null));
                        }
                        else if (levelNext <= SpeedometerComp.UnlockedLevel)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.Speedometer.TurnPointer".Translate(levelNext), delegate
                            {
                                SetLevelTo(levelNext);
                                SpeedometerComp.TickNextAction = Find.TickManager.TicksGame + SpeedometerComp.Props.tickPerAction * levelNext;
                                SpeedometerComp.TickCanRemove = Find.TickManager.TicksGame + SpeedometerComp.Props.tickPerAction;
                                if (!SpeedometerComp.Props.soundActivate.NullOrUndefined())
                                {
                                    SpeedometerComp.Props.soundActivate.PlayOneShot(SoundInfo.InMap(pawn));
                                }
                                if (SpeedometerComp.Props.fleckOnUsed != null)
                                {
                                    FleckMaker.AttachedOverlay(pawn, SpeedometerComp.Props.fleckOnUsed, Vector3.zero, SpeedometerComp.Props.fleckOnUsedScale);
                                }
                            }));
                        }
                        else
                        {
                            floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.Speedometer.TurnPointerDisabled".Translate(levelNext), null));
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "AnomaliesExpected.Speedometer.TurnPointer.Label".Translate(),
                defaultDesc = "AnomaliesExpected.Speedometer.TurnPointer.Desc".Translate(),
                icon = ActiveTex,
                Disabled = actionTicksLeft > 0,
                disabledReason = SpeedometerComp.Props.onCooldownString + " (" + "DurationLeft".Translate(actionTicksLeft.ToStringTicksToPeriod()) + ")"
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation($"AnomaliesExpected.Speedometer.{TakeOff}".Translate(Speedometer?.Label ?? "---", actionTicksLeft.ToStringTicksToPeriod()), delegate
                    {
                        pawn.health.RemoveHediff(this);
                        if (actionTicksLeft > 0)
                        {
                            SpeedometerComp.Notify_RemovedEarly(pawn, actionTicksLeft);
                        }
                    }));
                },
                defaultLabel = $"AnomaliesExpected.Speedometer.{TakeOff}.Label".Translate(),
                defaultDesc = $"AnomaliesExpected.Speedometer.{TakeOff}.Desc".Translate(Speedometer?.Label ?? "---", actionTicksLeft.ToStringTicksToPeriod()),
                icon = DropTex,
                Disabled = SpeedometerComp.TickCanRemove > Find.TickManager.TicksGame,
                disabledReason = SpeedometerComp.Props.onCooldownString + " (" + "DurationLeft".Translate((SpeedometerComp.TickCanRemove - Find.TickManager.TicksGame).ToStringTicksToPeriod()) + ")"
            };
            if (DebugSettings.ShowDevGizmos)
            {
                if (SpeedometerComp.TickNextAction > Find.TickManager.TicksGame)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Reset cooldown",
                        action = delegate
                        {
                            SpeedometerComp.TickNextAction = 0;
                        }
                    };
                }
                if (SpeedometerComp.TickCanRemove > Find.TickManager.TicksGame)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Reset early remove cooldown",
                        action = delegate
                        {
                            SpeedometerComp.TickCanRemove = 0;
                        }
                    };
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && innerContainer.removeContentsIfDestroyed)
            {
                innerContainer.removeContentsIfDestroyed = false;
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars) // Backcompat
            {
                Thing speedometer = null;
                Scribe_Deep.Look(ref speedometer, "Speedometer");
                if (speedometer != null)
                {
                    if (innerContainer == null)
                    {
                        innerContainer = new ThingOwner<Thing>(this, LookMode.Deep, removeContentsIfDestroyed: false);
                    }
                    innerContainer.TryAdd(speedometer);
                }
            }
            Scribe_Values.Look(ref maxLevel, "maxLevel");
        }
    }
}
