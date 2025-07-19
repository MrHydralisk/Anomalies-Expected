using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace AnomaliesExpected
{
    public class Building_AEMapPortal : MapPortal
    {
        protected static readonly CachedTexture ViewSubMapTex = new CachedTexture("UI/Commands/ViewCave");

        public virtual bool isHideEntry => false;

        public CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = GetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        protected virtual DamageDef pocketMapDamageDef => DamageDefOf.Crush;

        public override string DescriptionFlavor
        {
            get
            {
                if (StudyUnlocks != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(StudyUnlocks.TransformDesc(def.description));
                    if (AllComps != null)
                    {
                        for (int i = 0; i < AllComps.Count; i++)
                        {
                            string descriptionPart = AllComps[i].GetDescriptionPart();
                            if (!descriptionPart.NullOrEmpty())
                            {
                                if (stringBuilder.Length > 0)
                                {
                                    stringBuilder.AppendLine();
                                    stringBuilder.AppendLine();
                                }
                                stringBuilder.Append(descriptionPart);
                            }
                        }
                    }
                    return stringBuilder.ToString();
                }
                return base.DescriptionFlavor;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action command_Action && command_Action.icon == EnterTex && isHideEntry)
                {
                    continue;
                }
                yield return gizmo;
            }
        }

        protected void GeneratePocketMap()
        {
            PocketMapUtility.currentlyGeneratingPortal = this;
            pocketMap = GeneratePocketMapInt();
            PocketMapUtility.currentlyGeneratingPortal = null;
        }

        public virtual void DestroyPocketMap()
        {
            if (LoadInProgress)
            {
                CancelLoad();
            }
            if (base.PocketMapExists)
            {
                DamageInfo damageInfo = new DamageInfo(pocketMapDamageDef, 99999f, 999f);
                for (int num = pocketMap.mapPawns.AllPawns.Count - 1; num >= 0; num--)
                {
                    Pawn pawn = pocketMap.mapPawns.AllPawns[num];
                    pawn.TakeDamage(damageInfo);
                    if (!pawn.Dead)
                    {
                        pawn.Kill(damageInfo);
                    }
                }
                PocketMapUtility.DestroyPocketMap(pocketMap);
            }
        }
    }
}
