using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_IceThinBody : HediffComp
    {
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            float angle = dinfo.Angle;
            if (angle > 180)
            {
                angle -= 360;
            }
            float angleBounce = Mathf.Min(Mathf.Abs(angle), 180 - Mathf.Abs(angle));
            float percent = angleBounce / 90;
            if (Pawn?.Rotation.IsHorizontal ?? false)
            {
                percent = 1 - percent;
            }
            float mult = Mathf.Lerp(0.1f, 1.5f, percent);
            dinfo.SetAmount(dinfo.Amount * mult);
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
        }
    }
}
