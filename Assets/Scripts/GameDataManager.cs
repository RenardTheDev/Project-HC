using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UIElements;

public static class GameDataManager
{
    public static GameData data;

    public static Dictionary<int, Pilot> pilots;
    public static Dictionary<int, Station> stations;

    public static bool CheckForSavedGame()
    {
        bool fileExists = File.Exists(Application.persistentDataPath + "/game.dat");
        return fileExists;
    }

    public static void LoadGameData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.OpenRead(Application.persistentDataPath + "/game.dat");
        data = (GameData)bf.Deserialize(file);

        file.Close();

        UnlinkEvents();

        stations = new Dictionary<int, Station>();
        foreach (var p in data.stationsInfo)
        {
            stations.Add(p.Key, new Station(p.Value));
        }

        pilots = new Dictionary<int, Pilot>();
        foreach (var p in data.pilotsInfo)
        {
            pilots.Add(p.Key, new Pilot(p.Value));
        }

        Debug.Log("Saved data loaded");
    }

    public static void UnlinkEvents()
    {
        if (stations != null)
        {
            foreach (var p in stations)
            {
                p.Value.UnlinkEvents();
            }
        }

        if (pilots != null)
        {
            foreach (var p in pilots)
            {
                p.Value.UnlinkEvents();
            }
        }
    }

    public static void SaveGame()
    {
        for (int i = 0; i < data.pilotsInfo.Count; i++)
        {
            if (data.pilotsInfo[i].currStationID == data.currentActiveStation) 
                data.pilotsInfo[i].saved_pos = pilots[data.pilotsInfo[i].ID].GetPositionAsFloats();
        }

        data.lastSession = System.DateTime.Now;

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

        data.gameStart = System.DateTime.Now;

        data.pilotsInfo = new Dictionary<int, PilotData>();
        data.stationsInfo = new Dictionary<int, StationData>();

        UnlinkEvents();

        pilots = new Dictionary<int, Pilot>();
        stations = new Dictionary<int, Station>();

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
            Station station = new Station(new StationData()
            {
                Name = NameGenerator.Generate(),
                ownerID = ownerID,
                position = new float[] { points[i].x, points[i].y, Random.value * 360f },
                fleet = new List<int>(),
            });
            AddStation(station);

            Pilot owner = null;
            if (i == 0)
            {
                owner = new Pilot(true, true, "Player One", 0, 0);
                data.currentActiveStation = station.data.ID;
            }
            else
            {
                owner = new Pilot(true, false, NameGenerator.Generate() + " " + NameGenerator.Generate(), Random.Range(0, GameManager.inst.team.Length), i); 
            }

            owner.data.skinID = Random.Range(0, GameManager.inst.team[owner.data.teamID].teamSkins.Length);
            AddPilot(owner);

            owner.AssignBaseStation(i);
            owner.data.currStationID = i;

            Vector2 spawn = Vector2.zero;
            for (int s = 0; s < 25; s++)
            {
                if (Physics2D.OverlapCircleAll(spawn, 1f).Length == 0)
                {
                    break;
                }
                else
                {
                    spawn = owner.baseStation.data.positionV2 + Random.insideUnitCircle * 5f;
                }
            }
            owner.data.SavePosition(spawn, Random.value * 360f);
        }
    }

    public static void SpawnStations()
    {
        for (int i = 0; i < stations.Count; i++)
        {
            var go = PrefabManager.InstatiatePrefab(PrefabManager.inst.station[0]);
            var pos = new Vector2(stations[i].data.position[0], stations[i].data.position[1]);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, 0, stations[i].data.position[2]);

            StationEntity se = go.GetComponent<StationEntity>();
            se.station = stations[i];
            stations[i].ent = se;

            GlobalEvents.StationSpawned(se);
        }
    }

    public static void SpawnActiveStation()
    {
        var station = stations[data.currentActiveStation];
        var go = PrefabManager.InstatiatePrefab(PrefabManager.inst.station[0]);

        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.Euler(0, 0, station.data.position[2]);

        StationEntity se = go.GetComponent<StationEntity>();
        se.station = station;
        station.ent = se;

        GlobalEvents.StationSpawned(se);

        MapSystem.inst.SpawnStationMarkers();
    }

    public static void SpawnShips()
    {
        string str = "";
        string piltos = "";
        /*for (int i = 0; i < stations[data.currentActiveStation].data.pilots.Count; i++)
        {
            var pilot = pilots[stations[data.currentActiveStation].data.pilots[i]];

            str += $"\npilotID = {pilot.data.ID}, name = {pilot.data.Name}";

            ShipPool.current.SpawnShip(pilot);
        }*/


        Station station = GetCurrentStation();
        foreach (var p in pilots)
        {
            piltos += $"\npilotID = {p.Value.data.ID}, name = {p.Value.data.Name}, " +
                $"currStationID = {p.Value.data.currStationID}, baseStationID = {p.Value.data.baseStationID}";

            if (station.data.fleet.Exists(x => x == p.Key) || p.Value.data.currStationID == station.data.ID)
            {
                ShipPool.current.SpawnShip(p.Value);
                str += $"\npilotID = {p.Value.data.ID}, name = {p.Value.data.Name}";
            }
        }

        Debug.Log(piltos);
        Debug.Log(str);

        /*Ship spawnedShip;
        foreach (var p in pilots)
        {
            if (p.Value.data.currStationID == data.currentActiveStation)
            {
                p.Value.AssignBaseStation(p.Value.data.baseStationID);
                spawnedShip = ShipPool.current.SpawnShip(p.Value);
            }
        }*/
    }

    // PILOTS MANAGMENT //
    public static void AddPilot(Pilot pilot)
    {
        pilot.data.ID = data.lastPilotID;
        pilots.Add(pilot.data.ID, pilot);
        data.pilotsInfo.Add(pilot.data.ID, pilot.data);
        data.lastPilotID++;
    }
    public static void RemovePilot(Pilot pilot)
    {
        pilots.Remove(pilot.data.ID);
        data.pilotsInfo.Remove(pilot.data.ID);
    }

    public static Pilot GetPilot(int ID)
    {
        return pilots[ID];
    }

    // STATIONS MANAGMENT //
    public static void AddStation(Station station)
    {
        station.data.ID = data.lastStationID;
        stations.Add(station.data.ID, station);
        data.stationsInfo.Add(station.data.ID, station.data);
        data.lastStationID++;
    }
    public static void RemoveStation(Station station)
    {
        stations.Remove(station.data.ID);
    }

    public static Station GetStation(int ID)
    {
        return stations[ID];
    }

    public static Station GetCurrentStation()
    {
        return stations[data.currentActiveStation];
    }
}

[System.Serializable]
public class GameData
{
    public int currentActiveStation = -1; //station when the game take place

    public bool isPlayerDocked;

    public Dictionary<int, PilotData> pilotsInfo;
    public Dictionary<int, StationData> stationsInfo;

    public int lastPilotID;
    public int lastStationID;

    // STUFF //

    public System.DateTime gameStart;
    public System.DateTime lastSession;

    public Vector2 GetWorldCenter()
    {
        Vector2 value = Vector2.zero;
        for (int i = 0; i < stationsInfo.Count; i++)
        {
            value += stationsInfo[i].positionV2;
        }
        return value / stationsInfo.Count;
    }

    public float GetWorldRadius()
    {
        float value = 0;

        for (int i = 0; i < stationsInfo.Count; i++)
        {
            float radius = (stationsInfo[i].positionV2 - GameManager.inst.WorldCenter).magnitude;
            if (value < radius)
            {
                value = radius;
            }
        }

        return value;
    }

}