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

        private void GiveKnowledge(Pawn pawn)
        {
            if (pawn.skills != null && pawn.skills.skills.Where((SkillRecord x) => !x.TotallyDisabled).TryRandomElement(out var result))
            {
                result.Learn(Props.learnXPPerProgressPoint * Props.RequiredResearchDef?.ProgressReal ?? 20, direct: true);
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
                return "AlreadyHasHediff".Translate(Props.RequiredResearchDef.LabelCap);
            }
            return true;
        }
    }
}
