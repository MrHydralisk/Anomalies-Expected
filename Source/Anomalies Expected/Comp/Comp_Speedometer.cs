using RimWorld;
using System.Collections.Generic;
using Verse;
using static AnomaliesExpected.Comp_BeamTarget;

namespace AnomaliesExpected
{
    public class Comp_Speedometer : CompInteractable
    {
        public new CompProperties_Speedometer Props => (CompProperties_Speedometer)props;
        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 1) == 0;

        public int UnlockedLevel = 1;

        private Hediff GiveHediff(Pawn pawn, HediffDef hediffDef)
        {
            Hediff AddedHediff = HediffMaker.MakeHediff(hediffDef, pawn);
            pawn.health.AddHediff(AddedHediff);
            return AddedHediff;
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
            Hediff_SpeedometerLevel hediff_SpeedometerLevel = GiveHediff(caster, Props.AccelerationHediffDef) as Hediff_SpeedometerLevel;
            hediff_SpeedometerLevel.Speedometer = parent;
            hediff_SpeedometerLevel.SetLevelTo(1);
            parent.DeSpawn();
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
                if (activateBy.health.hediffSet.HasHediff(Props.AccelerationHediffDef))
                {
                    return "AlreadyHasHediff".Translate(Props.AccelerationHediffDef.label);
                }
            }
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref UnlockedLevel, "beamNextCount", 1);
        }
    }
}
