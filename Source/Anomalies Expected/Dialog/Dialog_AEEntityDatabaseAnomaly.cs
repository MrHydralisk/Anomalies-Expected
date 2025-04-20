using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Dialog_AEEntityDatabaseAnomaly : Window
    {
        private Vector2 ScrollPos;

        private float ScrollHeight;

        //private AEEntityEntry selectedEntry;

        //private List<(string, List<AEEntityEntry>)> EntriesByType = new List<(string, List<AEEntityEntry>)>();

        //private Dictionary<string, float> categoryRectSizes = new Dictionary<string, float>();

        //private static readonly Vector2 ButSize = new Vector2(150f, 38f);

        private const float HeaderHeight = 30f;

        //private const float EntrySize = 128f;

        //private const float EntryGap = 8f;
        //private List<Thing> RelatedAnalyzableThingsCached = new List<Thing>();

        public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.6f, UI.screenHeight * 0.75f);

        //private int state = 0;
        //private bool isShowRecord;
        //private bool[] lettersOpened;
        private Comp_EntityDatabaseAnomaly entityDatabaseAnomaly;

        public Dialog_AEEntityDatabaseAnomaly(Comp_EntityDatabaseAnomaly comp_EntityDatabaseAnomaly)
        {
            doCloseX = true;
            forcePause = true;
            entityDatabaseAnomaly = comp_EntityDatabaseAnomaly;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect rect = inRect;
            //rect.height -= ButSize.y + 10f;
            TaggedString taggedString = "AnomaliesExpected.EntityDatabaseAnomaly.Desc".Translate(entityDatabaseAnomaly.parent.Label);
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, HeaderHeight), entityDatabaseAnomaly.parent.Label);
            }
            rect.yMin += 40f;
            TaggedString taggedString1 = "AnomaliesExpected.EntityDataBase.Desc".Translate();
            float num = Text.CalcHeight(taggedString1, rect.width);
            Widgets.Label(new Rect(0f, rect.y, rect.width, num), taggedString);
            rect.yMin += num + 10f;
            //AllEntities(rect);
        }

        //private void EntityRecord(Rect inRect)
        //{
        //    Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, recordScrollHeight);
        //    Widgets.BeginScrollView(inRect, ref recordScrollPos, viewRect);
        //    if (selectedEntry != null)
        //    {
        //        if (Prefs.DevMode && AEMod.Settings.DevModeInfo)
        //        {
        //            Rect rect1 = new Rect(inRect.width - 8, 0, 8, 8);
        //            if (Widgets.ButtonImage(rect1, TexButton.CloseXSmall, tooltip: "Remove EntityEntry"))
        //            {
        //                isShowRecord = false;
        //                GameComponent_AnomaliesExpected.instance.EntityEntries.Remove(selectedEntry);
        //            }
        //        }
        //        EntityCodexEntryDef entityCodexEntryDef = selectedEntry.EntityCodexEntryDef;
        //        float num = 0f;
        //        bool flag = entityCodexEntryDef?.Discovered ?? true;
        //        Rect rect3 = new Rect(inRect.width / 2f - 128, num + 2, 256, 256);
        //        if (selectedEntry.EntityCodexEntryDef == null)
        //        {
        //            Texture2D uiIcon = selectedEntry.ThingDef.uiIcon;
        //            float heightMult = (float)uiIcon.height / uiIcon.width;
        //            if (heightMult > 1.05f)
        //            {
        //                rect3.width = rect3.width / heightMult;
        //            }
        //            else if (heightMult < 0.95f)
        //            {
        //                rect3.height = rect3.width * heightMult;
        //            }
        //            Widgets.DrawBoxSolid(rect3.ExpandedBy(2f), new Color(0f, 0f, 0f, 0.3f));
        //            Color colorTMP = GUI.color;
        //            GUI.color = selectedEntry.ThingDef.uiIconColor;
        //            GUI.DrawTexture(rect3, uiIcon);
        //            GUI.color = colorTMP;
        //            num += Mathf.CeilToInt(256 * heightMult + 4);
        //        }
        //        else
        //        {
        //            Widgets.DrawBoxSolid(rect3.ExpandedBy(2f), new Color(0f, 0f, 0f, 0.3f));
        //            GUI.DrawTexture(rect3, flag ? (selectedEntry.EntityCodexEntryDef.icon) : selectedEntry.EntityCodexEntryDef.silhouette);
        //            num += 260f;
        //        }
        //        using (new TextBlock(GameFont.Medium))
        //        {
        //            Widgets.Label(new Rect(0f, num, viewRect.width, HeaderHeight), flag ? selectedEntry.AnomalyLabel : "UndiscoveredEntity".Translate());
        //            num += 40f;
        //        }
        //        using (new TextBlock(newWordWrap: true))
        //        {
        //            string text = (flag ? selectedEntry.AnomalyDesc : ((string)"UndiscoveredEntityDesc".Translate()));
        //            float num2 = Text.CalcHeight(text, viewRect.width);
        //            Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
        //            num += num2 + EntryGap;
        //        }
        //        using (new TextBlock(newWordWrap: true))
        //        {
        //            int amountStudied = selectedEntry.CurrPawnAmountStudied.LastOrDefault();
        //            if (amountStudied > 0)
        //            {
        //                string text = "AnomaliesExpected.EntityDataBase.AmountStudied".Translate(amountStudied);
        //                float num2 = Text.CalcHeight(text, viewRect.width);
        //                Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
        //                num += num2 + EntryGap;
        //            }
        //        }
        //        for (int i = 0; i < selectedEntry.letters.Count(); i++)
        //        {
        //            ChoiceLetter choiceLetter = selectedEntry.letters[i];
        //            using (new TextBlock(GameFont.Medium))
        //            {
        //                Rect rect1 = new Rect(0f, num, viewRect.width, 30);
        //                Widgets.DrawBoxSolid(rect1, new Color(0f, 0f, 0f, 0.3f));
        //                using (new TextBlock(TextAnchor.MiddleCenter))
        //                {
        //                    Widgets.Label(rect1, choiceLetter.Label);
        //                }
        //                if (Widgets.ButtonInvisible(rect1))
        //                {
        //                    lettersOpened[i] = !lettersOpened[i];
        //                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //                }
        //                num += 35f;
        //            }
        //            if (lettersOpened[i])
        //            {
        //                string text = choiceLetter.Text;
        //                float num2 = Text.CalcHeight(text, viewRect.width);
        //                Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
        //                num += num2 + 5;
        //            }
        //        }
        //        if (flag)
        //        {
        //            if ((entityCodexEntryDef?.linkedThings.Count ?? 0) > 0)
        //            {
        //                foreach (ThingDef linkedThing in entityCodexEntryDef.linkedThings)
        //                {
        //                    Rect rect = new Rect(0f, num, viewRect.width, Text.LineHeight);
        //                    if (Find.EntityCodex.Discovered(linkedThing))
        //                    {
        //                        Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(linkedThing));
        //                    }
        //                    else
        //                    {
        //                        rect.xMin += rect.height;
        //                        using (new TextBlock(ColoredText.SubtleGrayColor))
        //                        {
        //                            Widgets.Label(rect, "Undiscovered".Translate());
        //                        }
        //                    }
        //                    num += rect.height;
        //                }
        //                num += EntryGap;
        //            }
        //            if ((entityCodexEntryDef?.discoveredResearchProjects.Count ?? 0) > 0)
        //            {
        //                Widgets.Label(new Rect(0f, num, viewRect.width, Text.LineHeight), "ResearchUnlocks".Translate() + ":");
        //                num += Text.LineHeight;
        //                foreach (ResearchProjectDef discoveredResearchProject in entityCodexEntryDef.discoveredResearchProjects)
        //                {
        //                    Rect rect2 = new Rect(0f, num, viewRect.width, Text.LineHeight);
        //                    if (discoveredResearchProject.IsHidden)
        //                    {
        //                        rect2.xMin += rect2.height;
        //                        using (new TextBlock(ColoredText.SubtleGrayColor))
        //                        {
        //                            Widgets.Label(rect2, "Undiscovered".Translate());
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (Widgets.ButtonText(rect2, "ViewHyperlink".Translate(discoveredResearchProject.LabelCap), drawBackground: false))
        //                        {
        //                            Close();
        //                            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
        //                            ((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).Select(discoveredResearchProject);
        //                        }
        //                    }
        //                    num += rect2.height;
        //                }
        //            }
        //            int SpawnedRelatedAnalyzableThingDefCount = selectedEntry.SpawnedRelatedAnalyzableThingDef.Count;
        //            if (SpawnedRelatedAnalyzableThingDefCount > 0)
        //            {
        //                float ThingSize = 64;
        //                float ThingGap = 4;
        //                GUI.color = Color.white;
        //                Widgets.Label(new Rect(0, num, viewRect.width, Text.LineHeight), "AnomaliesExpected.EntityDataBase.AnalyzableThings".Translate());
        //                num += Text.LineHeight + 4f;
        //                int MaxEntriesPerRow = Mathf.FloorToInt((viewRect.width - ThingGap) / (ThingSize + ThingGap));
        //                int RowsAmount = Mathf.CeilToInt((float)SpawnedRelatedAnalyzableThingDefCount / MaxEntriesPerRow);
        //                for (int i = 0; i < SpawnedRelatedAnalyzableThingDefCount; i++)
        //                {
        //                    ThingDef thingDef = selectedEntry.SpawnedRelatedAnalyzableThingDef[i];
        //                    int num5 = i / MaxEntriesPerRow;
        //                    int num6 = i % MaxEntriesPerRow;
        //                    int num7 = ((i >= SpawnedRelatedAnalyzableThingDefCount - SpawnedRelatedAnalyzableThingDefCount % MaxEntriesPerRow) ? (SpawnedRelatedAnalyzableThingDefCount % MaxEntriesPerRow) : MaxEntriesPerRow);
        //                    float num8 = (viewRect.width - (float)num7 * ThingSize - (float)(num7 - 1) * ThingGap) / 2f;
        //                    Rect rect2 = new Rect(num8 + (float)num6 * ThingSize + (float)num6 * ThingGap, num + (float)num5 * ThingSize + (float)num5 * ThingGap, ThingSize, ThingSize);
        //                    Rect rect4 = new Rect(rect2.x + rect2.width - 18, rect2.y - 1, 16, 16);
        //                    Widgets.DrawBoxSolid(rect2, new Color(0f, 0f, 0f, 0.3f));
        //                    GUI.DrawTexture(rect2.ContractedBy(2f), thingDef.uiIcon);
        //                    int analysisID = thingDef.GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>()?.analysisID ?? (-1);
        //                    if (analysisID != -1)
        //                    {
        //                        Thing thingExist;
        //                        if (Find.AnalysisManager.TryGetAnalysisProgress(analysisID, out var details) && details.Satisfied)
        //                        {
        //                            GUI.DrawTexture(rect4, Widgets.CheckboxOnTex);
        //                            TooltipHandler.TipRegionByKey(rect2, "AnomaliesExpected.EntityDataBase.ResearchNote.Studied.Tip", thingDef.LabelCap);
        //                        }
        //                        else if ((thingExist = RelatedAnalyzableThingsCached.FirstOrDefault((Thing t) => t.def == thingDef)) != null)
        //                        {
        //                            GUI.DrawTexture(rect4, Widgets.CheckboxPartialTex);
        //                            TooltipHandler.TipRegionByKey(rect2, "AnomaliesExpected.EntityDataBase.ResearchNote.Exist.Tip", thingDef.LabelCap);
        //                            if (Widgets.ButtonInvisible(rect2))
        //                            {
        //                                CameraJumper.TryJumpAndSelect(thingExist);
        //                                Close();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            GUI.DrawTexture(rect4, Widgets.CheckboxOffTex);
        //                            TooltipHandler.TipRegionByKey(rect2, "AnomaliesExpected.EntityDataBase.ResearchNote.Missing.Tip", thingDef.LabelCap);
        //                            if (Widgets.ButtonInvisible(rect2))
        //                            {
        //                                selectedEntry.SpawnThing(thingDef);
        //                                UpdateRelatedAnalyzableThing();
        //                            }
        //                        }
        //                    }
        //                }
        //                num += ThingGap + (float)RowsAmount * ThingSize + (float)(RowsAmount - 1) * ThingGap;
        //            }
        //        }
        //        if (Prefs.DevMode && AEMod.Settings.DevModeInfo)
        //        {
        //            using (new TextBlock(newWordWrap: true))
        //            {
        //                string text = $"Dev Mod\n" +
        //                    $"ThingDef {selectedEntry.ThingDef?.LabelCap ?? "---"}\n" +
        //                    $"EntityCodexEntryDef {selectedEntry.EntityCodexEntryDef?.LabelCap ?? "---"}\n" +
        //                    $"parentEntityEntry {selectedEntry.parentEntityEntry?.AnomalyLabel ?? "---"}\n" +
        //                    $"groupName {selectedEntry.groupName}\n" +
        //                    $"categoryLabelCap {selectedEntry.categoryLabelCap}\n" +
        //                    $"category {selectedEntry.category?.LabelCap ?? "---"}\n" +
        //                    $"threatClassString {selectedEntry.threatClassString}\n" +
        //                    $"modName {selectedEntry.modName}\n" +
        //                    $"CurrPawnAmountStudied {string.Join(" | ", selectedEntry.CurrPawnAmountStudied)}\n" +
        //                    $"parentEntityEntryRef {selectedEntry.parentEntityEntryRef}";
        //                float num2 = Text.CalcHeight(text, viewRect.width);
        //                Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
        //                num += num2 + EntryGap;
        //            }
        //            num += ButSize.y;
        //        }
        //        recordScrollHeight = num;
        //    }
        //    else
        //    {
        //        isShowRecord = false;
        //    }
        //    Widgets.EndScrollView();
        //}

        //private void UpdateRelatedAnalyzableThing()
        //{
        //    RelatedAnalyzableThingsCached = new List<Thing>();
        //    foreach (Map map in Find.Maps)
        //    {
        //        foreach (Thing thing in map.listerThings.AllThings)
        //        {
        //            if (selectedEntry.SpawnedRelatedAnalyzableThingDef.Any((ThingDef td) => td == thing.def))
        //            {
        //                RelatedAnalyzableThingsCached.Add(thing);
        //            }
        //        }
        //    }
        //}

        //private void AllEntities(Rect rect)
        //{
        //    Rect viewRect = new Rect(0f, 0f, rect.width - 16f, dbScrollHeight);
        //    Widgets.BeginScrollView(rect, ref dbScrollPos, viewRect);
        //    float num = 0f;
        //    Text.Anchor = TextAnchor.MiddleCenter;
        //    foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
        //    {
        //        float num2 = num;
        //        float height = categoryRectSizes[key];
        //        GUI.color = new Color(1f, 1f, 1f, 0.5f);
        //        Widgets.DrawHighlight(new Rect(0f, num, rect.width, height));
        //        GUI.color = Color.white;
        //        Widgets.Label(new Rect(0, num, rect.width, Text.LineHeight), key);
        //        num += Text.LineHeight + 4f;
        //        int MaxEntriesPerRow = Mathf.FloorToInt((viewRect.width - EntryGap) / (EntrySize + EntryGap));
        //        int EntryRows = Mathf.CeilToInt((float)value.Count / (float)MaxEntriesPerRow);
        //        for (int i = 0; i < value.Count; i++)
        //        {
        //            AEEntityEntry aeee = value[i];
        //            int num5 = i / MaxEntriesPerRow;
        //            int num6 = i % MaxEntriesPerRow;
        //            int num7 = ((i >= value.Count - value.Count % MaxEntriesPerRow) ? (value.Count % MaxEntriesPerRow) : MaxEntriesPerRow);
        //            float num8 = (viewRect.width - (float)num7 * EntrySize - (float)(num7 - 1) * EntryGap) / 2f;
        //            Rect rect2 = new Rect(num8 + (float)num6 * EntrySize + (float)num6 * EntryGap, num + (float)num5 * EntrySize + (float)num5 * EntryGap, EntrySize, EntrySize);
        //            bool flag = aeee.EntityCodexEntryDef?.Discovered ?? true;
        //            DrawEntry(rect2, aeee, flag);
        //            if (flag)
        //            {
        //                Text.Font = GameFont.Tiny;
        //                string name = aeee.EntityCodexEntryDef?.LabelCap ?? aeee.ThingDef.LabelCap;
        //                float num9 = Text.CalcHeight(name, rect2.width);
        //                Rect rect3 = new Rect(rect2.x, rect2.yMax - num9, rect2.width, num9);
        //                Widgets.DrawBoxSolid(rect3, new Color(0f, 0f, 0f, 0.3f));
        //                using (new TextBlock(TextAnchor.MiddleCenter))
        //                {
        //                    Widgets.Label(rect3, name);
        //                }
        //                Text.Font = GameFont.Small;
        //            }
        //        }
        //        num += EntryGap + (float)EntryRows * EntrySize + (float)(EntryRows - 1) * EntryGap;
        //        categoryRectSizes[key] = num - num2;
        //        num += EntryGap;
        //    }
        //    dbScrollHeight = num;
        //    Text.Anchor = TextAnchor.UpperLeft;
        //    Widgets.EndScrollView();
        //}

        //private void DrawEntry(Rect rect, AEEntityEntry entry, bool discovered)
        //{
        //    Widgets.DrawOptionBackground(rect, entry == selectedEntry);
        //    Rect rect1 = new Rect(rect);
        //    Texture2D uiIcon;
        //    Color colorTMP = GUI.color;
        //    if (discovered)
        //    {
        //        if (entry.EntityCodexEntryDef == null)
        //        {
        //            uiIcon = entry.ThingDef.uiIcon;
        //            GUI.color = entry.ThingDef.uiIconColor;
        //            float heightMult = (float)uiIcon.height / uiIcon.width;
        //            if (heightMult > 1.05f)
        //            {
        //                rect1.width = rect1.width / heightMult;
        //                rect1.x = rect1.x + (rect.width - rect1.width) / 2;
        //            }
        //            else if (heightMult < 0.95f)
        //            {
        //                rect1.height = rect1.width * heightMult;
        //                rect1.y = rect1.y + (rect.height - rect1.height) / 2;
        //            }
        //        }
        //        else
        //        {
        //            uiIcon = entry.EntityCodexEntryDef.icon;
        //        }
        //    }
        //    else
        //    {
        //        uiIcon = entry.EntityCodexEntryDef.silhouette;
        //    }
        //    GUI.DrawTexture(rect1.ContractedBy(2f), uiIcon);
        //    GUI.color = colorTMP;
        //    if (entry.letters.Count() > 0)
        //    {
        //        GUI.DrawTexture(new Rect(rect.x + rect.width - 20, rect.y - 1, 18, 18), TexButton.CategorizedResourceReadout);
        //    }
        //    if (Widgets.ButtonInvisible(rect))
        //    {
        //        SelectEntry(entry);
        //    }
        //}

        //public void SelectEntry(AEEntityEntry entry)
        //{
        //    selectedEntry = entry;
        //    isShowRecord = true;
        //    lettersOpened = new bool[selectedEntry.letters.Count()];
        //    SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //    UpdateRelatedAnalyzableThing();
        //}
    }
}

