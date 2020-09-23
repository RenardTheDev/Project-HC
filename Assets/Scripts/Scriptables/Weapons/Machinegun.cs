using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Machinegun", menuName = "Scriptables/Weapon/Machinegun")]
public class Machinegun : Weapon
{
    float lastShot;
    int gunID = 0;
    bool playSFX;

    bool reloading;

    float lastReload;

    [Header("Machinegun values")]
    public float reloadDelay = 0.5f;
    public int clip = 10;
    public int clipSize = 10;

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
    }

    public override void SetLevel(int level)
    {
        base.SetLevel(level);
    }

    public override void Trigger()
    {
        if (!reloading)
        {
            if (Time.time - lastShot > (60f / lvld_fireRate) / (owner.shipUpgrades[(int)UpgradeType.guns]))
            {
                MakeShot();
            }
        }
    }

    public void MakeShot()
    {
        float shotDeviation = Random.Range(-1.0f, 1.0f) * lvld_spread;

        Vector3 origin = trans.position;

        if (owner.shipUpgrades[(int)UpgradeType.guns] > 1 && gunID < 2)
        {
            origin = trans.TransformPoint((gunID % 2 == 0 ? Vector3.right : Vector3.left) * 0.5f);
        }

        ProjectileSystem.current.SpawnProjectile(origin, trans.eulerAngles.z + shotDeviation, owner, this);
        playSFX = true;

        lastShot = Time.time;

        gunID = gunID >= owner.shipUpgrades[(int)UpgradeType.guns] - 1 ? 0 : gunID + 1;

        clip--;
        if (clip == 0) { reloading = true; lastReload = Time.time; }
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