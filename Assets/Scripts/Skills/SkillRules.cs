public static class SkillRules
{
    public static SkillNode Find(SkillTree tree, string id)
    {
        if (tree == null || tree.nodes == null || string.IsNullOrEmpty(id))
            return null;

        foreach (SkillNode node in tree.nodes)
        {
            if (node != null && node.id == id)
                return node;
        }

        return null;
    }

    public static bool IsUnlocked(SkillTree tree, SkillNode node)
    {
        if (tree == null || node == null)
            return false;
        if (node.root)
            return true;
        return Profile.IsNodeUnlocked(tree.id, node.id);
    }

    public static bool IsVisible(SkillTree tree, SkillNode node)
    {
        if (tree == null || node == null)
            return false;
        if (node.root)
            return true;
        if (!Profile.IsEventDone(node.revealEvent))
            return false;
        return HasUnlockedNeighbor(tree, node);
    }

    public static bool CanBuy(SkillTree tree, SkillNode node)
    {
        if (!IsVisible(tree, node) || IsUnlocked(tree, node))
            return false;
        return SaveGame.Gold >= node.cost;
    }

    public static bool HasUnlockedNeighbor(SkillTree tree, SkillNode node)
    {
        if (node.links == null)
            return false;

        foreach (string link in node.links)
        {
            SkillNode other = Find(tree, link);
            if (IsUnlocked(tree, other))
                return true;
        }

        return false;
    }
}
