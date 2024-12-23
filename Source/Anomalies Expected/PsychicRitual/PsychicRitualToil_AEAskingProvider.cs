using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualToil_AEAskingProvider : PsychicRitualToil_InvokeHorax
    {
        public Building_Storage ProviderBox;
        private ThingDef inputThingDef;
        private int inputAmount;

        protected PsychicRitualToil_AEAskingProvider() : base()
        {
        }

        public PsychicRitualToil_AEAskingProvider(PsychicRitualRoleDef invokerRole, IEnumerable<IntVec3> invokerPositions, PsychicRitualRoleDef targetRole, IEnumerable<IntVec3> targetPositions, PsychicRitualRoleDef chanterRole, IEnumerable<IntVec3> chanterPositions, PsychicRitualRoleDef defenderRole, IEnumerable<IntVec3> defenderPositions, IngredientCount requiredOffering) :
            base(invokerRole, invokerPositions, targetRole, targetPositions, chanterRole, chanterPositions, defenderRole, defenderPositions, requiredOffering)
        {
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            base.Start(psychicRitual, parent);
            PsychicRitualDef_AEAskingProvider psychicRitualDef = psychicRitual.def as PsychicRitualDef_AEAskingProvider;
            Thing inputThing = ProviderBox.slotGroup.HeldThings.FirstOrDefault();
            inputThingDef = inputThing.def;
            inputAmount = psychicRitualDef.InputAmount(inputThing);
            inputThing.SplitOff(inputAmount).Destroy();
        }

        public override void End(PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
        {
            base.End(psychicRitual, parent, success);
            if (success)
            {
                IntVec3 position = psychicRitual.assignments.Target.Cell;
                Map map = psychicRitual.assignments.Target.Map;
                PsychicRitualDef_AEAskingProvider psychicRitualDef = psychicRitual.def as PsychicRitualDef_AEAskingProvider;
                int outputAmount = Mathf.FloorToInt(inputAmount * psychicRitualDef.MultFromResearch() * psychicRitualDef.MultFromQuality(psychicRitual.PowerPercent));
                Find.ResearchManager.ApplyKnowledge(psychicRitualDef.researchProjectDef, -Find.ResearchManager.GetKnowledge(psychicRitualDef.researchProjectDef), out _);
                while (outputAmount > inputThingDef.stackLimit)
                {
                    Thing thing = ThingMaker.MakeThing(inputThingDef);
                    thing.stackCount = inputThingDef.stackLimit;
                    GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near, null);
                    outputAmount -= inputThingDef.stackLimit;
                }
                if (outputAmount > 0)
                {
                    Thing thing2 = ThingMaker.MakeThing(inputThingDef);
                    thing2.stackCount = outputAmount;
                    GenPlace.TryPlaceThing(thing2, position, map, ThingPlaceMode.Near, null);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ProviderBox, "ProviderBox");
            Scribe_Values.Look(ref inputThingDef, "inputThingDef");
            Scribe_Values.Look(ref inputAmount, "inputValue", 0);
        }
    }
}
