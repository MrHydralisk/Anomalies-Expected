using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_ForbiddenFruit : CompInteractable
    {
        public new CompProperties_Interactable Props => (CompProperties_Interactable)props;
        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 1) == 0;

        private void GiveHediff(Pawn pawn)
        {
            float Severity = 0.01f;
            if (pawn.health.hediffSet.HasHediff(HediffDefOfLocal.Hediff_AEForbiddenFruit))
            {
                Severity = 1f;
            }
            HealthUtility.AdjustSeverity(pawn, HediffDefOfLocal.Hediff_AEForbiddenFruit, Severity);
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
            GiveHediff(caster);
            if (caster.skills != null && caster.skills.skills.Where((SkillRecord x) => !x.TotallyDisabled).TryRandomElement(out var result))
            {
                result.Learn(4800f, direct: true);
            }
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
                if (activateBy.health.hediffSet.HasHediff(HediffDefOfLocal.Hediff_AEForbiddenFruitWithdrawal))
                {
                    return "AlreadyHasHediff".Translate(HediffDefOfLocal.Hediff_AEForbiddenFruitWithdrawal.label);
                }
            }
            return true;
        }
    }
}
