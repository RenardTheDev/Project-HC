using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameRenderHelper : MonoBehaviour
{
    public static GameRenderHelper current;

    Camera cam;
    public RenderTexture render;

    public int targetResolution = 240;

    private void Awake()
    {
        current = this;
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateResolution();
        }
    }

    public void UpdateResolution()
    {
        float scrRatio = (float)Screen.width / Screen.height;
        render.Release();

        render.height = targetResolution;
        render.width = Mathf.RoundToInt(targetResolution * scrRatio);

        if (render.width <= 0) render.width = targetResolution;

        render.Create();
    }
}
