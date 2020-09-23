using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkillBase : ScriptableObject
{
    [HideInInspector] public Ship owner;

    public string Name = "Skill";
    public float recoverSpeed = 0.1f;

    public virtual void OnAssigned(Ship ship)
    {
        owner = ship;
    }

    public virtual void Activate()
    {
        Debug.Log("Skill activated");
    }
}
