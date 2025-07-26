using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding
    {
        //private Building_Turret parentTurret;

        private TopOnBuildingStructure topOnBuildingStructure;

        private float curRotationInt;

        //private int ticksUntilIdleTurn;

        //private int idleTurnTicksLeft;

        //private bool idleTurnClockwise;

        //private const float IdleTurnDegreesPerTick = 0.26f;

        //private const int IdleTurnDuration = 140;

        //private const int IdleTurnIntervalMin = 150;

        //private const int IdleTurnIntervalMax = 350;

        public static readonly int ArtworkRotation = -90;

        public float CurRotation
        {
            get
            {
                return curRotationInt;
            }
            set
            {
                curRotationInt = value;
                if (curRotationInt > 360f)
                {
                    curRotationInt -= 360f;
                }
                if (curRotationInt < 0f)
                {
                    curRotationInt += 360f;
                }
            }
        }

        public TopOnBuilding(/*Building_Turret ParentTurret, */TopOnBuildingStructure TopOnBuildingStructure)
        {
            //parentTurret = ParentTurret;
            topOnBuildingStructure = TopOnBuildingStructure;
        }

        //public void ForceFaceTarget(LocalTargetInfo targ)
        //{
        //    if (targ.IsValid)
        //    {
        //        float curRotation = (targ.Cell.ToVector3Shifted() - parentTurret.DrawPos).AngleFlat();
        //        CurRotation = curRotation;
        //    }
        //}

        //public void TurretTopTick()
        //{
        //    LocalTargetInfo currentTarget = parentTurret.CurrentTarget;
        //    if (currentTarget.IsValid)
        //    {
        //        float curRotation = (currentTarget.Cell.ToVector3Shifted() - parentTurret.DrawPos).AngleFlat();
        //        CurRotation = curRotation;
        //        ticksUntilIdleTurn = Rand.RangeInclusive(IdleTurnIntervalMin, IdleTurnIntervalMax);
        //    }
        //    else if (ticksUntilIdleTurn > 0)
        //    {
        //        ticksUntilIdleTurn--;
        //        if (ticksUntilIdleTurn == 0)
        //        {
        //            if (Rand.Value < 0.5f)
        //            {
        //                idleTurnClockwise = true;
        //            }
        //            else
        //            {
        //                idleTurnClockwise = false;
        //            }
        //            idleTurnTicksLeft = IdleTurnDuration;
        //        }
        //    }
        //    else
        //    {
        //        if (idleTurnClockwise)
        //        {
        //            CurRotation += IdleTurnDegreesPerTick;
        //        }
        //        else
        //        {
        //            CurRotation -= IdleTurnDegreesPerTick;
        //        }
        //        idleTurnTicksLeft--;
        //        if (idleTurnTicksLeft <= 0)
        //        {
        //            ticksUntilIdleTurn = Rand.RangeInclusive(IdleTurnIntervalMin, IdleTurnIntervalMax);
        //        }
        //    }
        //}

        public void DrawAt(Vector3 drawLoc, Vector3 recoilDrawOffset, float recoilAngleOffset)
        {
            Vector3 v = topOnBuildingStructure.drawOffset/*.RotatedBy(CurRotation)*/;
            float turretTopDrawSize = topOnBuildingStructure.drawSize;
            v = v.RotatedBy(recoilAngleOffset);
            v += recoilDrawOffset;
            float num = /*parentTurret.CurrentEffectiveVerb?.AimAngleOverride ??*/ CurRotation;
            Vector3 pos = drawLoc + Altitudes.AltIncVect + v;
            Quaternion q = CurRotation.ToQuat();
            Graphics.DrawMesh(matrix: Matrix4x4.TRS(pos, q, new Vector3(turretTopDrawSize, 1f, turretTopDrawSize)), mesh: MeshPool.plane10, material: topOnBuildingStructure.Material, layer: 0);
        }
    }
}
