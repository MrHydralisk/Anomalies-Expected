using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_EntityDatabaseAnomaly : ThingComp
    {
        public CompProperties_EntityDatabaseAnomaly Props => (CompProperties_EntityDatabaseAnomaly)props;

        public IncidentDef selectedIncidentDef;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                action = delegate
                {
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    List<IncidentDef> list = new List<IncidentDef>();
                    //Log.Message($"---");
                    foreach (EntityCategoryDef item in DefDatabase<EntityCategoryDef>.AllDefs.OrderBy((EntityCategoryDef x) => x.listOrder))
                    {
                        foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
                        {
                            if (allDef.category != item || allDef.provocationIncidents.NullOrEmpty() || !allDef.Discovered)
                            {
                                //Log.Message($"S {allDef.label} | {allDef.Discovered}");
                                continue;
                            }
                            foreach (IncidentDef provocationIncident in allDef.provocationIncidents)
                            {
                                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(provocationIncident.category, parent.MapHeld);
                                incidentParms.bypassStorytellerSettings = true;
                                //Log.Message($"T {allDef.label} | {provocationIncident.defName} | {provocationIncident.Worker.CanFireNow(incidentParms)}");
                                FloatMenuOption floatMenuOption = new FloatMenuOption($"{allDef.label} | {provocationIncident.defName}", delegate
                                {
                                    selectedIncidentDef = provocationIncident;
                                });
                                if (provocationIncident == selectedIncidentDef)
                                {
                                    floatMenuOption.Label = $"[{floatMenuOption.Label}]";
                                }
                                if (provocationIncident.Worker.CanFireNow(incidentParms))
                                {
                                    list.Add(provocationIncident);
                                }
                                else
                                {
                                    floatMenuOption.Disabled = true;
                                }
                                floatMenuOptions.Add(floatMenuOption);
                            }
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "Dev: Summon now",
                defaultDesc = "Summon pattern"
            };
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                    },
                    defaultLabel = "Dev: ",
                    defaultDesc = ""
                };
            }
        }

        public override void PostExposeData()
        {
            Scribe_Defs.Look(ref selectedIncidentDef, "selectedIncidentDef");
        }

        //public override string CompInspectStringExtra()
        //{
        //    List<string> inspectStrings = new List<string>();
        //    if (isConnected)
        //    {
        //        inspectStrings.Add("AnomaliesExpected.BloodPump.Connected".Translate(Source.LabelCap));
        //        inspectStrings.Add("AnomaliesExpected.BloodPump.TimeTillSpawn".Translate(TickTillNextSpawn.ToStringTicksToPeriod()));
        //    }
        //    else
        //    {
        //        inspectStrings.Add("AnomaliesExpected.BloodPump.NotConnected".Translate());
        //    }
        //    return String.Join("\n", inspectStrings);
        //}
    }
}
