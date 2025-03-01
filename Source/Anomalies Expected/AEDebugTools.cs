using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public static class AEDebugTools
    {
        [DebugAction("Anomalies Expected", "Void Provocation x#...", false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1300)]
        private static void VoidProvocationMultiple()
        {
            Find.WindowStack.Add(new Dialog_Slider("Void Provocation x{0} times", 1, 10, delegate (int x)
            {
                List<IncidentDef> calledIncidentDefs = new List<IncidentDef>();
                for (int i = 0; i < x; i++)
                {
                    if (!VoidProvocation(ref calledIncidentDefs))
                    {
                        break;
                    }

                }
            }));
        }

        public static bool VoidProvocation(ref List<IncidentDef> calledIncidentDefs)
        {
            Map map = Find.CurrentMap;
            List<IncidentDef> list = new List<IncidentDef>();
            bool flag = false;
            foreach (EntityCategoryDef item in DefDatabase<EntityCategoryDef>.AllDefs.OrderBy((EntityCategoryDef x) => x.listOrder))
            {
                foreach (EntityCodexEntryDef eceDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
                {
                    if (eceDef.category != item || eceDef.provocationIncidents.NullOrEmpty() || eceDef.Discovered)
                    {
                        continue;
                    }
                    foreach (IncidentDef provocationIncident in eceDef.provocationIncidents)
                    {
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(provocationIncident.category, map);
                        incidentParms.bypassStorytellerSettings = true;
                        if (provocationIncident.Worker.CanFireNow(incidentParms))
                        {
                            list.Add(provocationIncident);
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            if (!list.Any())
            {
                foreach (EntityCodexEntryDef eceDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
                {
                    if (eceDef.provocationIncidents.NullOrEmpty())
                    {
                        continue;
                    }
                    foreach (IncidentDef provocationIncident2 in eceDef.provocationIncidents)
                    {
                        IncidentParms incidentParms2 = StorytellerUtility.DefaultParmsNow(provocationIncident2.category, map);
                        incidentParms2.bypassStorytellerSettings = true;
                        if (provocationIncident2.Worker.CanFireNow(incidentParms2))
                        {
                            list.Add(provocationIncident2);
                        }
                    }
                }
            }
            bool flag2;
            for (int i = list.Count() - 1; i >= 0; i--)
            {
                if (calledIncidentDefs.Contains(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
            if (list.TryRandomElement(out var result))
            {
                flag2 = true;
                IncidentParms incidentParms3 = StorytellerUtility.DefaultParmsNow(result.category, map);
                incidentParms3.bypassStorytellerSettings = true;
                Find.Storyteller.incidentQueue.Add(result, Find.TickManager.TicksGame + Mathf.RoundToInt(2500f), incidentParms3);
                calledIncidentDefs.Add(result);
                Log.Message($"DevMode VoidProvocation successfull: {result.LabelCap}");
            }
            else
            {
                flag2 = false;
                Log.Message($"DevMode VoidProvocation unsuccessfull");
            }
            Find.Anomaly.hasPerformedVoidProvocation = true;
            return flag2;
        }
    }
}
