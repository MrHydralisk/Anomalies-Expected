using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_FleshbeastCommand : CompAbilityEffect
    {
        public new CompProperties_AbilityFleshbeastCommand Props => (CompProperties_AbilityFleshbeastCommand)props;

        private Pawn pawn => parent.pawn;

        private bool isIgnoreEnemies;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            LocalTargetInfo self = new LocalTargetInfo(pawn);
            List<Pawn> fleshbeasts = Fleshbeasts(Props.isCall ? target : self);
            if (fleshbeasts.NullOrEmpty())
            {
                return;
            }
            foreach (Pawn fleshbeast in fleshbeasts)
            {
                if (Props.isCall)
                {
                    CommandToMove(fleshbeast, self);
                }
                else
                {
                    CommandToMove(fleshbeast, target);
                }
            }
            base.Apply(target, dest);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            if (Props.isCall)
            {
                GenDraw.DrawRadiusRing(target.Cell, Props.gatherRadius);
            }
            else
            {
                GenDraw.DrawRadiusRing(pawn.Position, Props.gatherRadius);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!parent.GizmoDisabled(out _))
            {
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = "AnomaliesExpected.Fleshmass.IsIgnoreEnemies.Label".Translate();
                command_Toggle.defaultDesc = "AnomaliesExpected.Fleshmass.IsIgnoreEnemies.Desc".Translate();
                command_Toggle.isActive = () => isIgnoreEnemies;
                command_Toggle.toggleAction = delegate
                {
                    isIgnoreEnemies = !isIgnoreEnemies;
                };
                command_Toggle.activateSound = SoundDefOf.Tick_Tiny;
                command_Toggle.icon = parent.def.uiIcon;
                command_Toggle.Order = 5f + (((float?)parent.def.category?.displayOrder) ?? 0f) / 100f + (float)parent.def.displayOrder / 1000f + (float)parent.def.level / 10000f;
                yield return command_Toggle;
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!Props.isCall && !target.Cell.Standable(pawn.Map))
            {
                return false;
            }
            if (pawn.Faction != null)
            {
                foreach (IntVec3 item in GenRadial.RadialCellsAround(Props.isCall ? target.Cell : pawn.Position, Props.gatherRadius, true))
                {
                    List<Thing> thingList = item.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if ((thingList[i] is Pawn fleshbeast) && !fleshbeast.DeadOrDowned && fleshbeast.Faction == pawn.Faction && (fleshbeast.kindDef == PawnKindDefOf.Fingerspike || fleshbeast.kindDef == PawnKindDefOf.Trispike || fleshbeast.kindDef == PawnKindDefOf.Toughspike))
                        {
                            return true;
                        }
                    }
                }
            }
            if (throwMessages)
            {
                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AnomaliesExpected.Fleshmass.NothingToCommand".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
            }
            return false;
        }

        private List<Pawn> Fleshbeasts(LocalTargetInfo target)
        {
            List<Pawn> fleshbeasts = new List<Pawn>();
            if (pawn.Faction != null)
            {
                foreach (IntVec3 item in GenRadial.RadialCellsAround(target.Cell, Props.gatherRadius, true))
                {
                    List<Thing> thingList = item.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if ((thingList[i] is Pawn fleshbeast) && fleshbeast.Faction == pawn.Faction && (fleshbeast.kindDef == PawnKindDefOf.Fingerspike || fleshbeast.kindDef == PawnKindDefOf.Trispike || fleshbeast.kindDef == PawnKindDefOf.Toughspike))
                        {
                            fleshbeasts.Add(fleshbeast);
                        }
                    }
                }
            }
            return fleshbeasts;
        }

        private void CommandToMove(Pawn fleshbeast, LocalTargetInfo target)
        {
            Job job = JobMaker.MakeJob(JobDefOf.Goto);
            job.targetA = target;
            if (isIgnoreEnemies)
            {
                job.expiryInterval = 50;
                job.expireRequiresEnemiesNearby = true;
            }
            fleshbeast.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
        }
    }
}
