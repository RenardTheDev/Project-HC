using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Weapon : ScriptableObject
{
    [HideInInspector] public Ship owner;
    [HideInInspector] public Transform trans;

    public int WEAPON_ID = -1;

    public string Name = "The Gun";
    public Sprite bulletSprite;
    public AudioClip sfx;
    public float volume = 0.1f;

    public GameObject ui_prefab;

    public float damage = 1;
    public float fireRate = 0.2f;
    public float spread = 0.1f;

    public float lvld_damage;
    public float lvld_fireRate;
    public float lvld_spread;


    //--- leveling ---
    public int BuyCost;
    public int levelCost;

    public int level = 1;

    [Space]
    public WeaponProjectileParameters projectile;

    public virtual void OnAssigned(Ship ship)
    {
        owner = ship;
        trans = ship.transform;
    }

    public virtual void Trigger()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void SetLevel(int newLevel)
    {
        level = newLevel >= 1 ? newLevel : 1;
        UpdateStats();
    }

    private void OnValidate()
    {
        UpdateStats();
    }

    void UpdateStats()
    {
        int lvl = level - 1 >= 0 ? level - 1 : 0;

        lvld_fireRate = fireRate + (lvl * 0.01f * fireRate);
        lvld_damage = damage + (lvl * 0.05f * damage);
        lvld_spread = Mathf.Clamp(spread - (lvl * 0.01f * spread), 0, float.MaxValue);
    }
}

[System.Serializable]
public class WeaponProjectileParameters
{
    public float speed = 50f;
    public float distance = 50f;
    public float radius = 0.5f;
    public Color color = Color.white;

    public ImpactType impact;
}

public enum ImpactType
{
    none,
    explosive,
    plasma
}