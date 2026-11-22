using UnityEngine;

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

    public static int MaxStacks(SkillNode node)
    {
        if (node == null || node.stacks <= 1)
            return 1;
        return node.stacks;
    }

    public static int Rank(SkillTree tree, SkillNode node)
    {
        if (node == null)
            return 0;
        if (node.root)
            return 1;
        if (tree == null)
            return 0;
        return SaveGame.NodeRank(tree.id, node.id);
    }

    public static int NextCost(SkillTree tree, SkillNode node)
    {
        if (node == null)
            return 0;
        int price = Mathf.Max(0, node.cost);
        if (MaxStacks(node) <= 1)
            return price;
        return price * (Rank(tree, node) + 1);
    }

    public static bool IsComplete(SkillTree tree, SkillNode node)
    {
        if (node == null)
            return false;
        if (node.root)
            return true;
        return Rank(tree, node) >= MaxStacks(node);
    }

    public static bool IsUnlocked(SkillTree tree, SkillNode node)
    {
        if (tree == null || node == null)
            return false;
        if (node.root)
            return true;
        return Rank(tree, node) > 0;
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
        if (!IsVisible(tree, node) || IsComplete(tree, node))
            return false;
        return SaveGame.Gold >= NextCost(tree, node);
    }

    public static bool TryBuy(SkillTree tree, SkillNode node)
    {
        if (!CanBuy(tree, node) || !SaveGame.TrySpendGold(NextCost(tree, node)))
            return false;

        SaveGame.SetNodeRank(tree.id, node.id, Rank(tree, node) + 1);
        return true;
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
