using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeWindow : MonoBehaviour
{
    private static Vector3 LineOffsetY = Vector3.zero;
    private Vector3 treeStartingView = Vector3.zero;
    private Dictionary<ArchetypeSkillNode, ArchetypeUINode> nodeDict = new Dictionary<ArchetypeSkillNode, ArchetypeUINode>();
    private ArchetypeBase currentBase;

    public Sprite LargeNodeImage;

    [SerializeField]
    private ArchetypeUINode nodePrefab;

    [SerializeField]
    private UILineRenderer treeParent;

    [SerializeField]
    private ArchetypeNodeInfoPanel infoPanel;

    private float largestX = 0, largestY = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        RectTransform r = nodePrefab.transform as RectTransform;
        LineOffsetY.y = r.rect.height / 2;
    }

    public void SetUILinesDirty()
    {
        treeParent.SetVerticesDirty();
    }

    public void BuildTree(ArchetypeBase archetypeBase, HeroArchetypeData archetypeData)
    {
        currentBase = archetypeBase;
        ResetTreeView();
        infoPanel.SetArchetypeData( archetypeData);

        HashSet<ArchetypeSkillNode> traversedNodes = new HashSet<ArchetypeSkillNode>();
        largestX = 0;
        largestY = 0;
        Vector3 homeNodePosition = Vector3.zero;

        foreach (ArchetypeSkillNode node in archetypeBase.nodeList)
        {
            if (node.initialLevel == 1)
            {
                CreateTreeNode(node, traversedNodes);
            }
        }

        treeParent.rectTransform.sizeDelta = new Vector2(largestX * 250, largestY * 160);

        foreach (var uiNode in nodeDict.Values)
        {
            if (archetypeData != null)
                uiNode.UpdateNode(archetypeData);

            uiNode.CheckSurroundingNodes(archetypeData);
            if (uiNode.node.id == 0)
                homeNodePosition = (uiNode.transform as RectTransform).anchoredPosition;
        }

        (treeParent.transform as RectTransform).anchoredPosition = -homeNodePosition;
        treeParent.transform.localScale = Vector3.one;
    }

    public void ResetTreeView()
    {
        foreach (var node in nodeDict.Values)
        {
            node.gameObject.SetActive(false);
            Destroy(node.gameObject);
        }

        nodeDict.Clear();
        treeParent.Points.Clear();
    }

    private ArchetypeUINode CreateTreeNode(ArchetypeSkillNode node, HashSet<ArchetypeSkillNode> traversedNodes)
    {
        if (node == null)
            return null;

        ArchetypeUINode currentNode;

        // Check if node has been traversed yet
        // if already created, just return the node
        if (traversedNodes.Add(node))
        {
            currentNode = Instantiate(nodePrefab, treeParent.transform);
            currentNode.GetComponent<Button>().onClick.AddListener(() => infoPanel.SetPanel(currentNode));
            currentNode.SetNode(node);
            nodeDict.Add(node, currentNode);
            if (Math.Abs(node.nodePosition.x) > largestX)
                largestX = Math.Abs(node.nodePosition.x);
            if (Math.Abs(node.nodePosition.y) > largestY)
                largestY = Math.Abs(node.nodePosition.y);
        }
        else
        {
            return nodeDict[node];
        }

        foreach (int x in node.children)
        {
            ArchetypeSkillNode n = currentBase.GetNode(x);
            ArchetypeUINode child = CreateTreeNode(n, traversedNodes);
            UILineRenderer.LinePoint point = new UILineRenderer.LinePoint(currentNode.transform.localPosition + LineOffsetY, child.transform.localPosition + LineOffsetY, Color.black);

            currentNode.connectedNodes.Add(child, point);
            child.connectedNodes.Add(currentNode, point);

            treeParent.AddPoints(point);
        }

        return currentNode;
    }
}