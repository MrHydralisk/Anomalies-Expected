using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_SpawnThingOnDeath : HediffComp
    {
        public HediffCompProperties_SpawnThingOnDeath Props => (HediffCompProperties_SpawnThingOnDeath)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            if (parent == culprit)
            {
                string label = parent.pawn.Label;
                IntVec3 position = parent.pawn.Corpse.Position;
                Map map = parent.pawn.Corpse.Map;
                Thing thing = ThingMaker.MakeThing(Props.spawnedThingDef);
                parent.pawn.Corpse.Destroy(DestroyMode.KillFinalizeLeavingsOnly);
                CellRect cellRect = CellRect.CenteredOn(position, 3);
                for (int i = 0; i < 15; i++)
                {
                    IntVec3 randomCell = cellRect.RandomCell;
                    if (randomCell.InBounds(map) && GenSight.LineOfSight(randomCell, position, map))
                    {
                        FilthMaker.TryMakeFilth(randomCell, map, (i % 2 == 0) ? ThingDefOf.Filth_Blood : ThingDefOf.Filth_CorpseBile);
                    }
                }
                GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near, null);
                if (!Props.messageText.NullOrEmpty())
                {
                    Messages.Message(Props.messageText.Translate(label), thing, MessageTypeDefOf.ThreatSmall);
                }
                if (!Props.letterLabel.NullOrEmpty() && !Props.letterText.NullOrEmpty())
                {
                    Find.LetterStack.ReceiveLetter(Props.letterLabel.Translate(), Props.letterText.Translate(label), Props.letterDef ?? LetterDefOf.NeutralEvent, thing);
                }
            }            
        }
    }
}
