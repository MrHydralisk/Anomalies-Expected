using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class EntityCodexEntryDefExtension : DefModExtension
    {
        [MustTranslate]
        public string groupName;
        public List<string> groupMembersEntityCodexDefs = new List<string>();
        public List<string> groupMembersThingDefs = new List<string>();
    }
}
