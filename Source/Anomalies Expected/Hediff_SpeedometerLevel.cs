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
        }

        public override void PostRemoved()
        {
            if (!Speedometer.DestroyedOrNull())
            {
                GenPlace.TryPlaceThing(Speedometer, pawn.Position, pawn.Map, ThingPlaceMode.Near, null);
                SpeedometerComp.CooldownsStart();
            }
            base.PostRemoved();
        }

        public override void SetLevelTo(int targetLevel)
        {
            base.SetLevelTo(targetLevel);
            SpeedometerComp.TryUpdateUnlockedLevel(targetLevel);
            UpdateActiveTexture();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
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
                                SpeedometerComp.TickNextAction = Find.TickManager.TicksGame + SpeedometerComp.Props.tickPerAction;
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
                Disabled = SpeedometerComp.TickNextAction > Find.TickManager.TicksGame,
                disabledReason = SpeedometerComp.Props.onCooldownString + " (" + "DurationLeft".Translate((SpeedometerComp.TickNextAction - Find.TickManager.TicksGame).ToStringTicksToPeriod()) + ")"
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("AnomaliesExpected.Speedometer.TakeOff".Translate(Speedometer?.Label ?? "---"), delegate
                    {
                        pawn.health.RemoveHediff(this);
                    }));
                },
                defaultLabel = "AnomaliesExpected.Speedometer.TakeOff.Label".Translate(),
                defaultDesc = "AnomaliesExpected.Speedometer.TakeOff.Desc".Translate(Speedometer?.Label ?? "---"),
                icon = DropTex,
                Disabled = SpeedometerComp.TickNextAction > Find.TickManager.TicksGame,
                disabledReason = SpeedometerComp.Props.onCooldownString + " (" + "DurationLeft".Translate((SpeedometerComp.TickNextAction - Find.TickManager.TicksGame).ToStringTicksToPeriod()) + ")"
            };
            if (DebugSettings.ShowDevGizmos && SpeedometerComp.TickNextAction > Find.TickManager.TicksGame)
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
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Speedometer, "Speedometer");
        }
    }
}
