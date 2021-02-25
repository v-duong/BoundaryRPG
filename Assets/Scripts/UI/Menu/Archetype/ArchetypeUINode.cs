using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUINode : MonoBehaviour
{
    public static readonly Color UNAVAILABLE_COLOR = Color.black;
    public static readonly Color AVAILABLE_COLOR = new Color(0.6f, 0.6f, 0.6f);
    public static readonly Color CONNECTED_COLOR = new Color(0f, 0.6f, 0f);
    public static readonly Color LEVEL_COLOR = new Color(0.3f, 0.95f, 0.1f);
    public static readonly Color MAX_LEVEL_COLOR = new Color(1f, 0.85f, 0.1f);
    private static readonly int yPositionOffset = 180;

    public bool isLevelable = false;
    public ArchetypeSkillNode node;

    [SerializeField]
    private TextMeshProUGUI nodeText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private Button nodeButton;

    [SerializeField]
    private GameObject levelIconParent;

    [SerializeField]
    private List<Image> levelIcons;

    public Dictionary<ArchetypeUINode, UILineRenderer.LinePoint> connectedNodes = new Dictionary<ArchetypeUINode, UILineRenderer.LinePoint>();

    private void SetNode()
    {
        for (int i = 0; i < node.maxLevel - 1; i++)
        {
            Image newLevelIcon = Instantiate(levelIcons[0], levelIconParent.transform);
            levelIcons.Add(newLevelIcon);
        }

        if (node.type == NodeType.Greater)
        {
            ((RectTransform)transform).sizeDelta = new Vector2(95, 95);
        }
    }

    public void SetNode(ArchetypeSkillNode n)
    {
        node = n;
        SetNode();

        nodeText.text = "";

        foreach (NodeScalingBonusProperty nodeBonus in node.bonuses)
        {
            nodeText.text += LocalizationManager.Instance.GetBonusTypeString(nodeBonus.bonusType) + "\n";
        }
        foreach (var nodeTrigger in node.triggeredEffects)
        {
            //nodeText.text += LocalizationManager.Instance.GetLocalizationText_TriggeredEffect(nodeTrigger, nodeTrigger.effectMaxValue) + "\n";
        }

        int level = 0;
        levelText.text = level + "/" + node.maxLevel;

        ((RectTransform)transform).anchoredPosition = new Vector3(n.nodePosition.x * 110, n.nodePosition.y * 110 + yPositionOffset, 0);
    }

    public void UpdateNode(HeroArchetypeData archetypeData)
    {
        int level = archetypeData.GetNodeLevel(node);
        levelText.text = level + "/" + node.maxLevel;

        for (int i = 0; i < node.maxLevel; i++)
        {
            if (i < level)
                levelIcons[i].color = level == node.maxLevel ? MAX_LEVEL_COLOR : LEVEL_COLOR;
            else
                levelIcons[i].color = new Color(0.65f, 0.65f, 0.65f);
        }

        if (level == node.maxLevel)
        {
            nodeButton.image.color = new Color(1f, 1f, 1f, 1);
            foreach (var x in connectedNodes)
            {
                if (archetypeData.GetNodeLevel(x.Key.node) == 0)
                    x.Value.color = AVAILABLE_COLOR;
                else
                    x.Value.color = CONNECTED_COLOR;
            }
        }
        else if (level > 0)
        {
            nodeButton.image.color = new Color(1f, 1f, 1f, 1);
            foreach (var x in connectedNodes)
            {
                if (archetypeData.GetNodeLevel(x.Key.node) > 0)
                    x.Value.color = CONNECTED_COLOR;
                else
                    x.Value.color = UNAVAILABLE_COLOR;
            }
        }
        else
        {
            nodeButton.image.color = new Color(0.8f, 0.8f, 0.8f, 1);

            foreach (var x in connectedNodes)
            {
                if (archetypeData.GetNodeLevel(x.Key.node) == x.Key.node.maxLevel)
                    x.Value.color = AVAILABLE_COLOR;
                else
                    x.Value.color = UNAVAILABLE_COLOR;
            }
        }

        MenuUIManager.Instance.ArchetypeWindow.SetUILinesDirty();
    }

    public void EnableNode(bool isLeveled)
    {
        isLevelable = true;

        if (isLeveled)
        {
            nodeButton.image.color = new Color(1f, 1f, 1f, 1);
        }
        else
        {
            nodeButton.image.color = new Color(0.8f, 0.8f, 0.8f, 1);
        }
    }

    public void CheckSurroundingNodes(HeroArchetypeData archetypeData)
    {
        if (archetypeData == null)
        {
            EnablePreviewNode();
            return;
        }

        int nodeLevel = archetypeData.GetNodeLevel(node);

        if (nodeLevel > 0)
        {
            EnableNode(true);
            return;
        }

        foreach (var treeNode in connectedNodes.Keys)
        {
            if (archetypeData.IsNodeMaxLevel(treeNode.node))
            {
                EnableNode(false);
                return;
            }
            else
                DisableNode();
        }
    }

    private void EnablePreviewNode()
    {
        isLevelable = false;
        nodeButton.image.color = new Color(0.8f, 0.8f, 0.8f, 1);
    }

    public void DisableNode()
    {
        isLevelable = false;
        nodeButton.image.color = new Color(0.4f, 0.4f, 0.4f, 1);
    }

    /// <summary>
    /// Checks if there is a proper connected node path while exluding a specified node.
    /// Used to check if a node's children is dependent on it.
    /// </summary>
    /// <param name="uiNode">Node to exclude</param>
    /// <param name="traversedNodes"></param>
    /// <param name="archetypeData"></param>
    /// <returns></returns>
    public bool IsTherePathExcludingNode(ArchetypeUINode uiNode, List<ArchetypeUINode> traversedNodes, HeroArchetypeData archetypeData)
    {
        if (node == null || this == uiNode || traversedNodes.Contains(this))
            return false;

        traversedNodes.Add(this);

        if (node.initialLevel > 0)
            return true;

        foreach (ArchetypeUINode connectedNode in connectedNodes.Keys)
        {
            if (archetypeData.GetNodeLevel(node) > 0 && connectedNode.IsTherePathExcludingNode(uiNode, traversedNodes, archetypeData))
                return true;
        }

        return false;
    }
}