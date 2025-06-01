using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_BirthdayCake : CompInteractable
    {
        public new CompProperties_BirthdayCake Props => (CompProperties_BirthdayCake)props;

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 1) < 1;

        public void CelebrateBirthday(Pawn pawn)
        {
            Pawn_AgeTracker ageTracker = pawn.ageTracker;
            long ticksSinceBirthday = ageTracker.AgeBiologicalTicks - Mathf.FloorToInt(ageTracker.AgeBiologicalYearsFloat) * 3600000L;
            long ticksTillBirthday = Mathf.CeilToInt(ageTracker.AgeBiologicalYearsFloat) * 3600000L - ageTracker.AgeBiologicalTicks;
            if (ticksTillBirthday < ticksSinceBirthday)
            {
                ageTracker.AgeBiologicalTicks = Mathf.CeilToInt(ageTracker.AgeBiologicalYearsFloat) * 3600000L;
            }
            else
            {
                ageTracker.AgeBiologicalTicks = Mathf.FloorToInt(ageTracker.AgeBiologicalYearsFloat) * 3600000L;
            }
            Messages.Message("AnomaliesExpected.BirthdayCake.BirthdayCelebration".Translate(parent.LabelCap, ageTracker.AgeBiologicalYears).RawText, new TargetInfo(parent.Position, parent.Map), MessageTypeDefOf.NeutralEvent);
            StudyUnlocks.UnlockStudyNoteManual(0, pawn);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                if (gizmo is Command_Action command_Action)
                {
                    command_Action.hotKey = KeyBindingDefOf.Misc8;
                }
                yield return gizmo;
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            CelebrateBirthday(caster);
        }
    }
}
