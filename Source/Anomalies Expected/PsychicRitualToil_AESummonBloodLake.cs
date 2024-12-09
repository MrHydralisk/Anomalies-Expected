using RimWorld;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualToil_AESummonBloodLake : PsychicRitualToil
    {
        private PsychicRitualRoleDef invokerRole;

        protected PsychicRitualToil_AESummonBloodLake()
        {
        }

        public PsychicRitualToil_AESummonBloodLake(PsychicRitualRoleDef invokerRole)
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
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = invoker.Map;
            incidentParms.forced = true;
            Find.Storyteller.incidentQueue.Add(IncidentDefOfLocal.AE_IncidentDef_BloodLakeSpawn, Find.TickManager.TicksGame, incidentParms);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
        }
    }
}
