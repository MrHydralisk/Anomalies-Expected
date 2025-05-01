using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentWorker_DeployFromQuest : IncidentWorker_AE
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (def.questScriptDef != null)
            {
                if (!def.questScriptDef.CanRun(parms.points))
                {
                    return false;
                }
            }
            else if (parms.questScriptDef != null && !parms.questScriptDef.CanRun(parms.points))
            {
                return false;
            }
            return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoSuspended.Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            QuestScriptDef questScriptDef = def.questScriptDef ?? parms.questScriptDef ?? NaturalRandomQuestChooser.ChooseNaturalRandomQuest(parms.points, parms.target);
            if (questScriptDef == null)
            {
                return false;
            }
            parms.questScriptDef = questScriptDef;
            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questScriptDef, parms.points);
            if (!quest.hidden && questScriptDef.sendAvailableLetter)
            {
                QuestUtility.SendLetterQuestAvailable(quest);
            }
            return true;
        }
    }
}
