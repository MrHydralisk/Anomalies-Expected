using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualDef_AEAskingProvider : PsychicRitualDef_InvocationCircle
    {
        public ThingDef ProviderBoxDef;
        private Building_Storage ProviderBox;

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
            list.Add(new PsychicRitualToil_AEAskingProvider(InvokerRole) { ProviderBox = ProviderBox});
            return list;
        }

        public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
        {
            foreach (string item in base.BlockingIssues(assignments, map))
            {
                yield return item;
            }
            CompAffectedByFacilities compAffectedByFacilities = (assignments.Target.Thing as ThingWithComps).GetComp<CompAffectedByFacilities>();
            ProviderBox = compAffectedByFacilities.LinkedFacilitiesListForReading.FirstOrDefault((Thing t) => t.def == ProviderBoxDef) as Building_Storage;
            if (ProviderBox == null)
            {
                yield return "AnomaliesExpected.ProviderScripture.Ritual.MissingTheBox".Translate();
            }
            if (ProviderBox.slotGroup.HeldThings.EnumerableNullOrEmpty())
            {
                yield return "AnomaliesExpected.ProviderScripture.Ritual.BoxEmpty".Translate();
            }
        }
    }
}
