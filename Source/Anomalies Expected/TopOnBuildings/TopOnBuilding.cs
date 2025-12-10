using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding : IExposable
    {
        public TopOnBuildingStructure topOnBuildingStructure;
        public TopOnBuildingStructureTypes type;

        private float curRotationInt;

        public static readonly int InitialRotation = -90;

        public float ticksTillFullRotation;
        public float ticksTillWarmup;
        public float ticksTillAwaken;

        public bool isAwaken;

        public virtual float ticksFullRotationPerTick => 1;
        public virtual float ticksWarmupPerTick => 1;
        public bool isWarmup => ticksTillWarmup > 0;

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

        public TopOnBuilding()
        {
            curRotationInt = InitialRotation;
        }

        public TopOnBuilding(TopOnBuildingStructure TopOnBuildingStructure) : this()
        {
            topOnBuildingStructure = TopOnBuildingStructure;
            type = topOnBuildingStructure.type;
            ticksTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
            ticksTillAwaken = topOnBuildingStructure.ticksTillAwaken;
        }

        public virtual void Tick()
        {
            if (isAwaken)
            {
                if (ticksTillFullRotation > 0)
                {
                    ticksTillFullRotation -= ticksFullRotationPerTick;
                    CurRotation = 360 * (1 - ticksTillFullRotation / topOnBuildingStructure.tickPerFullRotation) + InitialRotation;
                    if (ticksTillFullRotation <= 0)
                    {
                        OnTimerEnd();
                    }
                }
                if (isWarmup)
                {
                    ticksTillWarmup -= ticksWarmupPerTick;
                    if (ticksTillWarmup <= 0)
                    {
                        OnWarmupEnd();
                    }
                }
            }
            else
            {
                if (ticksTillAwaken > 0)
                {
                    ticksTillAwaken -= 1;
                    if (ticksTillAwaken <= 0)
                    {
                        Awaken();
                    }
                }
            }
        }

        public void Awaken(bool enable = true)
        {
            if (!isAwaken && enable)
            {
                OnAwaken();
            }
            isAwaken = enable;
        }

        public virtual void OnAwaken()
        {

        }

        public virtual void OnTimerEnd()
        {
            if (topOnBuildingStructure.tickPerFullRotation > -1)
            {
                ticksTillWarmup = topOnBuildingStructure.tickPerWarmup;
            }
            else
            {
                ticksTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
            }
        }

        public virtual void OnWarmupEnd()
        {
            ticksTillFullRotation = topOnBuildingStructure.tickPerFullRotation;
        }

        public void DrawAt(Vector3 drawLoc, float altIncVectMult = 0)
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

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref type, "type");
            Scribe_Values.Look(ref curRotationInt, "curRotationInt", 0);
            Scribe_Values.Look(ref ticksTillFullRotation, "ticksTillFullRotation", 0);
            Scribe_Values.Look(ref ticksTillWarmup, "ticksTillWarmup", 0);
            Scribe_Values.Look(ref ticksTillAwaken, "ticksTillAwaken", 0);
            Scribe_Values.Look(ref isAwaken, "isAwaken", false);
        }
    }
}
