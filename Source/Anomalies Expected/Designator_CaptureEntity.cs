using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class Designator_CaptureEntity : Designator
    {
        private List<Building_HoldingPlatform> HoldingPlatforms = new List<Building_HoldingPlatform>();

        public override int DraggableDimensions => 2;

        protected override DesignationDef Designation => DesignationDefOfLocal.AE_CaptureEntity;

        public Designator_CaptureEntity()
        {
            defaultLabel = "CaptureEntity".Translate();
            defaultDesc = "AnomaliesExpected.Designator.CaptureEntity.Desc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Commands/CaptureEntity");
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.Click;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            if (!StudyUtility.HoldingPlatformAvailableOnCurrentMap())
            {
                return "NoHoldingPlatformsAvailable".Translate();
            }
            if (!CaptureEntitiesInCell(c).Any())
            {
                return "AnomaliesExpected.Designator.MessageMustDesignateCapturable".Translate();
            }
            return true;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            CompHoldingPlatformTarget compHoldingPlatformTarget = t.TryGetComp<CompHoldingPlatformTarget>();
            return compHoldingPlatformTarget != null && compHoldingPlatformTarget.StudiedAtHoldingPlatform && !compHoldingPlatformTarget.CurrentlyHeldOnPlatform && compHoldingPlatformTarget.CanBeCaptured && t.Spawned;
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(TutorTagDesignate, cells)))
            {
                return;
            }
            bool somethingSucceeded = false;
            bool flag = false;
            HoldingPlatforms = base.Map.listerBuildings.AllBuildingsColonistOfClass<Building_HoldingPlatform>().Where(b => !b.Occupied).ToList();
            foreach (IntVec3 cell in cells)
            {
                if (CanDesignateCell(cell).Accepted)
                {
                    DesignateSingleCell(cell);
                    somethingSucceeded = true;
                    if (!flag)
                    {
                        flag = ShowWarningForCell(cell);
                    }
                }
            }
            Finalize(somethingSucceeded);
            if (TutorSystem.TutorialMode)
            {
                TutorSystem.Notify_Event(new EventPack(TutorTagDesignate, cells));
            }
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            foreach (Thing thing in CaptureEntitiesInCell(loc))
            {
                DesignateThing(thing);
            }
        }

        public override void DesignateThing(Thing t)
        {
            if (HoldingPlatforms.NullOrEmpty())
            {
                return;
            }
            base.Map.designationManager.RemoveAllDesignationsOn(t);
            base.Map.designationManager.AddDesignation(new Designation(t, Designation));
            CompHoldingPlatformTarget compHoldingPlatformTarget = t.TryGetComp<CompHoldingPlatformTarget>();
            Thing building = GenClosest.ClosestThing_Global_Reachable(t.Position, base.Map, HoldingPlatforms, PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors), 9999f, null, delegate (Thing t)
            {
                CompEntityHolder CompEntityHolder = t.TryGetComp<CompEntityHolder>();
                return (CompEntityHolder != null && CompEntityHolder.ContainmentStrength >= t.GetStatValue(StatDefOf.MinimumContainmentStrength)) ? (CompEntityHolder.ContainmentStrength / Mathf.Max(t.PositionHeld.DistanceTo(t.Position), 1f)) : 0f;
            });
            if (building != null)
            {
                compHoldingPlatformTarget.targetHolder = building;
                HoldingPlatforms.Remove(building as Building_HoldingPlatform);
            }
        }

        private IEnumerable<Thing> CaptureEntitiesInCell(IntVec3 c)
        {
            if (c.Fogged(base.Map))
            {
                yield break;
            }
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (CanDesignateThing(thingList[i]).Accepted)
                {
                    yield return thingList[i];
                }
            }
        }
    }
}
