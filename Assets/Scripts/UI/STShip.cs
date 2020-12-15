using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class STShip : ScreenTag
{
    [HideInInspector]
    public Ship target;

    [Header("Graphics")]
    public GameObject go_health;
    public GameObject go_shield;

    public Image ui_health;
    public Image ui_shield;

    public void Assign(Transform target_transform, Ship target_ship)
    {
        base.Assign(target_transform, target_ship.Name);
        target = target_ship;

        ui_target.color = GameManager.inst.team[target.teamID].teamColor;
        nameTag.color = ui_target.color;
    }

    public override void UpdateVisibility()
    {
        base.UpdateVisibility();

        nameTag.enabled = isOnScreen && target.pilot.data.important;

        bool showHP = isOnScreen;
        if (go_health.activeSelf != showHP) go_health.SetActive(showHP);
        bool showSP = isOnScreen && target.maxShield > 0;
        if (go_shield.activeSelf != showSP) go_shield.SetActive(showSP);

        if (isOnScreen)
        {
            rt_icon.rotation = Quaternion.identity;

            ui_health.fillAmount = target.health / target.maxHealth;
            ui_shield.fillAmount = target.shield / target.maxShield;
        }
        else
        {
            rt_icon.rotation = Quaternion.Euler(0, 0, t_trans.eulerAngles.z);
        }
    }

    public override void UpdatePosition()
    {
        base.UpdatePosition();


    }
}
