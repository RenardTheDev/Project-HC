using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicTheme", menuName = "Scriptables/MusicThemeAsset")]
public class MusicTheme : ScriptableObject
{
    public AudioClip ost;
    public AudioClip[] ambient_loop;
    public MusicLoop[] action_loop;

    public int currActionLoop;
}

[System.Serializable]
public class MusicLoop
{
    public AudioClip loopStart;
    public AudioClip loopBody;
    public AudioClip loopEnd;
}