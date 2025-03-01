using RimWorld;
using System.Collections.Generic;
using UnityEngine;
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
                char[] chars = owner.LabelShortCap.ToCharArray();
                List<int> indexToCensor = new List<int>();
                for (int i = 0; i < chars.Length; i++)
                {
                    indexToCensor.Add(i);
                }
                int amountToCensor = Mathf.RoundToInt(chars.Length * Props.chanceToCensor);
                foreach (int i in indexToCensor.TakeRandom(amountToCensor))
                {
                    chars[i] = Props.symbolsToCensor.RandomElement();
                }
                PawnLabel = new string(chars);
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

        public override void PostExposeData()
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
