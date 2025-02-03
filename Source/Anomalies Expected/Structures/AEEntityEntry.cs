using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class AEEntityEntry : IExposable
    {
        public string AnomalyLabel;
        public string AnomalyDesc;

        public ThingDef ThingDef;
        public EntityCodexEntryDef EntityCodexEntryDef;
        public string categoryLabelCap => EntityCodexEntryDef?.category?.LabelCap ?? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();
        public string threatClassString => $"AnomaliesExpected.EntityDataBase.ThreatClass.{ThreatClass}".Translate();
        public string groupTag => GroupTag.NullOrEmpty()? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate() : GroupTag;
        public string GroupTag;

        public int ThreatClass = -1;

        protected List<ChoiceLetter> letters = new List<ChoiceLetter>();


        public void ExposeData()
        {
            Scribe_Values.Look(ref AnomalyLabel, "AnomalyLabel");
            Scribe_Values.Look(ref AnomalyDesc, "AnomalyDesc");
            Scribe_Values.Look(ref ThreatClass, "ThreatClass", -1);
            Scribe_Values.Look(ref GroupTag, "GroupTag");
            Scribe_Defs.Look(ref EntityCodexEntryDef, "EntityCodexEntryDef");
            Scribe_Defs.Look(ref ThingDef, "ThingDef");
            Scribe_Collections.Look(ref letters, "letters", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (letters == null)
                {
                    letters = new List<ChoiceLetter>();
                }
            }
        }
    }
}