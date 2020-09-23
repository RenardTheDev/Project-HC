using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    public Text label_name;
    public Text label_cost;
    public Button btn_upgrade;

    public UpgradeType type;

    private void OnValidate()
    {
        gameObject.name = $"ship_upgrade_{type}";
        label_name.name = $"label_name_{type}";
        label_cost.name = $"label_cost_{type}";
        btn_upgrade.name = $"btn_upgrade_{type}";

        label_name.text = $"{type}\n100";
        label_cost.text = $"250 hc\n+25%";
    }

    private void Awake()
    {
        btn_upgrade.onClick.AddListener(Upgrade);
    }

    private void Start()
    {

    }

    void Upgrade()
    {
        int cost = ShipUpgrade.dict[type].cost * Ship.PLAYER.shipUpgrades[(int)type];
        if (GameData.cash > cost)
        {
            if (type == UpgradeType.repair)
            {
                float hp_need = Ship.PLAYER.maxHealth - Ship.PLAYER.health;
                int cash_need = Mathf.CeilToInt(hp_need * ShipUpgrade.dict[type].cost);

                Ship.PLAYER.health = Ship.PLAYER.maxHealth;
                cost = cash_need;
            }
            else
            {
                Ship.PLAYER.shipUpgrades[(int)type]++;
            }

            GameData.cash -= cost;
        }

        Ship.PLAYER.ApplyUpgrades();

        GameData.respSave.shipUpgrades = Ship.PLAYER.shipUpgrades;

        PlayerUpgradeScreen.current.UpdateLabels();
    }

    public void UpdateLabels()
    {
        var upg = ShipUpgrade.dict[type];
        int currentLevel = Ship.PLAYER.shipUpgrades[(int)type];
        int level = currentLevel - 1;
        int cash = GameData.cash;

        btn_upgrade.interactable = level < upg.maxlevel && cash >= (level + 1) * upg.cost;
        label_cost.text = level < upg.maxlevel ? $"{(level + 1) * upg.cost}" : "max";

        switch (type)
        {
            case UpgradeType.repair:
                if (Ship.PLAYER != null)
                {
                    label_name.text = $"{upg.Name}";

                    float hp_need = Ship.PLAYER.maxHealth - Ship.PLAYER.health;
                    float cash_need = Mathf.CeilToInt(hp_need * upg.cost);

                    btn_upgrade.interactable = cash >= cash_need && hp_need > 0;
                    label_cost.text = $"{Mathf.CeilToInt(cash_need)}";
                }
                break;
            case UpgradeType.health:
                label_name.text = $"{upg.Name}\n{100 + upg.increment * level}";
                label_cost.text = $"{upg.cost * currentLevel} hc\n+{upg.increment}";
                break;
            /*case UpgradeType.hp_pickup:
                label_cost.text = $"+{upg.increment * level}";
                break;*/
            case UpgradeType.shield:
                label_name.text = $"{upg.Name}\n{upg.increment * level}";
                label_cost.text = $"{upg.cost * currentLevel} hc\n+{upg.increment}";
                break;
            case UpgradeType.sh_regen:
                label_name.text = $"{upg.Name}\n{1f + upg.increment * level} / sec";
                label_cost.text = $"{upg.cost * currentLevel}\n+{upg.increment} / sec";
                break;
            case UpgradeType.speed:
                label_name.text = $"{upg.Name}\n{50 + 0.5f * upg.increment * level}";
                label_cost.text = $"{upg.cost * currentLevel} hc\n+{upg.increment}%";
                break;
            case UpgradeType.resist:
                label_name.text = $"{upg.Name}\n{upg.increment * level}%";
                label_cost.text = $"{upg.cost * currentLevel} hc\n+{upg.increment}%";
                break;
            /*case UpgradeType.damage:
                label_name.text = $"{upg.Name}\n{upg.increment * level}";
                label_cost.text = $"{upg.cost * upg.currentLevel} hc\n+{upg.increment}";
                break;*/
            /*case UpgradeType.skill_cd:
                label_cost.text = $"-{upg.increment * level}%";
                break;*/
            case UpgradeType.guns:
                label_name.text = $"{upg.Name} * {currentLevel}";
                label_cost.text = $"{upg.cost * currentLevel} hc\n+{upg.increment}";

                btn_upgrade.interactable = (level + 1) < upg.maxlevel && cash >= (level + 1) * upg.cost;
                break;
        }
    }
}

public enum UpgradeType
{
    health,
    hp_pickup,
    shield,
    sh_regen,
    speed,
    resist,
    damage,
    skill_cd,
    guns,
    repair
}