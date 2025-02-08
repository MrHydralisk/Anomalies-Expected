using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Dialog_AEEntityDB : Window
    {
        private Vector2 recordScrollPos;

        private Vector2 dbScrollPos;

        private float recordScrollHeight;

        private float dbScrollHeight;

        private AEEntityEntry selectedEntry;

        private List<(string, List<AEEntityEntry>)> EntriesByType = new List<(string, List<AEEntityEntry>)>();

        private Dictionary<string, float> categoryRectSizes = new Dictionary<string, float>();

        private static readonly Vector2 ButSize = new Vector2(150f, 38f);

        private const float HeaderHeight = 30f;

        private const float EntrySize = 128f;

        private const float EntryGap = 8f;

        public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.9f, UI.screenHeight * 0.9f);

        private int state = 0;
        private bool isShowRecord;
        private bool[] lettersOpened;

        public Dialog_AEEntityDB(EntityCodexEntryDef selectedEntry = null)
        {
            doCloseX = true;
            forcePause = true;
            DoEntityCategory();
        }

        public void DoEntityCategory()
        {
            List<EntityCategoryDef> categoriesInOrder = DefDatabase<EntityCategoryDef>.AllDefsListForReading.OrderBy((EntityCategoryDef ecd) => ecd.listOrder).ToList();
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            categoryRectSizes.Clear();
            foreach (AEEntityEntry aeee in GameComponent_AnomaliesExpected.instance.EntityEntries)
            {
                List<AEEntityEntry> aeeeList;
                int index = EntriesByType.FindIndex(x => x.Item1 == (aeee.categoryLabelCap));
                if (index >= 0)
                {
                    aeeeList = EntriesByType[index].Item2;
                }
                else
                {
                    aeeeList = new List<AEEntityEntry>();
                    string key = aeee.categoryLabelCap;
                    EntriesByType.Add((key, aeeeList));
                    categoryRectSizes.Add(key, 0f);
                }
                aeeeList.Add(aeee);
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.orderInCategory ?? int.MaxValue, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.label ?? string.Empty);
            }
            EntriesByType.SortBy(x => categoriesInOrder.FindIndex((EntityCategoryDef ecd) => ecd.defName == x.Item1));
        }

        public void DoEntityClass()
        {
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            categoryRectSizes.Clear();
            foreach (AEEntityEntry aeee in GameComponent_AnomaliesExpected.instance.EntityEntries)
            {
                List<AEEntityEntry> aeeeList;
                (string, List<AEEntityEntry>) aeeePair = EntriesByType.FirstOrDefault(x => x.Item1 == aeee.threatClassString);
                if (aeeePair.Item2 == null)
                {
                    aeeeList = new List<AEEntityEntry>();
                    EntriesByType.Add((aeee.threatClassString, aeeeList));
                    categoryRectSizes.Add(aeee.threatClassString, 0f);
                }
                else
                {
                    aeeeList = aeeePair.Item2;
                }
                aeeeList.Add(aeee);
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.orderInCategory ?? int.MaxValue, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.label ?? string.Empty);
            }
            EntriesByType.SortBy(x => x.Item2.FirstOrDefault()?.ThreatClass ?? -1);
        }

        public void DoEntityGroup()
        {
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            categoryRectSizes.Clear();
            List<AEEntityEntry> entityEntries = GameComponent_AnomaliesExpected.instance.EntityEntries.ToList();
            int exitIndex = 0;
            while (!entityEntries.NullOrEmpty() && exitIndex < 9999)
            {
                AEEntityEntry aeee = entityEntries.FirstOrDefault();
                List<AEEntityEntry> aeeeList = new List<AEEntityEntry>();
                if (aeee.EntityCodexEntryDef?.Discovered ?? aeee.parentEntityEntry?.EntityCodexEntryDef?.Discovered ?? false)
                {
                    aeeeList = FindEntityGroupMembers(aeee, ref entityEntries);
                }
                else
                {
                    aeeeList.Add(aeee);
                    entityEntries.Remove(aeee);
                }
                string groupTag = aeee.groupName;
                if (aeeeList.Count() > 1)
                {
                    EntriesByType.Add((groupTag, aeeeList));
                    categoryRectSizes.Add(groupTag, 0f);
                }
                else
                {
                    groupTag = "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();
                    (string, List<AEEntityEntry>) aeeePair = EntriesByType.FirstOrDefault(x => x.Item1 == groupTag);
                    if (aeeePair.Item2 == null)
                    {
                        EntriesByType.Add((groupTag, aeeeList));
                        categoryRectSizes.Add(groupTag, 0f);
                    }
                    else
                    {
                        aeeePair.Item2.AddRange(aeeeList);
                    }
                }
                exitIndex++;
            }
            if (exitIndex >= 9999)
            {
                Log.Warning("Reached infinite loop");
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.orderInCategory ?? int.MaxValue, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.label ?? string.Empty);
            }
            EntriesByType.SortBy(x => x.Item1 ?? "");
        }

        public List<AEEntityEntry> FindEntityGroupMembers(AEEntityEntry aEEntityEntry, ref List<AEEntityEntry> entityEntries)
        {
            List<AEEntityEntry> aeeeList = new List<AEEntityEntry>() { aEEntityEntry };
            EntityCodexEntryDefExtension modExt = aEEntityEntry.EntityCodexEntryDef?.GetModExtension<EntityCodexEntryDefExtension>();
            if (modExt != null)
            {
                foreach (string entityCodexEntryDef in modExt.groupMembersEntityCodexDefs)
                {
                    AEEntityEntry aeeeC = GameComponent_AnomaliesExpected.instance.EntityEntries.FirstOrDefault((AEEntityEntry aeee) => (aeee.EntityCodexEntryDef?.defName ?? "") == entityCodexEntryDef && (aeee.EntityCodexEntryDef?.Discovered ?? false));
                    if (aeeeC != null)
                    {
                        aeeeList.AddRange(FindEntityGroupMembers(aeeeC, ref entityEntries));
                    }
                }
                foreach (string thingDefs in modExt.groupMembersThingDefs)
                {
                    AEEntityEntry aeeeT = GameComponent_AnomaliesExpected.instance.EntityEntries.FirstOrDefault((AEEntityEntry aeee) => (aeee.ThingDef?.defName ?? "") == thingDefs);
                    if (aeeeT != null)
                    {
                        aeeeList.AddRange(FindEntityGroupMembers(aeeeT, ref entityEntries));
                    }
                }
            }
            if (entityEntries.Contains(aEEntityEntry))
            {
                entityEntries.Remove(aEEntityEntry);
            }
            else
            {
                for (int i = EntriesByType.Count() - 1; i >= 0; i--)
                {
                    EntriesByType[i].Item2.RemoveAll((AEEntityEntry ee) => ee == aEEntityEntry);
                    if (EntriesByType[i].Item2.NullOrEmpty())
                    {
                        EntriesByType.RemoveAt(i);
                    }
                }
            }
            return aeeeList;
        }

        public void DoModGroup()
        {
            EntriesByType = new List<(string, List<AEEntityEntry>)>();
            categoryRectSizes.Clear();
            foreach (AEEntityEntry aeee in GameComponent_AnomaliesExpected.instance.EntityEntries)
            {
                List<AEEntityEntry> aeeeList;
                (string, List<AEEntityEntry>) aeeePair = EntriesByType.FirstOrDefault(x => x.Item1 == aeee.modName);
                if (aeeePair.Item2 == null)
                {
                    aeeeList = new List<AEEntityEntry>();
                    EntriesByType.Add((aeee.modName, aeeeList));
                    categoryRectSizes.Add(aeee.modName, 0f);
                }
                else
                {
                    aeeeList = aeeePair.Item2;
                }
                aeeeList.Add(aeee);
            }
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                value.SortBy((AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.orderInCategory ?? int.MaxValue, (AEEntityEntry aeee) => aeee.EntityCodexEntryDef?.label ?? string.Empty);
            }
            EntriesByType.SortBy(x => x.Item1);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect rect = inRect;
            rect.height -= ButSize.y + 10f;
            TaggedString taggedString = "AnomaliesExpected.EntityDataBase.Desc".Translate();
            Rect rect1 = new Rect(inRect.width / 2f - 132, 4, 264, 25);
            if (Widgets.ButtonText(rect1, "AnomaliesExpected.EntityDataBase.GroupType".Translate($"AnomaliesExpected.EntityDataBase.GroupType.{state}".Translate())))
            {
                List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.EntityDataBase.GroupType.0".Translate(), delegate
                {
                    DoEntityCategory();
                    state = 0;
                }));
                floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.EntityDataBase.GroupType.1".Translate(), delegate
                {
                    DoEntityClass();
                    state = 1;
                }));
                floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.EntityDataBase.GroupType.2".Translate(), delegate
                {
                    DoEntityGroup();
                    state = 2;
                }));
                floatMenuOptions.Add(new FloatMenuOption("AnomaliesExpected.EntityDataBase.GroupType.3".Translate(), delegate
                {
                    DoModGroup();
                    state = 3;
                }));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            Rect rect2 = new Rect(inRect.width - 39, 7, 32, 32);
            if (Widgets.ButtonImage(rect2, TexButton.CodexButton, tooltip: "EntityCodexGizmoTip".Translate()))
            {
                Find.WindowStack.Add(new Dialog_EntityCodex());
                Close();
            }
            if (isShowRecord)
            {
                Rect rect3 = new Rect(inRect.width / 2f + 159, 0, 33, 33);
                if (Widgets.ButtonImage(rect3, TexButton.CloseXSmall))
                {
                    isShowRecord = false;
                }
            }
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, HeaderHeight), "AnomaliesExpected.EntityDataBase.Label".Translate());
            }
            rect.yMin += 40f;
            TaggedString taggedString1 = "AnomaliesExpected.EntityDataBase.Desc".Translate();
            float num = Text.CalcHeight(taggedString1, rect.width);
            Widgets.Label(new Rect(0f, rect.y, rect.width, num), taggedString);
            rect.yMin += num + 10f;
            if (isShowRecord)
            {
                EntityRecord(rect);
            }
            else
            {
                AllEntities(rect);
            }
        }

        private void EntityRecord(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, recordScrollHeight);
            Widgets.BeginScrollView(inRect, ref recordScrollPos, viewRect);
            if (selectedEntry != null)
            {
                if (Prefs.DevMode && AEMod.Settings.DevModeInfo)
                {
                    Rect rect1 = new Rect(inRect.width - 8, 0, 8, 8);
                    if (Widgets.ButtonImage(rect1, TexButton.CloseXSmall, tooltip: "Remove EntityEntry"))
                    {
                        isShowRecord = false;
                        GameComponent_AnomaliesExpected.instance.EntityEntries.Remove(selectedEntry);
                    }
                }
                EntityCodexEntryDef entityCodexEntryDef = selectedEntry.EntityCodexEntryDef;
                float num = 0f;
                bool flag = entityCodexEntryDef?.Discovered ?? true;
                Rect rect3 = new Rect(inRect.width / 2f - 128, num + 2, 256, 256);
                if (selectedEntry.EntityCodexEntryDef == null)
                {
                    Texture2D uiIcon = selectedEntry.ThingDef.uiIcon;
                    float heightMult = (float)uiIcon.height / uiIcon.width;
                    if (heightMult > 1.05f)
                    {
                        rect3.width = rect3.width / heightMult;
                    }
                    else if (heightMult < 0.95f)
                    {
                        rect3.height = rect3.width * heightMult;
                    }
                    Widgets.DrawBoxSolid(rect3.ExpandedBy(2f), new Color(0f, 0f, 0f, 0.3f));
                    Color colorTMP = GUI.color;
                    GUI.color = selectedEntry.ThingDef.uiIconColor;
                    GUI.DrawTexture(rect3, uiIcon);
                    GUI.color = colorTMP;
                    num += Mathf.CeilToInt(256 * heightMult + 4);
                }
                else
                {
                    Widgets.DrawBoxSolid(rect3.ExpandedBy(2f), new Color(0f, 0f, 0f, 0.3f));
                    GUI.DrawTexture(rect3, flag ? (selectedEntry.EntityCodexEntryDef.icon) : selectedEntry.EntityCodexEntryDef.silhouette);
                    num += 260f;
                }
                using (new TextBlock(GameFont.Medium))
                {
                    Widgets.Label(new Rect(0f, num, viewRect.width, HeaderHeight), flag ? selectedEntry.AnomalyLabel : "UndiscoveredEntity".Translate());
                    num += 40f;
                }
                using (new TextBlock(newWordWrap: true))
                {
                    string text = (flag ? selectedEntry.AnomalyDesc : ((string)"UndiscoveredEntityDesc".Translate()));
                    float num2 = Text.CalcHeight(text, viewRect.width);
                    Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
                    num += num2 + EntryGap;
                }
                for (int i = 0; i < selectedEntry.letters.Count(); i++)
                {
                    ChoiceLetter choiceLetter = selectedEntry.letters[i];
                    using (new TextBlock(GameFont.Medium))
                    {
                        Rect rect1 = new Rect(0f, num, viewRect.width, 30);
                        Widgets.DrawBoxSolid(rect1, new Color(0f, 0f, 0f, 0.3f));
                        using (new TextBlock(TextAnchor.MiddleCenter))
                        {
                            Widgets.Label(rect1, choiceLetter.Label);
                        }
                        if (Widgets.ButtonInvisible(rect1))
                        {
                            lettersOpened[i] = !lettersOpened[i];
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        }
                        num += 35f;
                    }
                    if (lettersOpened[i])
                    {
                        string text = choiceLetter.Text;
                        float num2 = Text.CalcHeight(text, viewRect.width);
                        Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
                        num += num2 + 5;
                    }
                }
                if (flag)
                {
                    if ((entityCodexEntryDef?.linkedThings.Count ?? 0) > 0)
                    {
                        foreach (ThingDef linkedThing in entityCodexEntryDef.linkedThings)
                        {
                            Rect rect = new Rect(0f, num, viewRect.width, Text.LineHeight);
                            if (Find.EntityCodex.Discovered(linkedThing))
                            {
                                Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(linkedThing));
                            }
                            else
                            {
                                rect.xMin += rect.height;
                                using (new TextBlock(ColoredText.SubtleGrayColor))
                                {
                                    Widgets.Label(rect, "Undiscovered".Translate());
                                }
                            }
                            num += rect.height;
                        }
                        num += EntryGap;
                    }
                    if ((entityCodexEntryDef?.discoveredResearchProjects.Count ?? 0) > 0)
                    {
                        Widgets.Label(new Rect(0f, num, viewRect.width, Text.LineHeight), "ResearchUnlocks".Translate() + ":");
                        num += Text.LineHeight;
                        foreach (ResearchProjectDef discoveredResearchProject in entityCodexEntryDef.discoveredResearchProjects)
                        {
                            Rect rect2 = new Rect(0f, num, viewRect.width, Text.LineHeight);
                            if (discoveredResearchProject.IsHidden)
                            {
                                rect2.xMin += rect2.height;
                                using (new TextBlock(ColoredText.SubtleGrayColor))
                                {
                                    Widgets.Label(rect2, "Undiscovered".Translate());
                                }
                            }
                            else
                            {
                                if (Widgets.ButtonText(rect2, "ViewHyperlink".Translate(discoveredResearchProject.LabelCap), drawBackground: false))
                                {
                                    Close();
                                    Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
                                    ((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).Select(discoveredResearchProject);
                                }
                            }
                            num += rect2.height;
                        }
                    }
                }
                if (Prefs.DevMode && AEMod.Settings.DevModeInfo)
                {
                    using (new TextBlock(newWordWrap: true))
                    {
                        string text = $"Dev Mod\n" +
                            $"ThingDef {selectedEntry.ThingDef?.LabelCap ?? "---"}\n" +
                            $"EntityCodexEntryDef {selectedEntry.EntityCodexEntryDef?.LabelCap ?? "---"}\n" +
                            $"parentEntityEntry {selectedEntry.parentEntityEntry?.AnomalyLabel ?? "---"}\n" +
                            $"groupName {selectedEntry.groupName}\n" +
                            $"categoryLabelCap {selectedEntry.categoryLabelCap}\n" +
                            $"threatClassString {selectedEntry.threatClassString}\n" +
                            $"modName {selectedEntry.modName}\n" +
                            $"CurrPawnAmountStudied {string.Join(" | ", selectedEntry.CurrPawnAmountStudied)}\n" +
                            $"parentEntityEntryRef {selectedEntry.parentEntityEntryRef}";
                        float num2 = Text.CalcHeight(text, viewRect.width);
                        Widgets.Label(new Rect(0f, num, viewRect.width, num2), text);
                        num += num2 + EntryGap;
                    }
                    num += ButSize.y;
                }
                recordScrollHeight = num;
            }
            else
            {
                isShowRecord = false;
            }
            Widgets.EndScrollView();
        }

        private void AllEntities(Rect rect)
        {
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, dbScrollHeight);
            Widgets.BeginScrollView(rect, ref dbScrollPos, viewRect);
            float num = 0f;
            Text.Anchor = TextAnchor.MiddleCenter;
            foreach ((string key, List<AEEntityEntry> value) in EntriesByType)
            {
                float num2 = num;
                float height = categoryRectSizes[key];
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawHighlight(new Rect(0f, num, rect.width, height));
                GUI.color = Color.white;
                Widgets.Label(new Rect(0, num, rect.width, Text.LineHeight), key);
                num += Text.LineHeight + 4f;
                int MaxEntriesPerRow = Mathf.FloorToInt((viewRect.width - EntryGap) / (EntrySize + EntryGap));
                int EntryRows = Mathf.CeilToInt((float)value.Count / (float)MaxEntriesPerRow);
                for (int i = 0; i < value.Count; i++)
                {
                    AEEntityEntry aeee = value[i];
                    int num5 = i / MaxEntriesPerRow;
                    int num6 = i % MaxEntriesPerRow;
                    int num7 = ((i >= value.Count - value.Count % MaxEntriesPerRow) ? (value.Count % MaxEntriesPerRow) : MaxEntriesPerRow);
                    float num8 = (viewRect.width - (float)num7 * EntrySize - (float)(num7 - 1) * EntryGap) / 2f;
                    Rect rect2 = new Rect(num8 + (float)num6 * EntrySize + (float)num6 * EntryGap, num + (float)num5 * EntrySize + (float)num5 * EntryGap, EntrySize, EntrySize);
                    bool flag = aeee.EntityCodexEntryDef?.Discovered ?? true;
                    DrawEntry(rect2, aeee, flag);
                    if (flag)
                    {
                        Text.Font = GameFont.Tiny;
                        string name = aeee.EntityCodexEntryDef?.LabelCap ?? aeee.ThingDef.LabelCap;
                        float num9 = Text.CalcHeight(name, rect2.width);
                        Rect rect3 = new Rect(rect2.x, rect2.yMax - num9, rect2.width, num9);
                        Widgets.DrawBoxSolid(rect3, new Color(0f, 0f, 0f, 0.3f));
                        using (new TextBlock(TextAnchor.MiddleCenter))
                        {
                            Widgets.Label(rect3, name);
                        }
                        Text.Font = GameFont.Small;
                    }
                }
                num += EntryGap + (float)EntryRows * EntrySize + (float)(EntryRows - 1) * EntryGap;
                categoryRectSizes[key] = num - num2;
                num += EntryGap;
            }
            dbScrollHeight = num;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndScrollView();
        }

        private void DrawEntry(Rect rect, AEEntityEntry entry, bool discovered)
        {
            Widgets.DrawOptionBackground(rect, entry == selectedEntry);
            Rect rect1 = new Rect(rect);
            Texture2D uiIcon;
            Color colorTMP = GUI.color;
            if (discovered)
            {
                if (entry.EntityCodexEntryDef == null)
                {
                    uiIcon = entry.ThingDef.uiIcon;
                    GUI.color = entry.ThingDef.uiIconColor;
                    float heightMult = (float)uiIcon.height / uiIcon.width;
                    if (heightMult > 1.05f)
                    {
                        rect1.width = rect1.width / heightMult;
                        rect1.x = rect1.x + (rect.width - rect1.width) / 2;
                    }
                    else if (heightMult < 0.95f)
                    {
                        rect1.height = rect1.width * heightMult;
                        rect1.y = rect1.y + (rect.height - rect1.height) / 2;
                    }
                }
                else
                {
                    uiIcon = entry.EntityCodexEntryDef.icon;
                }
            }
            else
            {
                uiIcon = entry.EntityCodexEntryDef.silhouette;
            }
            GUI.DrawTexture(rect1.ContractedBy(2f), uiIcon);
            GUI.color = colorTMP;
            if (entry.letters.Count() > 0)
            {
                GUI.DrawTexture(new Rect(rect.x + rect.width - 20, rect.y - 1, 18, 18), TexButton.CategorizedResourceReadout);
            }
            if (Widgets.ButtonInvisible(rect))
            {
                SelectEntry(entry);
            }
        }

        public void SelectEntry(AEEntityEntry entry)
        {
            selectedEntry = entry;
            isShowRecord = true;
            lettersOpened = new bool[selectedEntry.letters.Count()];
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        }
    }
}

