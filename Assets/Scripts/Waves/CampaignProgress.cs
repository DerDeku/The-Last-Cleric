using UnityEngine;

public static class CampaignProgress
{
    public static int Count(WaveTable table)
    {
        int total = 0;
        int[] counts = Counts(table);
        for (int i = 0; i < counts.Length; i++)
            total += counts[i];
        return total;
    }

    public static WaveDefinition Wave(WaveTable table, int index)
    {
        if (table == null || table.chapters == null || index < 0)
            return null;

        int cursor = 0;
        foreach (ChapterTable chapter in table.chapters)
        {
            int length = Length(chapter);
            if (index < cursor + length)
                return chapter.waves[index - cursor];
            cursor += length;
        }

        return null;
    }

    public static bool IsOpen(WaveTable table, int index)
    {
        return index >= 0 && index < Count(table) && index <= SaveGame.Cleared;
    }

    public static int Selected(WaveTable table)
    {
        return SaveGame.SelectedWave(Count(table));
    }

    public static void Jump(WaveTable table, int index)
    {
        int count = Count(table);
        if (count == 0 || index < 0 || index >= count)
            return;

        SaveGame.SetProgress(index, index);
        SyncChapters(table, index);
    }

    public static void OpenAll(WaveTable table)
    {
        int count = Count(table);
        if (count == 0)
            return;

        SaveGame.SetProgress(count, count - 1);
        SyncChapters(table, count);
    }

    public static void Select(WaveTable table, int index)
    {
        if (!IsOpen(table, index))
            return;
        SaveGame.SetSelected(index);
    }

    public static int ChapterCount(WaveTable table)
    {
        return table != null && table.chapters != null ? table.chapters.Length : 0;
    }

    public static int ChapterStart(WaveTable table, int chapter)
    {
        int[] counts = Counts(table);
        if (counts.Length == 0)
            return 0;

        chapter = Mathf.Clamp(chapter, 0, counts.Length - 1);
        int start = 0;
        for (int i = 0; i < chapter; i++)
            start += counts[i];
        return start;
    }

    public static int ChapterLength(WaveTable table, int chapter)
    {
        int[] counts = Counts(table);
        if (chapter < 0 || chapter >= counts.Length)
            return 0;
        return counts[chapter];
    }

    public static int SelectedChapter(WaveTable table)
    {
        int chapter;
        int wave;
        Locate(table, Selected(table), out chapter, out wave);
        return chapter - 1;
    }

    public static bool ChapterOpen(WaveTable table, int chapter)
    {
        if (chapter < 0 || chapter >= ChapterCount(table))
            return false;
        return ChapterStart(table, chapter) <= SaveGame.Cleared;
    }

    public static void SelectChapter(WaveTable table, int chapter)
    {
        if (!ChapterOpen(table, chapter))
            return;
        SaveGame.SetSelected(ChapterStart(table, chapter));
    }

    public static void NoteChapter(WaveTable table, int chapter, bool clearedChapter)
    {
        if (!clearedChapter || table == null)
            return;

        int end = ChapterStart(table, chapter) + ChapterLength(table, chapter);
        int cleared = SaveGame.Cleared;
        bool advanced = end > cleared;
        if (advanced)
            cleared = end;

        int lastChapter = Mathf.Max(0, ChapterCount(table) - 1);
        int selectedChapter = advanced ? Mathf.Min(chapter + 1, lastChapter) : chapter;
        int lastWave = Mathf.Max(0, Count(table) - 1);
        SaveGame.SetProgress(cleared, Mathf.Min(ChapterStart(table, selectedChapter), lastWave));
        SyncChapters(table, cleared);
    }

    public static void Locate(WaveTable table, int index, out int chapter, out int wave)
    {
        chapter = 1;
        wave = index + 1;
        int cursor = 0;
        int[] counts = Counts(table);
        for (int i = 0; i < counts.Length; i++)
        {
            if (index < cursor + counts[i])
            {
                chapter = i + 1;
                wave = index - cursor + 1;
                return;
            }

            cursor += counts[i];
        }
    }

    static void SyncChapters(WaveTable table, int cleared)
    {
        int passed = 0;
        int[] counts = Counts(table);
        for (int i = 0; i < counts.Length; i++)
        {
            passed += counts[i];
            string id = "chapter." + (i + 1);
            if (cleared >= passed)
            {
                if (!SaveGame.HasEvent(id))
                {
                    Profile.BenchForEvent(id);
                    BattleLog.AddUnlock(UnlockLabel(id));
                }
                SaveGame.AddEvent(id);
            }
            else
                SaveGame.RemoveEvent(id);
        }
    }

    static int[] Counts(WaveTable table)
    {
        int count = ChapterCount(table);
        var counts = new int[count];
        for (int i = 0; i < count; i++)
            counts[i] = Length(table.chapters[i]);
        return counts;
    }

    public static void Reveal(WaveTable table)
    {
        SyncChapters(table, SaveGame.Cleared);
    }

    static string UnlockLabel(string eventId)
    {
        PrepareRoster roster = Resources.Load<PrepareRoster>("Menu/Roster");
        if (roster != null && roster.heroes != null)
        {
            foreach (RosterEntry entry in roster.heroes)
            {
                if (entry != null && entry.unlockEvent == eventId && !string.IsNullOrEmpty(entry.nameKey))
                    return entry.nameKey;
            }
        }

        return eventId;
    }

    static int Length(ChapterTable chapter)
    {
        return chapter != null && chapter.waves != null ? chapter.waves.Length : 0;
    }
}
