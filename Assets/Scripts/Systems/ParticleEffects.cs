using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffects : MonoBehaviour
{
    public static ParticleEffects current;

    public ParticleSystem ps_explosion;
    public ParticleSystem ps_small_explosion;
    public ParticleSystem ps_fire;
    public ParticleSystem ps_asteroidBreak;

    public ParticleSystem ps_impact_plasma;
    private void Awake()
    {
        current = this;
    }

    public void SpawnExplosion(Vector3 pos)
    {
        pos.z = 0;
        ps_explosion.transform.position = pos;
        ps_explosion.Emit(1);
    }

    public void SpawnSmallExplosion(Vector3 pos)
    {
        pos.z = 0;
        ps_small_explosion.transform.position = pos;
        ps_small_explosion.Emit(1);
    }

    public void SpawnFire(Vector3 pos)
    {
        pos.z = 0;
        ps_fire.transform.position = pos;
        ps_fire.Emit(1);
    }

    public void SpawnAsteroidDebries(Vector3 pos, SpriteRenderer spr)
    {
        ps_asteroidBreak.transform.position = pos;

        var shape = ps_asteroidBreak.shape;
        shape.shapeType = ParticleSystemShapeType.SpriteRenderer;
        shape.spriteRenderer = spr;

        int size = Mathf.CeilToInt(spr.bounds.size.magnitude);

        var main = ps_asteroidBreak.main;
        main.startSizeMultiplier = size * 0.2f;

        //Debug.Log($"sprite size = {size}");

        ps_asteroidBreak.Emit(size);
    }

    public void SoawnImpactPlasma(Vector3 pos, Color color)
    {
        ps_impact_plasma.transform.position = pos;

        var psMain = ps_impact_plasma.main;
        psMain.startColor = color;

        ps_impact_plasma.Emit(Random.Range(3, 5));
    }
}