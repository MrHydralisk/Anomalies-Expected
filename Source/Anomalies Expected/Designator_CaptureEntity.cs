using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{

    public class Designator_CaptureEntity : Designator
    {
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

        public override void DesignateSingleCell(IntVec3 loc)
        {
            foreach (Thing thing in CaptureEntitiesInCell(loc))
            {
                DesignateThing(thing);
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            CompHoldingPlatformTarget compHoldingPlatformTarget = t.TryGetComp<CompHoldingPlatformTarget>();
            return compHoldingPlatformTarget != null && compHoldingPlatformTarget.StudiedAtHoldingPlatform && !compHoldingPlatformTarget.CurrentlyHeldOnPlatform && compHoldingPlatformTarget.CanBeCaptured && t.Spawned;
        }

        public override void DesignateThing(Thing t)
        {
            base.Map.designationManager.RemoveAllDesignationsOn(t);
            base.Map.designationManager.AddDesignation(new Designation(t, Designation));
            StudyUtility.TargetHoldingPlatformForEntity(null, t);
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
