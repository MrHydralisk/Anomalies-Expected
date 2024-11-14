using RimWorld;
using System.Text;
using Verse;

namespace AnomaliesExpected
{
    public class Building_AEMapPortal : MapPortal
    {
        public CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = GetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

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

        public override Map GetOtherMap()
        {
            return null;
        }

        public override IntVec3 GetDestinationLocation()
        {
            return IntVec3.Invalid;
        }
    }
}
