using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigunUI : WeaponUI
{
    public Text label_name;
    public Text label_lvl;
    public Image fill_heat;

    public Gradient color;

    public Minigun weapon;

    public override void AssignWeapon(Weapon weap)
    {
        weapon = (Minigun)weap;
    }

    private void Update()
    {
        int guns = Ship.PLAYER.shipUpgrades[(int)UpgradeType.guns];
        if (guns > 1)
        {
            label_name.text = $"{weapon.Name}x{guns}";
        }
        else
        {
            label_name.text = $"{weapon.Name}";
        }

        label_lvl.text = $"^{weapon.level}";

        label_name.color = weapon.overHeated ? color.Evaluate(1) : Color.white;
        label_lvl.color = weapon.overHeated ? color.Evaluate(1) : Color.white;

        fill_heat.fillAmount = weapon.heat;
        fill_heat.color = color.Evaluate(fill_heat.fillAmount);
    }
}