using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    [DefOf]
    public static class SoundDefOfLocal
    {
        public static SoundDef Psycast_Skip_Exit => ModsConfig.RoyaltyActive ? SoundDefOf.Psycast_Skip_Exit : SoundDefOf.Psycast_Skip_Entry;
        public static SoundDef Explosion_Stun;
    }
}