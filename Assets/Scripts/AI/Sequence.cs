using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    protected List<Node> m_nodes = new List<Node>();

    public Sequence(List<Node> nodes)
    {
        m_nodes = nodes;
    }

    public override NodeStates Evaluate()
    {
        bool anyChildrenRunning = false;

        foreach (Node node in m_nodes)
        {
            switch (node.Evaluate())
            {
                case NodeStates.FAILURE:
                    m_nodeState = NodeStates.FAILURE;
                    return m_nodeState;

                case NodeStates.SUCCESS:
                    continue;

                case NodeStates.RUNNING:
                    anyChildrenRunning = true;
                    continue;

                default:
                    m_nodeState = NodeStates.SUCCESS;
                    return m_nodeState;
            }
        }

        m_nodeState = anyChildrenRunning ? NodeStates.RUNNING : NodeStates.SUCCESS;
        return m_nodeState;
    }
}
