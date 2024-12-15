using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_StudyNotepad : CompInteractable
    {
        public new CompProperties_StudyNotepad Props => (CompProperties_StudyNotepad)props;
        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 1) == 0;

        public static readonly CachedTexture selectSkillIcon = new CachedTexture("UI/Icons/SwitchFaction");

        private SkillDef selectedSkillDef => selectedSkillDefCached ?? (selectedSkillDefCached = DefDatabase<SkillDef>.AllDefs.OrderByDescending((SkillDef sd) => sd.listOrder).FirstOrDefault());
        private SkillDef selectedSkillDefCached;

        private void GiveKnowledge(Pawn pawn)
        {
            SkillRecord skillRecord;
            if (pawn.skills != null && (skillRecord = pawn.skills.skills.FirstOrDefault((SkillRecord sr) => sr.def == selectedSkillDef)) != null)
            {
                skillRecord.Learn(Props.learnXPPerProgressPoint * Props.RequiredResearchDef?.ProgressReal ?? 20, direct: true);
                if (Props.RequiredResearchDef != null)
                {
                    Find.ResearchManager.ApplyKnowledge(Props.RequiredResearchDef, -Find.ResearchManager.GetKnowledge(Props.RequiredResearchDef), out _);
                    Find.ResearchManager.AddProgress(Props.RequiredResearchDef, -Find.ResearchManager.GetProgress(Props.RequiredResearchDef));
                }
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
            if (!HideInteraction)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs.OrderByDescending((SkillDef sd) => sd.listOrder))
                        {
                            floatMenuOptions.Add(new FloatMenuOption(skillDef.LabelCap, delegate
                            {
                                selectedSkillDefCached = skillDef;
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "AnomaliesExpected.StudyNotepad.selectSkill.Label".Translate(selectedSkillDef?.LabelCap ?? "---"),
                    defaultDesc = "AnomaliesExpected.StudyNotepad.selectSkill.Desc".Translate(),
                    icon = selectSkillIcon.Texture
                };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            GiveKnowledge(caster);
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
            if (!result.Accepted)
            {
                return result;
            }
            if (Props.RequiredResearchDef != null && Props.RequiredResearchDef.ProgressReal < 1)
            {
                return "AnomaliesExpected.StudyNotepad.NoResearchProgress".Translate(Props.RequiredResearchDef.LabelCap);
            }
            if (activateBy != null)
            {
                if (selectedSkillDef == null)
                {
                    Log.Warning("selectedSkillDef is null for Comp_StudyNotepad");
                }
                else if (activateBy.skills?.skills.FirstOrDefault((SkillRecord sr) => sr.def == selectedSkillDef)?.TotallyDisabled ?? true)
                {
                    return "AbilityDisabledNoCapacity".Translate(activateBy, selectedSkillDef.LabelCap);
                }
            }
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref selectedSkillDefCached, "selectedSkillDefCached");
        }
    }
}
