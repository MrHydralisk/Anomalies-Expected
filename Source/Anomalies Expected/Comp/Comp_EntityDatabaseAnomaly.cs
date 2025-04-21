using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking.Types;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_EntityDatabaseAnomaly : ThingComp, IActivity
    {
        public CompProperties_EntityDatabaseAnomaly Props => (CompProperties_EntityDatabaseAnomaly)props;

        public IncidentDef selectedIncidentDef;
        private float activityOnPassive;

        public List<AEEntityIncidents> entityIncidents
        {
            get
            {
                if (lastUpToDateTick != Find.TickManager.TicksGame)
                {
                    foreach (AEEntityIncidents entityIncidents in entityIncidentsCached)
                    {
                        entityIncidents.UpToDate(parent.MapHeld);
                    }
                    lastUpToDateTick = Find.TickManager.TicksGame;
                }
                return entityIncidentsCached;
            }
        }
        private List<AEEntityIncidents> entityIncidentsCached = new List<AEEntityIncidents>();
        private int lastUpToDateTick;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            foreach (EntityCodexEntryDef entityCodexEntryDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
            {
                entityIncidentsCached.Add(new AEEntityIncidents(entityCodexEntryDef, parent.MapHeld));
            }
        }

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
                    Find.WindowStack.Add(new Dialog_AEEntityDatabaseAnomaly(this));
                },
                defaultLabel = "AnomaliesExpected.EntityDatabaseAnomaly.Dialog.Label".Translate(),
                defaultDesc = "AnomaliesExpected.EntityDatabaseAnomaly.Dialog.Desc".Translate()
            };
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

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add("AnomaliesExpected.EntityDatabaseAnomaly.Selected".Translate(entityIncidents.FirstOrDefault((AEEntityIncidents aeei) => aeei.entityCodexEntryDef.provocationIncidents?.Any((IncidentDef id) => id == selectedIncidentDef) ?? false)?.entityCodexEntryDef.LabelCap ?? "---"));
            return String.Join("\n", inspectStrings);
        }
    }
}
