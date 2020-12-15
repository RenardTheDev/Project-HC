using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;

public class BuildDateSaver : IPreprocessBuildWithReport
{
    RectTransform textParent;
    Text buildDate;

    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        buildDate = GameObject.Find("text_buildDate").GetComponent<Text>();
        textParent = buildDate.transform.parent.GetComponent<RectTransform>();

        var culture = new CultureInfo("ru-RU");
        var localDate = DateTime.Now;
        buildDate.text = $"{localDate.ToString(culture)}";
        textParent.sizeDelta = new Vector2(buildDate.preferredWidth + 8, buildDate.preferredHeight * 4);
    }
}