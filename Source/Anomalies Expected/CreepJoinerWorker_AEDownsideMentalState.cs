using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CreepJoinerWorker_AEDownsideMentalState : BaseCreepJoinerWorker
    {
        public CreepJoinerDefExtension Ext => Tracker.downside.GetModExtension<CreepJoinerDefExtension>();

        public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
        {
            base.Pawn.mindState.mentalStateHandler.TryStartMentalState(Ext.mentalStateDef, forced: true);
        }
    }
}
