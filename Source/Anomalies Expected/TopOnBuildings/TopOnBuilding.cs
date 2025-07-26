using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding
    {
        private TopOnBuildingStructure topOnBuildingStructure;

        private float curRotationInt;

        public static readonly int InitialRotation = -90;

        public float tickTillFullRotation;
        public float tickTillWarmup;

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
            tickTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
        }

        public void Tick()
        {
            if (tickTillFullRotation > 0)
            {
                tickTillFullRotation -= 1;
                curRotationInt = 360 * (1 - tickTillFullRotation / topOnBuildingStructure.tickPerFullRotation) + InitialRotation;
                if (tickTillFullRotation <= 0)
                {
                    OnTimerEnd();
                    if (topOnBuildingStructure.tickPerFullRotation > 0)
                    {
                        tickTillWarmup = topOnBuildingStructure.tickPerWarmup;
                    }
                    else
                    {
                        tickTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
                    }
                }
            }
            else if (tickTillWarmup > 0)
            {
                tickTillWarmup -= 1;
                if (tickTillWarmup <= 0)
                {
                    OnWarmupEnd();
                    tickTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
                }
            }
        }

        public virtual void OnTimerEnd()
        {

        }

        public virtual void OnWarmupEnd()
        {

        }

        public void DrawAt(Vector3 drawLoc)
        {
            Vector3 v = topOnBuildingStructure.drawOffset;
            float turretTopDrawSize = topOnBuildingStructure.drawSize;
            Vector3 pos = drawLoc + Altitudes.AltIncVect + v;
            Quaternion q = CurRotation.ToQuat();
            Graphics.DrawMesh(matrix: Matrix4x4.TRS(pos, q, new Vector3(turretTopDrawSize, 1f, turretTopDrawSize)), mesh: MeshPool.plane10, material: topOnBuildingStructure.Material, layer: 0);

            if (tickTillWarmup > 0 /*&& Find.Selector.IsSelected(stanceTracker.pawn)*/)
            {
                GenDraw.DrawAimPieRaw(pos, CurRotation - InitialRotation, Mathf.CeilToInt(90 * tickTillWarmup / topOnBuildingStructure.tickPerWarmup));
            }
        }
    }
}
