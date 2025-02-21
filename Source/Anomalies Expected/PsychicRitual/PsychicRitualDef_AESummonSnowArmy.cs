using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualDef_AESummonSnowArmy : PsychicRitualDef_InvocationCircle
    {
        public SimpleCurve CombatPointsMultFromQualityCurve;
        public GameConditionDef PreventedByGameConditionDef;
        public IncidentDef IncidentDefBasic;
        public IncidentDef IncidentDefAdvanced;
        public SoundDef soundDef;

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
            list.Add(new PsychicRitualToil_AESummonSnowArmy(InvokerRole));
            return list;
        }

        public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
        {
            foreach (string item in base.BlockingIssues(assignments, map))
            {
                yield return item;
            }
            if (PreventedByGameConditionDef != null && map.gameConditionManager.GetActiveCondition(PreventedByGameConditionDef) != null)
            {
                yield return "AnomaliesExpected.SnowArmy.Ritual.AlreadyExists".Translate();
            }
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add(outcomeDescription.Formatted());
            inspectStrings.Add("AnomaliesExpected.SnowArmy.Ritual.OutputDesc".Translate(CombatPointsMultFromQualityCurve.Evaluate(qualityRange.min).ToStringPercent()));
            return String.Join("\n", inspectStrings);
        }
    }
}
