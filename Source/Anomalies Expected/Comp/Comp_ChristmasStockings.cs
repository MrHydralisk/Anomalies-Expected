using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace AnomaliesExpected
{
    public class Comp_ChristmasStockings : CompInteractable
    {
        public new CompProperties_ChristmasStockings Props => (CompProperties_ChristmasStockings)props;
        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 1) == 0;

        public int giftAmount;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                giftAmount = Props.giftAmount;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            ThingWithComps gift = ThingMaker.MakeThing(Props.lastGift) as ThingWithComps;
            gift.SetFactionDirect(Faction.OfPlayer);
            CompAEStudyUnlocks compAEStudyUnlocks = gift.GetComp<CompAEStudyUnlocks>();
            if (compAEStudyUnlocks != null)
            {
                foreach (ChoiceLetter letter in StudyUnlocks.Letters)
                {
                    compAEStudyUnlocks.AddStudyNoteLetter(letter);
                }
            }
            GenPlace.TryPlaceThing(gift, parent.Position, previousMap, ThingPlaceMode.Near);
            compAEStudyUnlocks.UnlockStudyNoteManual(0);
            base.PostDestroy(mode, previousMap);
        }

        private void TakeGift(Pawn pawn)
        {
            Thought_Memory thought = null;
            if (GoodDeeds(pawn) < 0)
            {
                thought = (Thought_Memory)ThoughtMaker.MakeThought(Props.thoughtDefNegative);
            }
            else
            {
                thought = (Thought_Memory)ThoughtMaker.MakeThought(Props.thoughtDefPositive);
            }
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
            giftAmount--;
            if (giftAmount < 1)
            {
                Comp_CanDestroyedAfterStudy canDestroyedAfterStudy = parent.GetComp<Comp_CanDestroyedAfterStudy>();
                if (canDestroyedAfterStudy != null)
                {
                    canDestroyedAfterStudy.isCanDestroyForced = true;
                    StudyUnlocks.UnlockStudyNoteManual(0);
                }
            }
        }

        private float GoodDeeds(Pawn pawn)
        {
            Pawn_RecordsTracker records = pawn.records;
            if (records == null)
            {
                return 0;
            }
            float goodDeeds = 0;
            goodDeeds += records.GetValue(RecordDefOf.KillsHumanlikes) * Props.goodPerKillsHumanlikes;
            goodDeeds += records.GetValue(RecordDefOf.KillsAnimals) * Props.goodPerKillsAnimals;
            goodDeeds += records.GetValue(RecordDefOf.KillsEntities) * Props.goodPerKillsEntities;
            goodDeeds += records.GetValue(RecordDefOf.KillsMechanoids) * Props.goodPerKillsMechanoids;
            goodDeeds += records.GetValue(RecordDefOf.TimesTendedOther) * Props.goodPerTimesTendedOther;
            //Log.Message($"GoodOrBad {goodDeeds}\n" +
            //    $"KillsHumanlikes {records.GetValue(RecordDefOf.KillsHumanlikes) * Props.goodPerKillsHumanlikes} = {records.GetValue(RecordDefOf.KillsHumanlikes)} * {Props.goodPerKillsHumanlikes}\n" +
            //    $"KillsAnimals {records.GetValue(RecordDefOf.KillsAnimals) * Props.goodPerKillsAnimals} = {records.GetValue(RecordDefOf.KillsAnimals)} * {Props.goodPerKillsAnimals}\n" +
            //    $"KillsEntities {records.GetValue(RecordDefOf.KillsEntities) * Props.goodPerKillsEntities} = {records.GetValue(RecordDefOf.KillsEntities)} * {Props.goodPerKillsEntities}\n" +
            //    $"KillsMechanoids {records.GetValue(RecordDefOf.KillsMechanoids) * Props.goodPerKillsMechanoids} = {records.GetValue(RecordDefOf.KillsMechanoids)} * {Props.goodPerKillsMechanoids}\n" +
            //    $"TimesTendedOther {records.GetValue(RecordDefOf.TimesTendedOther) * Props.goodPerTimesTendedOther} = {records.GetValue(RecordDefOf.TimesTendedOther)} * {Props.goodPerTimesTendedOther}");
            return goodDeeds;
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
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 15", delegate
                        {
                            giftAmount += 15;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 5", delegate
                        {
                            giftAmount += 5;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1", delegate
                        {
                            giftAmount += 1;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1", delegate
                        {
                            giftAmount -= 1;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 5", delegate
                        {
                            giftAmount -= 5;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 15", delegate
                        {
                            giftAmount -= 15;
                        }));
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Change Gift Amount",
                    defaultDesc = $"Change amount of gits: {giftAmount}/{Props.giftAmount}"
                };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            TakeGift(caster);
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
            if (!result.Accepted)
            {
                return result;
            }
            if (giftAmount < 1)
            {
                return "AnomaliesExpected.ChristmasStockings.NoGifts".Translate();
            }
            MemoryThoughtHandler memories;
            if (activateBy != null && (memories = activateBy.needs?.mood?.thoughts?.memories) != null)
            {
                Thought_Memory memory = memories.GetFirstMemoryOfDef(Props.thoughtDefNegative);
                if (memory == null)
                {
                    memory = memories.GetFirstMemoryOfDef(Props.thoughtDefPositive);
                }
                if (memory != null)
                {
                    return "AnomaliesExpected.ChristmasStockings.AlreadyGotGift".Translate(memory.LabelCap);
                }
            }
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref giftAmount, "giftAmount");
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            int study = StudyUnlocks?.NextIndex ?? 1;
            if (study > 0)
            {
                inspectStrings.Add("AnomaliesExpected.ChristmasStockings.GiftsLeft".Translate(giftAmount, Props.giftAmount).RawText);
            }
            inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }
    }
}
