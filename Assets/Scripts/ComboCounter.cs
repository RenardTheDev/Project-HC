using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboCounter : MonoBehaviour
{
    private void Awake()
    {
        GlobalEvents.OnShipKilled += OnShipKilled;
    }

    private void OnShipKilled(Damage dmg)
    {
        if (dmg.attacker != null && dmg.attacker.isPlayer && dmg.victim != null && dmg.victim.teamID != dmg.attacker.teamID)
        {
            Action_Combo.Accumulate();
        }
    }

    private void Update()
    {
        Action_Combo.Update();
    }
}

public static class Action_Combo
{
    public static float comboTimer;
    public static int combo = 1;

    public static void Accumulate()
    {
        if (comboTimer > 0)
        {
            combo++;
        }
        comboTimer = 1;
    }

    public static void Update()
    {
        if (combo > 1)
        {
            comboTimer = Mathf.MoveTowards(comboTimer, 0f, Time.deltaTime * Mathf.Pow(combo, 0.5f) * 0.2f);

            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
        else
        {
            comboTimer = Mathf.MoveTowards(comboTimer, 0f, Time.deltaTime * 0.25f);
        }
    }

    public static void ResetCombo()
    {
        combo = 1;
        comboTimer = 0;
    }
}