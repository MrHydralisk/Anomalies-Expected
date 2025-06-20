using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Building_AEPocketMapExit : Building_AEMapPortal
    {
        protected static readonly CachedTexture ExitPitGateTex = new CachedTexture("UI/Commands/ExitCave");
        protected override Texture2D EnterTex => EnterPitGateTex.Texture;
        public override string CancelEnterString => "CommandCancelExitPortal".Translate();

        public override void OnEntered(Pawn pawn)
        {
            Notify_ThingAdded(pawn);
        }
    }
}
