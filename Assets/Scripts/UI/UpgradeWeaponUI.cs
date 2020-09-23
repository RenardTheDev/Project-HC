using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeWeaponUI : MonoBehaviour
{
    public Toggle toggle_install;
    public Text label_name;
    public Text label_cost;
    public Button btn_upgrade;
    public Text btn_label;

    public Weapon weapon;

    private void Awake()
    {
        btn_upgrade.onClick.AddListener(Upgrade);
        toggle_install.onValueChanged.AddListener(Install);
    }

    private void Start()
    {

    }

    private void OnValidate()
    {
        toggle_install.name = $"toggle_install_{weapon.Name}";
        label_name.name = $"label_name_{weapon.Name}";
        label_cost.name = $"label_cost_{weapon.Name}";
        btn_upgrade.name = $"btn_upgrade_{weapon.Name}";
        btn_label.name = $"label_upgrade_{weapon.Name}";

        toggle_install.interactable = false;
        toggle_install.isOn = false;
    }

    public void Install(bool toggle)
    {
        if (toggle)
        {
            if (Ship.PLAYER.weapUpgrades[weapon.WEAPON_ID] > 0)
            {
                Ship.PLAYER.weap.AssignWeapon(weapon);
            }
        }

        Ship.PLAYER.weap.UpdateWeaponUpgrade();
        GameData.respSave.equippedWeapID = Ship.PLAYER.equippedWeapID;

        PlayerUpgradeScreen.current.UpdateLabels();
    }

    public void Upgrade()
    {
        if (Ship.PLAYER.weapUpgrades[weapon.WEAPON_ID] <= 0)
        {
            Ship.PLAYER.weapUpgrades[weapon.WEAPON_ID] = 1;
            GameData.cash -= weapon.BuyCost;
        }
        else
        {
            GameData.cash -= weapon.levelCost * Ship.PLAYER.weapUpgrades[weapon.WEAPON_ID];
            Ship.PLAYER.weapUpgrades[weapon.WEAPON_ID]++;
        }

        Ship.PLAYER.weap.UpdateWeaponUpgrade();

        GameData.respSave.weapUpgrades = Ship.PLAYER.weapUpgrades;

        PlayerUpgradeScreen.current.UpdateLabels();
    }

    public void UpdateLabels()
    {
        int level = Ship.PLAYER.weapUpgrades[weapon.WEAPON_ID];
        int cash = GameData.cash;

        toggle_install.interactable = level > 0;
        toggle_install.isOn = Ship.PLAYER.equippedWeapID == weapon.WEAPON_ID;

        if (level <= 0)
        {
            btn_upgrade.interactable = cash >= weapon.BuyCost;
            label_name.text = $"{weapon.Name}";
            label_cost.text = $"{weapon.BuyCost} hc";
        }
        else
        {
            btn_upgrade.interactable = cash >= weapon.levelCost * level;
            label_name.text = $"{weapon.Name}\nlevel {level}";
            label_cost.text = $"{weapon.levelCost * level} hc";
        }
        btn_label.text = level == 0 ? "buy" : "upgrade";
    }
}