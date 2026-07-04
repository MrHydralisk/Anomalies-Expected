using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class HediffComp_BrokenStatue : HediffComp_ObservingStage, IThingHolder
    {
        public HediffCompProperties_BrokenStatue Props => (HediffCompProperties_BrokenStatue)props;

        private ThingOwner<Thing> innerContainer;
        public ThingWithComps BrokenStatue => innerContainer.InnerListForReading.FirstOrDefault() as ThingWithComps;
        public Comp_BrokenStatue BrokenStatueComp => BrokenStatueCompCached ?? (BrokenStatueCompCached = BrokenStatue.TryGetComp<Comp_BrokenStatue>());
        private Comp_BrokenStatue BrokenStatueCompCached;

        public int countWandering;

        IThingHolder IThingHolder.ParentHolder => Pawn;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public HediffComp_BrokenStatue()
        {
            innerContainer = new ThingOwner<Thing>(this, LookMode.Deep, removeContentsIfDestroyed: false);
        }

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
            if (BrokenStatue.DestroyedOrNull())
            {
                innerContainer.TryAdd(ThingMaker.MakeThing(Props.spawnedThingDef));
                Pawn.TryGetComp<CompAEStudyUnlocks>()?.SetParentThing(BrokenStatue);
            }
        }

        public DamageWorker.DamageResult BreakSpine(Pawn target)
        {
            Props.soundSpineBreak.PlayOneShot(target);
            return target.TakeDamage(new DamageInfo(Props.damageDef, Props.spineDMG, Props.spinePenetration, instigator: Pawn, hitPart: target.health.hediffSet.GetBodyPartRecord(BodyPartDefOfLocal.Spine)));
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            Transform();
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
            if (victim.RaceProps.Humanlike && !victim.IsSubhuman)
            {
                BrokenStatueComp.StudyUnlocks.UnlockStudyNoteManual(1, victim);
            }
        }

        public void Transform()
        {
            IntVec3 intVec3 = Pawn.PositionHeld;
            Map map = Pawn.MapHeld;
            if (!Props.soundTransform.NullOrUndefined())
            {
                Props.soundTransform.PlayOneShot(SoundInfo.InMap(Pawn));
            }
            if (Pawn.Dead)
            {
                ResurrectionUtility.TryResurrect(Pawn);
            }
            Pawn.DeSpawn();
            BrokenStatueComp.GetDirectlyHeldThings().TryAdd(Pawn);
            ThingWithComps brokenStatue = BrokenStatue;
            Comp_BrokenStatue brokenStatueComp = BrokenStatueComp;
            if (!innerContainer.TryDrop(BrokenStatue, intVec3, map, ThingPlaceMode.Near, out var lastResultingThing))
            {
                if (!RCellFinder.TryFindRandomCellNearWith(intVec3, (IntVec3 c) => c.Standable(map), map, out var result, 1))
                {
                    Debug.LogError("Could not drop BrokenStatue pile!");
                }
                lastResultingThing = GenSpawn.Spawn(innerContainer.Take(BrokenStatue), result, map);
            }
            GetDirectlyHeldThings().RemoveAll(t => t == brokenStatue);
            brokenStatueComp.OnTransform();
        }

        public bool Wander(bool isFoundTarget = false)
        {
            if (isFoundTarget)
            {
                countWandering = 0;
                return false;
            }
            countWandering++;
            if (countWandering > 10)
            {
                countWandering = 0;
                Transform();
                return true;
            }
            return false;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && innerContainer.removeContentsIfDestroyed)
            {
                innerContainer.removeContentsIfDestroyed = false;
            }
            Scribe_Values.Look(ref countWandering, "countWandering", 0);
        }
    }
}
