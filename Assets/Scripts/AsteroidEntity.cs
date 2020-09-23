using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AsteroidEntity : MonoBehaviour
{
    public AsteroidSize size = AsteroidSize.big;

    public AsteroidSize onDeathSpawnSize = AsteroidSize.tiny;
    public int onDeathSpawnCount;

    public float health = 100f;
    public float maxHealth = 100;

    public Rigidbody2D rig;
    SpriteRenderer sprRend;

    private void Awake()
    {
        sprRend = GetComponent<SpriteRenderer>();
    }

    public void ApplyDamage(float dmg, Ship attacker)
    {
        if (attacker != null)
        {
            dmg = dmg * attacker.damageMult;
        }

        if (health > dmg)
        {
            health -= dmg;
        }
        else
        {
            health = 0;

            GlobalEvents.AsteroidDestroyed(this);
            ParticleEffects.current.SpawnAsteroidDebries(transform.position, sprRend);

            if (onDeathSpawnCount > 0)
            {
                for (int i = 0; i < onDeathSpawnCount; i++)
                {
                    Vector3 rnd = Random.insideUnitCircle.normalized;
                    AsteroidSystem.current.SpawnAsteroid(transform.position + rnd, onDeathSpawnSize, false);
                }
            }
        }
    }
}

public enum AsteroidSize
{
    big,
    medium,
    small,
    tiny
}

#if UNITY_EDITOR
[CustomEditor(typeof(AsteroidEntity))]
public class AsteroidEntityEditor : Editor
{
    AsteroidEntity script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (AsteroidEntity)target;

        if (GUILayout.Button("Update"))
        {
            script.rig = script.GetComponent<Rigidbody2D>();
            script.maxHealth = script.rig.mass;
            script.health = script.maxHealth;
        }

        base.OnInspectorGUI();
    }
}
#endif