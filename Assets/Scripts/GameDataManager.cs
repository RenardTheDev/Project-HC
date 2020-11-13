using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class GameDataManager
{
    public static GameData data;

    public static Dictionary<Station, StationEntity> station_ent = new Dictionary<Station, StationEntity>();

    public static bool CheckForSavedGame()
    {
        return File.Exists(Application.persistentDataPath + "/game.dat");
    }

    public static void LoadGame()
    {
        if (CheckForSavedGame())
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/game.dat");
            data = (GameData)bf.Deserialize(file);

            file.Close();
        }

        Debug.Log("Saved data DEBUG");
    }

    public static void SaveGame()
    {
        for (int i = 0; i < data.pilots.Count; i++)
        {
            data.pilots[i].SavePosition();
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (CheckForSavedGame())
        {
            file = File.OpenWrite(Application.persistentDataPath + "/game.dat");
            Debug.Log("game save file updated");
        }
        else
        {
            file = File.Create(Application.persistentDataPath + "/game.dat");
            Debug.Log("game save file created");
        }
        bf.Serialize(file, data);
        file.Close();
    }

    public static void GenerateNewGameData()
    {
        data = new GameData();

        data.pilots = new List<Pilot>();
        data.stations = new List<Station>();

        // generate station locations //

        var points = new List<Vector2>();
        int iterations = 0;
        while (points.Count < 25)
        {
            points = PoissonDiscSampler.GeneratePoints(2000f, new Vector2(100000, 100000), 2);
            iterations++;
            if (iterations > 100) break;
        }

        // generate stations with owner pilots //

        for (int i = 0; i < 25; i++)
        {
            int ownerID = i;
            Station station = new Station(NameGenerator.Generate(), ownerID);
            station.position = new float[] { points[i].x, points[i].y, Random.value * 360f };
            data.stations.Add(station);

            Pilot owner = null;
            if (i == 0)
            {
                owner = new Pilot(true, true, "Player One", 0, 0);
            }
            else
            {
                owner = new Pilot(true, false, NameGenerator.Generate() + " " + NameGenerator.Generate(), Random.Range(0, GameManager.current.team.Length), i); 
            }

            owner.skinID = Random.Range(0, GameManager.current.team[owner.teamID].teamSkins.Length);
            data.pilots.Add(owner);

            int stationID = data.stations.IndexOf(station);
            owner.AssignBaseStation(stationID);

            Vector2 spawn = owner.baseStation.positionV2;
            for (int s = 0; s < 25; s++)
            {
                if (Physics2D.OverlapCircleAll(spawn, 1f).Length == 0)
                {
                    break;
                }
                else
                {
                    spawn = owner.baseStation.positionV2 + Random.insideUnitCircle * 5f;
                }
            }
            owner.SavePosition(spawn, Random.value * 360f);
        }
    }

    public static void SpawnStations()
    {
        station_ent = new Dictionary<Station, StationEntity>();
        for (int i = 0; i < data.stations.Count; i++)
        {
            var go = PrefabManager.InstatiatePrefab(PrefabManager.station[0]);
            var pos = new Vector2(data.stations[i].position[0], data.stations[i].position[1]);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, 0, data.stations[i].position[2]);

            StationEntity se = go.GetComponent<StationEntity>();
            se.station = data.stations[i];

            station_ent.Add(data.stations[i], se);

            GlobalEvents.StationSpawned(se);
        }
    }

    public static void SpawnShips()
    {
        Ship spawnedShip;
        foreach (var p in data.pilots)
        {
            p.AssignBaseStation(p.baseStationID);
            spawnedShip = ShipPool.current.SpawnShip(p);
        }
    }
}

[System.Serializable]
public class GameData
{
    public List<Pilot> pilots;
    public List<Station> stations;


    public Pilot GetPilot(int ID)
    {
        return pilots[ID];
    }

    public Station GetStation(int ID)
    {
        return stations[ID];
    }

    public int GetPilotID(Pilot pilot)
    {
        return pilots.IndexOf(pilot);
    }

    public int GetStationID(Station station)
    {
        return stations.IndexOf(station);
    }
    public Vector2 GetWorldCenter()
    {
        Vector2 value = Vector2.zero;
        for (int i = 0; i < stations.Count; i++)
        {
            value += stations[i].positionV2;
        }
        return value / stations.Count;
    }

    public float GetWorldRadius()
    {
        float value = 0;

        for (int i = 0; i < stations.Count; i++)
        {
            float radius = (stations[i].positionV2 - GameManager.current.WorldCenter).magnitude;
            if (value < radius)
            {
                value = radius;
            }
        }

        return value;
    }
}