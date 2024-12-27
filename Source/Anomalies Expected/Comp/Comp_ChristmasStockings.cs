using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_ChristmasStockings : CompInteractable
    {
        public new CompProperties_ChristmasStockings Props => (CompProperties_ChristmasStockings)props;
        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 1) == 0;

        public int giftAmount;

        //public static readonly CachedTexture selectSkillIcon = new CachedTexture("UI/Icons/SwitchFaction");

        //private SkillDef selectedSkillDef => selectedSkillDefCached ?? (selectedSkillDefCached = DefDatabase<SkillDef>.AllDefs.OrderByDescending((SkillDef sd) => sd.listOrder).FirstOrDefault());
        //private SkillDef selectedSkillDefCached;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                giftAmount = Props.giftAmount;
            }
        }

        public override void PostDeSpawn(Map map)
        {
            Thing gift = ThingMaker.MakeThing(Props.lastGift);
            gift.SetFactionDirect(Faction.OfPlayer);
            GenPlace.TryPlaceThing(gift, parent.Position, map, ThingPlaceMode.Near);
            base.PostDeSpawn(map);
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

        //public override IEnumerable<Gizmo> CompGetGizmosExtra()
        //{
        //    foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        //    {
        //        if (gizmo is Command_Action command_Action)
        //        {
        //            command_Action.hotKey = KeyBindingDefOf.Misc6;
        //        }
        //        yield return gizmo;
        //    }
        //    if (!HideInteraction)
        //    {
        //        yield return new Command_Action
        //        {
        //            action = delegate
        //            {
        //                List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
        //                foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs.OrderByDescending((SkillDef sd) => sd.listOrder))
        //                {
        //                    floatMenuOptions.Add(new FloatMenuOption(skillDef.LabelCap, delegate
        //                    {
        //                        selectedSkillDefCached = skillDef;
        //                    }));
        //                }
        //                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
        //            },
        //            defaultLabel = "AnomaliesExpected.StudyNotepad.selectSkill.Label".Translate(selectedSkillDef?.LabelCap ?? "---"),
        //            defaultDesc = "AnomaliesExpected.StudyNotepad.selectSkill.Desc".Translate(),
        //            icon = selectSkillIcon.Texture
        //        };
        //    }
        //}

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
