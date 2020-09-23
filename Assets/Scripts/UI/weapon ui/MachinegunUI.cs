using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachinegunUI : WeaponUI
{
    public Text label_name;
    public Text label_lvl;
    public Image fill_clip;

    public Machinegun weapon;

    public override void AssignWeapon(Weapon weap)
    {
        weapon = (Machinegun)weap;
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
        fill_clip.fillAmount = (float)weapon.clip / weapon.clipSize;
    }
}