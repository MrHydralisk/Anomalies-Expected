using Mono.Unix.Native;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualToil_AEAskingProvider : PsychicRitualToil
    {
        private PsychicRitualRoleDef invokerRole;
        public Building_Storage ProviderBox;

        protected PsychicRitualToil_AEAskingProvider()
        {
        }

        public PsychicRitualToil_AEAskingProvider(PsychicRitualRoleDef invokerRole)
        {
            this.invokerRole = invokerRole;
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            base.Start(psychicRitual, parent);
            Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            psychicRitual.ReleaseAllPawnsAndBuildings();
            if (pawn != null)
            {
                ApplyOutcome(psychicRitual, pawn);
            }
        }

        private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker)
        {
            Thing inputThing = ProviderBox.slotGroup.HeldThings.FirstOrDefault();
            Thing resource = ThingMaker.MakeThing(inputThing.def);
            resource.stackCount = resource.def.stackLimit;
            GenPlace.TryPlaceThing(resource, psychicRitual.assignments.Target.Cell, psychicRitual.assignments.Target.Map, ThingPlaceMode.Near, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
        }
    }
}
