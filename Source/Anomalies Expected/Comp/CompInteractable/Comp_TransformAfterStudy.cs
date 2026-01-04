using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class Comp_TransformAfterStudy : CompInteractable
    {
        public new CompProperties_TransformAfterStudy Props => (CompProperties_TransformAfterStudy)props;
        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 1) == 0;

        protected override void OnInteracted(Pawn caster)
        {
            ThingWithComps transformedThing = ThingMaker.MakeThing(Props.transformedDef) as ThingWithComps;
            if (Props.transformedDef.CanHaveFaction)
            {
                transformedThing.SetFactionDirect(Faction.OfPlayer);
            }
            CompAEStudyUnlocks compAEStudyUnlocks = transformedThing.GetComp<CompAEStudyUnlocks>();
            if (compAEStudyUnlocks != null)
            {
                foreach (ChoiceLetter letter in StudyUnlocks.Letters)
                {
                    compAEStudyUnlocks.AddStudyNoteLetter(letter);
                }
            }
            GenPlace.TryPlaceThing(transformedThing, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near);
            parent.Destroy();
        }
    }
}
