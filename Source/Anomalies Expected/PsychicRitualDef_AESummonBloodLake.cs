using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualDef_AESummonBloodLake : PsychicRitualDef_InvocationCircle
    {
        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
            list.Add(new PsychicRitualToil_AESummonBloodLake(InvokerRole));
            return list;
        }

        public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
        {
            foreach (string item in base.BlockingIssues(assignments, map))
            {
                yield return item;
            }
            if (map.listerThings.AllThings.Any((Thing t) => t.def == ThingDefOfLocal.AE_BloodLake || t.def == ThingDefOfLocal.AE_BloodLakeSpawner))
            {
                yield return "AnomaliesExpected.BloodLake.AlreadyExists".Translate();
            }
        }
    }
}
