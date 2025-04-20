using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_EntityDatabaseAnomaly : ThingComp, IActivity
    {
        public CompProperties_EntityDatabaseAnomaly Props => (CompProperties_EntityDatabaseAnomaly)props;

        public IncidentDef selectedIncidentDef;
        private float activityOnPassive;

        //public List<AEEntityIncidents> entityIncidents
        //{
        //    get
        //    {
        //        if (entityIncidentsCached.NullOrEmpty() || entityIncidentsCachedTick != Find.TickManager.TicksGame)
        //        {
        //            entityIncidentsCached = new List<AEEntityIncidents>();
        //            foreach (EntityCodexEntryDef entityCodexEntryDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
        //            {
        //                if (!entityCodexEntryDef.provocationIncidents.NullOrEmpty())
        //                {
        //                    entityIncidentsCached.Add(new AEEntityIncidents(entityCodexEntryDef, entityCodexEntryDef.))
        //                }

        //            }
        //            entityIncidentsCachedTick = Find.TickManager.TicksGame;
        //        }
        //        return entityIncidentsCached;
        //    }
        //}
        //private List<AEEntityIncidents> entityIncidentsCached = new List<AEEntityIncidents>();
        //private int entityIncidentsCachedTick;

        //public void

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
                        Find.WindowStack.Add(new Dialog_AEEntityDatabaseAnomaly(this));
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

        public void OnActivityActivated()
        {
            List<IncidentDef> list = new List<IncidentDef>();
            foreach (EntityCategoryDef item in DefDatabase<EntityCategoryDef>.AllDefs.OrderBy((EntityCategoryDef x) => x.listOrder))
            {
                foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
                {
                    if (allDef.category != item || allDef.provocationIncidents.NullOrEmpty() || !allDef.Discovered)
                    {
                        continue;
                    }
                    foreach (IncidentDef provocationIncident in allDef.provocationIncidents)
                    {
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(provocationIncident.category, parent.MapHeld);
                        incidentParms.bypassStorytellerSettings = true;
                        if (provocationIncident.Worker.CanFireNow(incidentParms))
                        {
                            list.Add(provocationIncident);
                        }
                    }
                }
            }
            MessageTypeDef messageTypeDef = MessageTypeDefOf.ThreatBig;
            int signalStrength = 3;
            IncidentDef incidentToActivate = list.Where((IncidentDef id) => id.category == IncidentCategoryDefOf.ThreatBig).RandomElement();
            if (incidentToActivate == null)
            {
                incidentToActivate = list.Where((IncidentDef id) => id.category == IncidentCategoryDefOf.ThreatSmall).RandomElement();
                if (incidentToActivate == null)
                {
                    incidentToActivate = list.RandomElement();
                    if (incidentToActivate == null)
                    {
                        activityOnPassive = 0.9f;
                        messageTypeDef = MessageTypeDefOf.NeutralEvent;
                        signalStrength = 0;
                        parent.GetComp<CompActivity>()?.EnterPassiveState();
                        return;
                    }
                    else
                    {
                        activityOnPassive = 0.6f;
                        messageTypeDef = MessageTypeDefOf.NegativeEvent;
                        signalStrength = 1;
                    }
                }
                else
                {
                    activityOnPassive = 0.3f;
                    messageTypeDef = MessageTypeDefOf.ThreatSmall;
                    signalStrength = 2;
                }
            }
            else
            {
                activityOnPassive = 0;
            }
            Messages.Message("AnomaliesExpected.EntityDatabaseAnomaly.Active".Translate(parent.LabelCap, signalStrength).RawText, parent, messageTypeDef);
            IncidentParms incidentParms2 = StorytellerUtility.DefaultParmsNow(incidentToActivate.category, parent.MapHeld);
            incidentParms2.bypassStorytellerSettings = true;
            Find.Storyteller.incidentQueue.Add(incidentToActivate, Find.TickManager.TicksGame + Mathf.RoundToInt(Props.incidentActiveDelayHoursRange.RandomInRange * 2500f), incidentParms2);
            if (!Props.soundActivate.NullOrUndefined())
            {
                Props.soundActivate.PlayOneShot(SoundInfo.InMap(parent));
            }
            if (Props.fleckOnUsed != null)
            {
                FleckMaker.AttachedOverlay(this.parent, Props.fleckOnUsed, Vector3.zero, Props.fleckOnUsedScale * (1 + signalStrength));
            }
            parent.GetComp<CompActivity>()?.EnterPassiveState();
        }

        public void OnPassive()
        {
            parent.GetComp<CompActivity>()?.AdjustActivity(activityOnPassive);
        }

        public bool ShouldGoPassive()
        {
            return false;
        }

        public bool CanBeSuppressed()
        {
            return true;
        }

        public bool CanActivate()
        {
            return true;
        }

        public string ActivityTooltipExtra()
        {
            return null;
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
