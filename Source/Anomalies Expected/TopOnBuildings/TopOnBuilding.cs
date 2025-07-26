using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding
    {
        protected TopOnBuildingStructure topOnBuildingStructure;

        private float curRotationInt;

        public static readonly int InitialRotation = -90;

        public float ticksTillFullRotation;
        public float ticksTillWarmup;

        public virtual float ticksFullRotationPerTick => 1;
        public virtual float ticksWarmupPerTick => 1;

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

        public TopOnBuilding(TopOnBuildingStructure TopOnBuildingStructure)
        {
            topOnBuildingStructure = TopOnBuildingStructure;
            curRotationInt = InitialRotation;
            ticksTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
        }

        public virtual void Tick()
        {
            if (ticksTillFullRotation > 0)
            {
                ticksTillFullRotation -= ticksFullRotationPerTick;
                CurRotation = 360 * (1 - ticksTillFullRotation / topOnBuildingStructure.tickPerFullRotation) + InitialRotation;
                if (ticksTillFullRotation <= 0)
                {
                    OnTimerEnd();
                    if (topOnBuildingStructure.tickPerFullRotation > -1)
                    {
                        ticksTillWarmup = topOnBuildingStructure.tickPerWarmup;
                    }
                    else
                    {
                        ticksTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
                    }
                }
            }
            else if (ticksTillWarmup > 0)
            {
                ticksTillWarmup -= ticksWarmupPerTick;
                if (ticksTillWarmup <= 0)
                {
                    OnWarmupEnd();
                    ticksTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
                }
            }
        }

        public virtual void OnTimerEnd()
        {

        }

        public virtual void OnWarmupEnd()
        {

        }

        public void DrawAt(Vector3 drawLoc, float altIncVectMult = 1)
        {
            Vector3 v = topOnBuildingStructure.drawOffset;
            float turretTopDrawSize = topOnBuildingStructure.drawSize;
            Vector3 pos = drawLoc + Altitudes.AltIncVect * altIncVectMult + v;
            Quaternion q = (CurRotation + topOnBuildingStructure.drawRotation).ToQuat();
            Graphics.DrawMesh(matrix: Matrix4x4.TRS(pos, q, new Vector3(turretTopDrawSize, 1f, turretTopDrawSize)), mesh: MeshPool.plane10, material: topOnBuildingStructure.Material, layer: 0);

            if (ticksTillWarmup > 0 /*&& Find.Selector.IsSelected(stanceTracker.pawn)*/)
            {
                GenDraw.DrawAimPieRaw(pos, CurRotation - InitialRotation, Mathf.CeilToInt(90 * ticksTillWarmup / topOnBuildingStructure.tickPerWarmup));
            }
        }
    }
}
