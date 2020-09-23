
using System.Collections.Generic;

[System.Serializable]
public class ShipUpgrade
{
    public static Dictionary<UpgradeType, ShipUpgrade> dict;

    public static void GenerateUpgrades()
    {
        dict = new Dictionary<UpgradeType, ShipUpgrade>();
        foreach (int upg in System.Enum.GetValues(typeof(UpgradeType)))
        {
            dict.Add((UpgradeType)upg, new ShipUpgrade((UpgradeType)upg));
        }
    }

    public string Name = "Parameter";
    public int cost = 1000;
    public float increment = 1f;
    public int maxlevel = 1;

    public ShipUpgrade(UpgradeType type)
    {
        maxlevel = int.MaxValue;

        switch (type)
        {
            case UpgradeType.health:
                Name = "health";
                cost = 500;
                increment = 10f;
                break;

            case UpgradeType.hp_pickup:
                Name = "health pickup";
                cost = 750;
                increment = 10f;
                break;

            case UpgradeType.shield:
                Name = "shield";
                cost = 500;
                increment = 5f;
                break;

            case UpgradeType.sh_regen:
                Name = "shield regen";
                cost = 250;
                increment = 0.25f;
                break;

            case UpgradeType.speed:
                Name = "move speed";
                cost = 250;
                increment = 5f;
                maxlevel = 50;
                break;

            case UpgradeType.resist:
                Name = "damage resistance";
                cost = 750;
                increment = 3f;
                maxlevel = 33;
                break;

            case UpgradeType.damage:
                Name = "damage";
                cost = 600;
                increment = 5f;
                break;

            case UpgradeType.skill_cd:
                Name = "skill cooldown";
                cost = 1000;
                increment = 1f;
                maxlevel = 25;
                break;

            case UpgradeType.guns:
                Name = "gun on board";
                cost = 25000;
                increment = 1f;
                maxlevel = 3;
                break;

            case UpgradeType.repair:
                Name = "repair ship";
                cost = 10;
                break;
        }
    }
}
