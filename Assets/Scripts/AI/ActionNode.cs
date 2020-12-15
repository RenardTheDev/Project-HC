using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode : Node
{
    public delegate NodeStates ActionNodeDelegate();

    private ActionNodeDelegate m_action;

    public ActionNode(ActionNodeDelegate action)
    {
        m_action = action;
    }

    public override NodeStates Evaluate()
    {
        switch (m_action())
        {
            case NodeStates.FAILURE:
                m_nodeState = NodeStates.FAILURE;
                return m_nodeState;

            case NodeStates.SUCCESS:
                m_nodeState = NodeStates.SUCCESS;
                return m_nodeState;

            case NodeStates.RUNNING:
                m_nodeState = NodeStates.RUNNING;
                return m_nodeState;

            default:
                m_nodeState = NodeStates.FAILURE;
                return m_nodeState;
        }
    }
}
