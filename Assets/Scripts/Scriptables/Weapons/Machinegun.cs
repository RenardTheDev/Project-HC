using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Machinegun", menuName = "Scriptables/Weapon/Machinegun")]
public class Machinegun : Weapon
{
    float lastShot;
    bool playSFX;

    bool reloading;
    bool shooting;

    float lastReload;

    [Header("Machinegun values")]
    public float reloadDelay = 0.5f;
    public int clip = 10;
    public int clipSize = 10;

    public float autoReloadDelay = 1;
    public float autoReloadSpeed = 5;   //rpm
    float lastAutoReload;

    private void Awake()
    {

    }

    public override void OnAssigned(Ship ship)
    {
        base.OnAssigned(ship);
    }

    public override void Update()
    {
        if (playSFX)
        {
            owner.sfx_source.pitch = 0.9f + Random.value * 0.2f;
            owner.sfx_source.PlayOneShot(sfx, volume);
            playSFX = false;
        }

        if (reloading)
        {
            if (Time.time - lastReload > reloadDelay)
            {
                reloading = false;
                clip = clipSize;
            }
        }
        else
        {
            if (shooting)
            {
                if (owner.isAlive && Time.time > lastShot + (60f / lvld_fireRate))
                {
                    _makeShot();

                    clip--;
                    shooting = false;
                    if (clip <= 0)
                    {
                        reloading = true;
                        lastReload = Time.time;
                    }
                }
            }
            else
            {
                if (Time.time - lastShot > autoReloadDelay && clip < clipSize)
                {
                    if (Time.time - lastAutoReload > autoReloadSpeed)
                    {
                        clip++;
                        lastAutoReload = Time.time;
                    }
                }
            }
        }
    }

    public override void SetLevel(int level)
    {
        base.SetLevel(level);
    }

    public override void Trigger()
    {
        if (!reloading && !shooting)
        {
            shooting = true;
        }
    }

    public void _makeShot()
    {
        float shotDeviation = Random.Range(-1.0f, 1.0f) * spread;

        Vector3 origin = trans.position;

        ProjectileSystem.current.SpawnProjectile(origin, trans.eulerAngles.z + shotDeviation, owner, this);
        playSFX = true;

        lastShot = Time.time;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Machinegun))]
public class MachinegunEditor : Editor
{
    Machinegun script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (Machinegun)target;
        base.OnInspectorGUI();

        float dmg = script.damage;
        float rpm = script.fireRate;
        float dps = (dmg * rpm) / 60f;
        float ttk = 100f / dps;

        script.autoReloadSpeed = script.reloadDelay / script.clipSize;

        EditorGUILayout.Space();
        GUILayout.Label("Base stats:");
        GUILayout.Label(
            $"DMG = {dmg:0.00}\n" +
            $"RPM = {rpm:0.00}\n" +
            $"DPS = {dps:0.00}\n" +
            $"TTK = {ttk:0.00}"
            );

        dmg = script.lvld_damage;
        rpm = script.lvld_fireRate;
        dps = (dmg * rpm) / 60f;
        ttk = 100f / dps;

        GUILayout.Label("Leveled stats:");
        GUILayout.Label(
            $"DMG = {dmg:0.00}\n" +
            $"RPM = {rpm:0.00}\n" +
            $"DPS = {dps:0.00}\n" +
            $"TTK = {ttk:0.00}"
            );
    }
}
#endif