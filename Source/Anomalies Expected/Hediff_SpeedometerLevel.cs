using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Hediff_SpeedometerLevel : Hediff_Level
    {
        public Thing Speedometer;
        public Comp_Speedometer SpeedometerComp => speedometerCompCached ?? (speedometerCompCached = Speedometer.TryGetComp<Comp_Speedometer>());
        private Comp_Speedometer speedometerCompCached;

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
            SpeedometerComp.UnlockedLevel = Mathf.Max(SpeedometerComp.UnlockedLevel, targetLevel + 1);
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
                        if (levelNext <= SpeedometerComp.UnlockedLevel)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.Speedometer.TurnPointer".Translate(levelNext), delegate
                            {
                                SetLevelTo(levelNext);
                                SpeedometerComp.TickNextAction = Find.TickManager.TicksGame + SpeedometerComp.Props.tickPerAction;
                            }));
                        }
                        else if (levelNext == level)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.Speedometer.TurnPointerCurrent".Translate(levelNext), null));
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
            Scribe_References.Look(ref Speedometer, "Speedometer");
        }
    }
}
