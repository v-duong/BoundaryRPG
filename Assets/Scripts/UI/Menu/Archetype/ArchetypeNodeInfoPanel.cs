using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeNodeInfoPanel : MonoBehaviour
{
    private HeroArchetypeData archetypeData;
    private ArchetypeSkillNode node;
    private ArchetypeUINode uiNode;

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI nextInfoText;
    [SerializeField] private TextMeshProUGUI topApText;
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private Button levelButton;
    [SerializeField] private Button delevelButton;
    [SerializeField] private Button resetTreeButton;

    public void SetArchetypeData(HeroArchetypeData data)
    {
        archetypeData = data;
        if (archetypeData == null)
        {
            buttonsParent.SetActive(false);
        }
        else
        {
            buttonsParent.SetActive(true);
        }
    }

    public void SetPanel(ArchetypeUINode uiNode)
    {
        this.uiNode = uiNode;
        node = uiNode.node;

        if (archetypeData == null)
        {
            UpdatePanelPreviewMode();
        }
        else
        {
            UpdatePanel();
        }
    }

    private void UpdatePanel()
    {
        int currentLevel = archetypeData.GetNodeLevel(node);
        if (uiNode.isLevelable && !archetypeData.IsNodeMaxLevel(node) && archetypeData.hero.ArchetypePoints > 0)
            levelButton.interactable = true;
        else
            levelButton.interactable = false;

        if (archetypeData.GetNodeLevel(node) > 0 && node.initialLevel == 0 && IsChildrenIndependent())
            delevelButton.interactable = true;
        else
            delevelButton.interactable = false;

        infoText.text = "";
        nextInfoText.text = "";
        topApText.text = "AP: " + archetypeData.hero.ArchetypePoints;
        nextInfoText.gameObject.SetActive(false);

        if (currentLevel == 0)
        {
            infoText.text += "<b>Level 1: </b>\n";
            infoText.text += node.GetBonusInfoString(1);
            if (node.maxLevel > 1)
            {
                nextInfoText.gameObject.SetActive(true);
                nextInfoText.text += "<b>Level " + (node.maxLevel) + ":</b>\n";
                nextInfoText.text += node.GetBonusInfoString(node.maxLevel);
            }
            return;
        }
        if (currentLevel != 0)
        {
            infoText.text += "<b>Current: Level " + currentLevel + "</b>\n";
            infoText.text += node.GetBonusInfoString(currentLevel);
        }
        if (currentLevel != node.maxLevel)
        {
            nextInfoText.gameObject.SetActive(true);
            nextInfoText.text += "<b>Next: Level " + (currentLevel + 1) + "</b>\n";
            nextInfoText.text += node.GetBonusInfoString(currentLevel + 1);
        }
    }

    private void UpdatePanelPreviewMode()
    {
        infoText.text += "<b>Level 1: </b>\n";

        infoText.text += node.GetBonusInfoString(1);
        if (node.maxLevel != 1)
        {
            nextInfoText.gameObject.SetActive(true);
            nextInfoText.text = "<b>Level " + node.maxLevel + ":</b>\n";

            nextInfoText.text += node.GetBonusInfoString(node.maxLevel);
        }
        else
        {
            nextInfoText.gameObject.SetActive(false);
        }
    }

    public void LevelUpNode()
    {
        if (archetypeData.IsNodeMaxLevel(node) || archetypeData.hero.ArchetypePoints <= 0)
            return;
        archetypeData.LevelUpNode(node);
        archetypeData.hero.ModifyArchetypePoints(-1);
        UpdatePanel();
        uiNode.UpdateNode(archetypeData);
        if (archetypeData.IsNodeMaxLevel(node))
        {
            foreach (ArchetypeUINode uiTreeNode in uiNode.connectedNodes.Keys)
            {
                if (archetypeData.GetNodeLevel(uiTreeNode.node) == 0)
                    uiTreeNode.EnableNode(true);
            }
        }
    }

    public void DelevelNode()
    {
        if (archetypeData.GetNodeLevel(node) == 0 || archetypeData.GetNodeLevel(node) == node.initialLevel)
            return;
        if (archetypeData.IsNodeMaxLevel(node))
        {
            foreach (ArchetypeUINode uiTreeNode in uiNode.connectedNodes.Keys)
            {
                if (archetypeData.GetNodeLevel(uiTreeNode.node) == 0)
                {
                    bool hasAnotherConnection = false;
                    foreach (ArchetypeUINode connectedNode in uiTreeNode.connectedNodes.Keys)
                    {
                        if (connectedNode != uiNode && archetypeData.GetNodeLevel(connectedNode.node) >= connectedNode.node.maxLevel)
                        {
                            hasAnotherConnection = true;
                            break;
                        }
                    }

                    if (!hasAnotherConnection)
                        uiTreeNode.DisableNode();
                }
            }
        }

        archetypeData.DelevelNode(node);
        archetypeData.hero.ModifyArchetypePoints(1);
        UpdatePanel();
        uiNode.UpdateNode(archetypeData);
    }

    private bool IsChildrenIndependent()
    {
        foreach (ArchetypeUINode uiTreeNode in uiNode.connectedNodes.Keys)
        {
            if (archetypeData.GetNodeLevel(uiTreeNode.node) > 0 && !uiTreeNode.IsTherePathExcludingNode(uiNode, new List<ArchetypeUINode>(), archetypeData))
            {
                return false;
            }
        }
        return true;
    }
}