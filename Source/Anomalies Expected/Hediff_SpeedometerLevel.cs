using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Hediff_SpeedometerLevel : Hediff_Level
    {
        public int UnlockedLevel = 1;
        public Thing Speedometer;

        public override void Tick()
        {
            base.Tick();
            int ageTicks = (int)Mathf.Pow(2, level) - 1;
            if (ageTicks > 0)
            {
                pawn.ageTracker.AgeTickMothballed(ageTicks);
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            SetLevelTo(1);
        }

        public override void PostRemoved()
        {
            if (!Speedometer.DestroyedOrNull())
            {
                GenPlace.TryPlaceThing(Speedometer, pawn.Position, pawn.Map, ThingPlaceMode.Near, null);
            }
            base.PostRemoved();
        }

        public override void SetLevelTo(int targetLevel)
        {
            base.SetLevelTo(targetLevel);
            UnlockedLevel = Mathf.Max(UnlockedLevel, targetLevel + 1);
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
                        if (levelNext <= UnlockedLevel)
                        {
                            floatMenuOptions.Add(new FloatMenuOption($"Turn pointer to {levelNext}", delegate
                            {
                                SetLevelTo(levelNext);
                            }));
                        }
                        else
                        {
                            floatMenuOptions.Add(new FloatMenuOption($"Turn pointer to {levelNext} [Disabled]", null));
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "Turn pointer".Translate(),
                defaultDesc = "Rotate screw to turn pointer to a different value".Translate()
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Take off".Translate(), delegate
                    {
                        pawn.health.RemoveHediff(this);
                    }));
                },
                defaultLabel = "Take off".Translate(),
                defaultDesc = "Take off {0} from yourself".Translate(Speedometer?.Label ?? "---")
            };
            if (DebugSettings.ShowDevGizmos/* && inCooldown && CanCooldown*/)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Reset cooldown",
                    action = delegate
                    {
                        //inCooldown = false;
                        //charges = maxCharges;
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
