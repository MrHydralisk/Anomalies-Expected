using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Hediff_SpeedometerLevel : Hediff_Level
    {
        public Thing Speedometer;
        public Comp_Speedometer SpeedometerComp => speedometerCompCached ?? (speedometerCompCached = Speedometer.TryGetComp<Comp_Speedometer>());
        private Comp_Speedometer speedometerCompCached;

        public Texture ActiveTex => activeTexCached ?? (UpdateActiveTexture());
        private Texture activeTexCached;

        public Texture2D DropTex => dropTexCached ?? (dropTexCached = ContentFinder<Texture2D>.Get(SpeedometerComp.Props.dropTexPath));
        private Texture2D dropTexCached;

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
            if (pawn.ageTracker.AgeBiologicalYears >= pawn.RaceProps.lifeExpectancy || pawn.ageTracker.BiologicalTicksPerTick <= 0)
            {
                HealthUtility.AdjustSeverity(pawn, SpeedometerComp.Props.ChronoDestabilizationHediffDef, (Mathf.Pow(2, level) + 1) / 60000);
                SpeedometerComp.Notify_Destabilized(pawn);
            }
        }

        public override void PostRemoved()
        {
            if (!Speedometer.DestroyedOrNull())
            {
                GenPlace.TryPlaceThing(Speedometer, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near, null);
                SpeedometerComp.CooldownsStart();
            }
            base.PostRemoved();
        }

        public override void SetLevelTo(int targetLevel)
        {
            base.SetLevelTo(targetLevel);
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
            Scribe_Deep.Look(ref Speedometer, "Speedometer");
        }
    }
}
