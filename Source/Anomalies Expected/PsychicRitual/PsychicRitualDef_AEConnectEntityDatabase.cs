using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualDef_AEConnectEntityDatabase : PsychicRitualDef_VoidProvocation
    {
        public ThingDef EntityDatabaseAnomalyDef;
        private Comp_EntityDatabaseAnomaly EntityDatabaseComp;

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
            list.Replace(list.FirstOrDefault((PsychicRitualToil prt) => prt is PsychicRitualToil_VoidProvocation), new PsychicRitualToil_AEConnectEntityDatabase(InvokerRole, psychicShockChanceFromQualityCurve)
            {
                selectedIncidentDef = EntityDatabaseComp.selectedIncidentDef
            });
            return list;
        }

        public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
        {
            foreach (string item in base.BlockingIssues(assignments, map))
            {
                yield return item;
            }
            CompAffectedByFacilities compAffectedByFacilities = (assignments.Target.Thing as ThingWithComps).GetComp<CompAffectedByFacilities>();
            Thing EntityDatabaseAnomaly = compAffectedByFacilities.LinkedFacilitiesListForReading.FirstOrDefault((Thing t) => t.def == EntityDatabaseAnomalyDef);
            EntityDatabaseComp = EntityDatabaseAnomaly?.TryGetComp<Comp_EntityDatabaseAnomaly>();
            if (EntityDatabaseComp == null || EntityDatabaseComp.parent.DestroyedOrNull())
            {
                yield return "AnomaliesExpected.EntityDatabaseAnomaly.Ritual.MissingEntityDatabase".Translate();
            }
            if (EntityDatabaseComp.selectedIncidentDef == null)
            {
                yield return "AnomaliesExpected.EntityDatabaseAnomaly.Ritual.EntityNotSelected".Translate();
            }
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(EntityDatabaseComp.selectedIncidentDef.category, map);
            incidentParms.bypassStorytellerSettings = true;
            if (EntityDatabaseComp.selectedIncidentDef.Worker.ChanceFactorNow(incidentParms.target) <= 0 || !EntityDatabaseComp.selectedIncidentDef.Worker.CanFireNow(incidentParms))
            {
                yield return "AnomaliesExpected.EntityDatabaseAnomaly.Ritual.AlreadyProvoked".Translate();
            }
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            string desc = outcomeDescription.Formatted(psychicShockChanceFromQualityCurve.Evaluate(qualityRange.min).ToStringPercent());
            if (EntityDatabaseComp != null)
            {
                desc += "AnomaliesExpected.EntityDatabaseAnomaly.Ritual.OutputDesc".Translate(EntityDatabaseComp.entityIncidents.FirstOrDefault((AEEntityIncidents aeei) => aeei.entityCodexEntryDef.provocationIncidents?.Any((IncidentDef id) => id == EntityDatabaseComp.selectedIncidentDef) ?? false)?.entityCodexEntryDef.LabelCap ?? "---");
            }
            return desc;
        }
    }
}
