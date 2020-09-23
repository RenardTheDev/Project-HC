using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    Renderer render;
    public float parallax_mul = 1f;
    float quadSize;

    private void Awake()
    {
        render = GetComponent<Renderer>();
        uvScale = render.material.GetTextureScale("_MainTex");
    }

    private void Start()
    {

    }

    Vector2 parallax;
    Vector2 uvScale;

    private void LateUpdate()
    {
        quadSize = transform.localScale.x;

        parallax.x = Mathf.Repeat(parallax_mul * (transform.position.x / (quadSize / uvScale.x)), 1);
        parallax.y = Mathf.Repeat(parallax_mul * (transform.position.y / (quadSize / uvScale.y)), 1);

        render.material.SetTextureOffset("_MainTex", parallax);
    }
}
