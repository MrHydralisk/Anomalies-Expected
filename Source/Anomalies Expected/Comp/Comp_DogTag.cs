using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_DogTag : CompInteractable
    {
        public new CompProperties_DogTag Props => (CompProperties_DogTag)props;

        public string PawnLabel = "Unknown";
        public string PawnUniqueLoadID = "Unknown";

        public void SetOwner(Pawn owner)
        {
            if (owner != null)
            {
                PawnLabel = owner.LabelShortCap;
                PawnUniqueLoadID = owner.GetUniqueLoadID();
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            if (Props.hediffOnUse != null)
            {
                HealthUtility.AdjustSeverity(caster, Props.hediffOnUse, 1);
            }
            Messages.Message("AnomaliesExpected.DogTag.Interacted".Translate(parent.Label, caster.LabelShortCap).RawText, caster, MessageTypeDefOf.PositiveEvent);
            parent.Destroy();
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
                if (activateBy.GetUniqueLoadID() == PawnUniqueLoadID)
                {
                    return true;
                }
                else
                {
                    return "AnomaliesExpected.DogTag.WrongPawn".Translate(PawnLabel);
                }
            }
            return true;
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

        public void ExposeData()
        {
            Scribe_Values.Look(ref PawnLabel, "PawnLabel", "Unknown");
            Scribe_Values.Look(ref PawnUniqueLoadID, "PawnUniqueLoadID", "Unknown");
        }

        public override string CompInspectStringExtra()
        {
            TaggedString taggedString;
            taggedString += "AnomaliesExpected.DogTag.PawnLabel".Translate(PawnLabel);
            return taggedString.Resolve();
        }
    }
}
