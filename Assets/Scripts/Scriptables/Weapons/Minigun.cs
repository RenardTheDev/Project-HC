using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Minigun", menuName = "Scriptables/Weapon/Minigun")]
public class Minigun : Weapon
{
    [Header("Minigun values")]
    public float heat;
    public float hIncrement = 0.02f;
    public float coolSpeed = 0.1f;

    public float cdDelay = 0.25f;

    public bool overHeated;
    bool shooting;

    public AnimationCurve fireRateOverHeat;
    public AnimationCurve spreadOverHeat;

    public AudioClip overheatSFX;

    //public AnimationCurve overheatFirerateDrop;

    float lastShot;
    bool playSFX;

    public override void OnAssigned(Ship ship)
    {
        base.OnAssigned(ship);
    }

    public override void Trigger()
    {
        /*base.Trigger();

        if (Time.time - lastShot > ((60f / lvld_fireRate) * fireRateOverHeat.Evaluate(heat)) / owner.shipUpgrades[(int)UpgradeType.guns] && !overHeated)
        {
            MakeShot();
        }*/
        if (!shooting)
        {
            shooting = true;
        }
    }

    public override void SetLevel(int level)
    {
        base.SetLevel(level);
    }

    public override void Update()
    {
        base.Update();

        if (Time.time - lastShot > cdDelay)
        {
            heat = Mathf.MoveTowards(heat, 0, Time.deltaTime * coolSpeed);
        }

        if (overHeated && heat < 0.5f)
        {
            overHeated = false;
        }

        if (playSFX)
        {
            owner.sfx_source.pitch = 0.9f + Random.value * 0.2f;
            owner.sfx_source.PlayOneShot(sfx, volume);
            playSFX = false;
        }

        if (shooting)
        {
            if (owner.isAlive && Time.time > lastShot + ((60f / lvld_fireRate) * fireRateOverHeat.Evaluate(heat)) && !overHeated)
            {
                _makeShot();
                shooting = false;
            }
        }
    }

    public void _makeShot()
    {
        float shotDeviation = Random.Range(-1.0f, 1.0f) * lvld_spread * spreadOverHeat.Evaluate(heat);

        Vector3 origin = trans.position;

        ProjectileSystem.current.SpawnProjectile(origin, trans.eulerAngles.z + shotDeviation, owner, this);
        playSFX = true;

        lastShot = Time.time;

        //--- heat ---
        heat += hIncrement;
        if (heat >= 1f)
        {
            overHeated = true;
            owner.sfx_source.PlayOneShot(overheatSFX, 0.1f);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Minigun))]
public class MinigunEditor : Editor
{
    Minigun script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (Minigun)target;
        base.OnInspectorGUI();

        float dmg = script.damage;
        float rpm = script.fireRate;
        float dps = (dmg * rpm) / 60f;
        float ttk = 100f / dps;

        EditorGUILayout.Space();
        GUILayout.Label("Stats:");
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