using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class GameData 
{
    public static int wave;
    public static int score;
    public static int cash;
    public static int kills;

    public static SavedGame saved;
    public static RespSavedData respSave;

    public static void GameStartUp()
    {
        wave = 0;
        score = 0;
        cash = 0;
        kills = 0;
        
        LoadGame();
    }

    public static void NewGameReset()
    {
        wave = 0;
        score = 0;
        cash = 0;
        kills = 0;

        respSave = new RespSavedData();
    }

    public static void ContinueGame()
    {
        wave = saved.wave;
        score = saved.score;
        cash = saved.cash;
        kills = saved.kills;

        respSave = new RespSavedData();
    }

    public static void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/game.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/game.dat");
            saved = (SavedGame)bf.Deserialize(file);

            file.Close();

            PlayerShipUI.current.btn_continue.interactable = true;
            PlayerShipUI.current.btn_continue_label.text = $"> wave {saved.wave + 1}";
        }
    }

    public static void SaveGame()
    {
        saved = new SavedGame();

        saved.equippedWeapID = Ship.PLAYER.equippedWeapID;

        saved.shipUpgrades = Ship.PLAYER.shipUpgrades;
        saved.weapUpgrades = Ship.PLAYER.weapUpgrades;

        saved.wave = wave;
        saved.score = score;
        saved.cash = cash;
        saved.kills = kills;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (File.Exists(Application.persistentDataPath + "/game.dat"))
        {
            file = File.OpenWrite(Application.persistentDataPath + "/game.dat");
            Debug.Log("save file updated");
        }
        else
        {
            file = File.Create(Application.persistentDataPath + "/game.dat");
            Debug.Log("save file created");
        }
        bf.Serialize(file, saved);
        file.Close();
    }
}

[System.Serializable]
public class SavedGame
{
    public Dictionary<int, int> shipUpgrades;
    public Dictionary<int, int> weapUpgrades;

    public int equippedWeapID;

    public int wave;
    public int score;
    public int cash;
    public int kills;

    public SavedGame()
    {
        equippedWeapID = 0;

        wave = 0;
        score = 0;
        cash = 0;
        kills = 0;

        shipUpgrades = new Dictionary<int, int>();
        foreach (int upg in System.Enum.GetValues(typeof(UpgradeType)))
        {
            shipUpgrades.Add(upg, 1);
        }

        weapUpgrades = new Dictionary<int, int>();
        foreach (var weap in GameManager.current.weapons)
        {
            weapUpgrades.Add(weap.WEAPON_ID, 0);
        }
    }
}

public class RespSavedData
{
    public Dictionary<int, int> shipUpgrades;
    public Dictionary<int, int> weapUpgrades;

    public int equippedWeapID;
}