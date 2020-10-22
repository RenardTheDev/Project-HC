using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class NameGenerator : MonoBehaviour
{
    public static List<char> vowels = new List<char>(new char[] { 'a', 'e', 'i', 'o', 'u', 'y' });
    public static List<char> unvowels = new List<char>(new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' });

    static int word_length_min = 4;
    static int word_length_max = 8;
    static int maxVowels = 2;
    static int maxUnvowels = 1;

    public static string Generate()
    {
        string GeneratedName = "";

        int vowComb = 0;
        int unvComb = 0;

        int len = Random.Range(word_length_min, word_length_max + 1);

        for (int i = 0; i < len; i++)
        {
            bool vowel = Random.value > 0.5f;

            if (vowel)
            {
                if (vowComb >= maxVowels)
                {
                    vowel = false;
                    vowComb = -1;
                }
                vowComb++;
            }
            else
            {
                if (unvComb >= maxUnvowels)
                {
                    vowel = true;
                    unvComb = -1;
                }
                unvComb++;
            }

            GeneratedName += vowel ? vowels[Random.Range(0, vowels.Count)] : unvowels[Random.Range(0, unvowels.Count)];
        }

        return GeneratedName.First().ToString().ToUpper() + GeneratedName.Substring(1);
    }

    public static void Setup(int word_length_min, int word_length_max, int maxVowels, int maxUnvowels)
    {
        NameGenerator.word_length_min = word_length_min;
        NameGenerator.word_length_max = word_length_max;
        NameGenerator.maxVowels = maxVowels;
        NameGenerator.maxUnvowels = maxUnvowels;
    }
}