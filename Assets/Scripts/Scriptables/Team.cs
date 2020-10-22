using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Team", menuName = "Scriptables/Team")]
public class Team : ScriptableObject
{
    public string teamName = "";
    public Color teamColor;
    public Sprite[] teamSkins;

    public Sprite GetRandomSkin()
    {
        return teamSkins[Random.Range(0, teamSkins.Length)];
    }
}
