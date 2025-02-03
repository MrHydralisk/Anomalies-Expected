using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class AEEntityEntry : IExposable
    {
        public string AnomalyLabel;
        public string AnomalyDesc;
        public string EntityEntryGroupTag;

        public ThingDef ThingDef;
        public EntityCodexEntryDef EntityCodexEntryDef;

        public AEEntityClass EntityClass;
        public bool isEntityClassUnknown = true;
        public bool isEntityClassApocalypse;

        protected List<ChoiceLetter> letters = new List<ChoiceLetter>();


        public void ExposeData()
        {
            Scribe_Values.Look(ref AnomalyLabel, "AnomalyLabel");
            Scribe_Values.Look(ref AnomalyDesc, "AnomalyDesc");
            Scribe_Values.Look(ref EntityEntryGroupTag, "EntityEntryGroupTag");
            Scribe_Values.Look(ref EntityClass, "EntityClass");
            Scribe_Values.Look(ref isEntityClassUnknown, "isEntityClassUnknown", true);
            Scribe_Values.Look(ref isEntityClassApocalypse, "isEntityClassApocalypse");
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