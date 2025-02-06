using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class EntityCodexEntryDefExtension : DefModExtension
    {
        public string groupName;
        public List<string> groupMembersEntityCodexDefs = new List<string>();
        public List<string> groupMembersThingDefs = new List<string>();
    }
}
