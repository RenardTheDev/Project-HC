using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class NameGenerator : MonoBehaviour
{
    public List<char> vowels;
    public List<char> unvowels;

    public string GeneratedName;

    [HideInInspector] public IntRange word_length = new IntRange();
    [HideInInspector] public int maxVowels;
    [HideInInspector] public int maxUnvowels;

    public void Generate(IntRange length, int maxVowels, int maxUnvowels)
    {
        GeneratedName = "";

        int vowComb = 0;
        int unvComb = 0;

        int len = Random.Range(length.min, length.max);

        for (int i = 0; i < len; i++)
        {
            bool vowel = Random.value > 0.5;
            if (vowel)
            {
                vowComb++;
                if (vowComb > maxVowels)
                {
                    vowComb = 0;
                    vowel = false;
                    unvComb++;
                    if (unvComb > maxUnvowels)
                    {
                        unvComb = 0;
                        vowel = true;
                        vowComb++;
                    }
                }
            }

            GeneratedName += vowel ? vowels[Random.Range(0, vowels.Count)] : unvowels[Random.Range(0, unvowels.Count)];
        }
    }

    public void Generate()
    {
        Generate(word_length, maxVowels, maxUnvowels);
    }
}

public struct FloatRange
{
    public float min;
    public float max;
}

public struct IntRange
{
    public int min;
    public int max;
}


#if UNITY_EDITOR
[CustomEditor(typeof(NameGenerator))]
public class NameGeneratorEditor : Editor
{
    NameGenerator script;

    public override void OnInspectorGUI()
    {
        if (script == null) script = (NameGenerator)target;
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        script.word_length.min = EditorGUILayout.IntField("Word length min", script.word_length.min);
        script.word_length.max = EditorGUILayout.IntField("Word length max", script.word_length.max);
        script.maxVowels = EditorGUILayout.IntField("Max vowels", script.maxVowels);
        script.maxUnvowels = EditorGUILayout.IntField("Max not vowels", script.maxUnvowels);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate()"))
        {
            script.Generate();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
            EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
        }
    }
}
#endif