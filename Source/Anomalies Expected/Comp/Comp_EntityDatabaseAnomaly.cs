﻿using RimWorld;
using System;
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

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public IncidentDef selectedIncidentDef;
        private float activityOnPassive;
        public bool isActivatedOnce;

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

        public Texture2D UIIcon
        {
            get
            {
                if (!(activateTex != null))
                {
                    return activateTex = ContentFinder<Texture2D>.Get(Props.activateTexPath);
                }
                return activateTex;
            }
        }
        private Texture2D activateTex;

        public IEnumerable<AEEntityIncidents> entityIncidentsAvailable => entityIncidents.Where((AEEntityIncidents aeei) => aeei.entityCodexEntryDef.Discovered);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            entityIncidentsCached = new List<AEEntityIncidents>();
            foreach (EntityCodexEntryDef entityCodexEntryDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
            {
                AEEntityIncidents entityIncidents = new AEEntityIncidents(entityCodexEntryDef);
                if (!entityIncidents.isCannotBeProvoked && !Props.entityCodexEntryDefsBlacklist.Contains(entityCodexEntryDef))
                {
                    entityIncidentsCached.Add(entityIncidents);
                }
            }
            if (!entityIncidentsCached.Any((AEEntityIncidents aeei) => aeei.incidentDefs.Contains(selectedIncidentDef)))
            {
                selectedIncidentDef = null;
            }
            if (StudyUnlocks.isStudyNoteManualUnlocked(0))
            {
                isActivatedOnce = true;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if ((StudyUnlocks?.NextIndex ?? 2) >= 2)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Find.WindowStack.Add(new Dialog_AEEntityDatabaseAnomaly(this));
                    },
                    defaultLabel = "AnomaliesExpected.EntityDatabaseAnomaly.Dialog.Label".Translate(),
                    defaultDesc = "AnomaliesExpected.EntityDatabaseAnomaly.Dialog.Desc".Translate(),
                    icon = UIIcon
                };
            }
        }

        public override void PostExposeData()
        {
            Scribe_Defs.Look(ref selectedIncidentDef, "selectedIncidentDef");
            Scribe_Values.Look(ref isActivatedOnce, "isActivatedOnce", false);
        }

        public void OnActivityActivated()
        {
            List<IncidentDef> list = new List<IncidentDef>();
            foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
            {
                if (allDef.provocationIncidents.NullOrEmpty() || !allDef.Discovered)
                {
                    continue;
                }
                foreach (IncidentDef provocationIncident in allDef.provocationIncidents)
                {
                    IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(provocationIncident.category, parent.MapHeld);
                    incidentParms.bypassStorytellerSettings = true;
                    if (provocationIncident.Worker.ChanceFactorNow(incidentParms.target) > 0 && provocationIncident.Worker.CanFireNow(incidentParms))
                    {
                        list.Add(provocationIncident);
                    }
                }
            }
            MessageTypeDef messageTypeDef = MessageTypeDefOf.NeutralEvent;
            int signalStrength = 0;
            IncidentDef incidentToActivate = list.RandomElementByWeight((IncidentDef id) => id.category == IncidentCategoryDefOf.ThreatBig ? 1f : id.category == IncidentCategoryDefOf.ThreatSmall ? 0.5f : 0.25f);
            if (incidentToActivate != null)
            {
                if (incidentToActivate.category == IncidentCategoryDefOf.ThreatBig)
                {
                    activityOnPassive = 0;
                    messageTypeDef = MessageTypeDefOf.ThreatBig;
                    signalStrength = 3;
                }
                else if (incidentToActivate.category == IncidentCategoryDefOf.ThreatSmall)
                {
                    activityOnPassive = 0.3f;
                    messageTypeDef = MessageTypeDefOf.ThreatSmall;
                    signalStrength = 2;
                }
                else
                {
                    activityOnPassive = 0.6f;
                    messageTypeDef = MessageTypeDefOf.NegativeEvent;
                    signalStrength = 1;
                }
                IncidentParms incidentParms2 = StorytellerUtility.DefaultParmsNow(incidentToActivate.category, parent.MapHeld);
                incidentParms2.bypassStorytellerSettings = true;
                Find.Storyteller.incidentQueue.Add(incidentToActivate, Find.TickManager.TicksGame + Mathf.RoundToInt(Props.incidentActiveDelayHoursRange.RandomInRange * 2500f), incidentParms2);
            }
            else
            {
                activityOnPassive = 0.9f;
                messageTypeDef = MessageTypeDefOf.NeutralEvent;
                signalStrength = 0;
            }
            Messages.Message("AnomaliesExpected.EntityDatabaseAnomaly.Active".Translate(parent.LabelCap, signalStrength).RawText, parent, messageTypeDef);
            if (!isActivatedOnce)
            {
                StudyUnlocks.UnlockStudyNoteManual(0);
                isActivatedOnce = true;
            }
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
            if ((StudyUnlocks?.NextIndex ?? 2) >= 2)
            {
                inspectStrings.Add("AnomaliesExpected.EntityDatabaseAnomaly.Selected".Translate(selectedIncidentDef?.LabelCap ?? "---"));
            }
            return String.Join("\n", inspectStrings);
        }
    }
}
