using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class GameDataManager 
{
    public static GameData data;

    public static void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/game.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/game.dat");
            data = (GameData)bf.Deserialize(file);

            file.Close();
        }
    }

    public static void SaveGame()
    {
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
        bf.Serialize(file, data);
        file.Close();
    }
}

public class GameData
{

}