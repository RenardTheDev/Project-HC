using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPool : MonoBehaviour
{
    public static ShipPool current;
    public GameObject _shipPrefab;

    public List<ShipPoolEntry> activeList;
    public List<ShipPoolEntry> inactiveList;

    private void Awake()
    {
        current = this;

        activeList = new List<ShipPoolEntry>();
        inactiveList = new List<ShipPoolEntry>();
    }

    private void Start()
    {
        PreloadShips(20);
    }

    private void Update()
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i].ship.health <= 0)
            {
                if (activeList[i].deadTimer >= 10)
                {
                    HideShip(activeList[i]);
                }
                else
                {
                    activeList[i].deadTimer += Time.deltaTime;
                }
            }
        }
    }

    public Ship SpawnPlayeShip(Vector2 pos, float rotation)
    {
        Ship s = SpawnShip(pos, rotation);

        s.MarkAsPlayer(true);
        s.Name = "Player";
        s.ChangeSkin(GameManager.current.team[0].GetRandomSkin(), false);

        GlobalEvents.ShipSpawned(s);

        CameraController.current.AssignTarget(s.transform);
        PlayerUI.current.AssignTarget(s);
        return s;
    }

    public Ship SpawnEnemyShip(Vector2 pos, float rotation)
    {
        Ship s = SpawnShip(pos, rotation);

        s.MarkAsPlayer(false);
        s.Name = $"Pilot#{Random.value * 1000:0000}";
        s.ChangeSkin(GameManager.current.team[1].GetRandomSkin(), false);

        GlobalEvents.ShipSpawned(s);
        return s;
    }

    public Ship SpawnTeamShip(Vector2 pos, float rotation, int team)
    {
        Ship s = SpawnShip(pos, rotation);

        s.MarkAsPlayer(false);
        s.teamID = team;
        s.Name = $"{NameGenerator.Generate()} {NameGenerator.Generate()}";
        s.ChangeSkin(GameManager.current.team[team].GetRandomSkin(), false);

        GlobalEvents.ShipSpawned(s);
        return s;
    }

    public void HideShips()
    {
        foreach (var item in activeList)
        {
            if (item.ship.isAlive) item.ship.Hide();
        }
    }

    public void ObliterateShips()
    {
        foreach (var item in activeList)
        {
            if (item.ship.isAlive) item.ship.Obliterate();
        }
    }

    Ship SpawnShip(Vector2 pos, float rotation)
    {
        ShipPoolEntry s = GetShip();
        if (s == null)
        {
            s = CreateNewShip();
        }
        else
        {
            inactiveList.Remove(s);
            activeList.Add(s);
        }

        s.Spawn(pos, rotation);

        return s.ship;
    }

    ShipPoolEntry GetShip()
    {
        if (inactiveList.Count > 0)
        {
            return inactiveList[0];
        }
        else
        {
            return null;
        }
    }

    ShipPoolEntry CreateNewShip()
    {
        var go = Instantiate(_shipPrefab);
        ShipPoolEntry p = new ShipPoolEntry(go);

        GlobalEvents.ShipPoolCreatedNewInstance(p.ship);

        activeList.Add(p);

        return p;
    }

    void PreloadShips(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(_shipPrefab);
            ShipPoolEntry p = new ShipPoolEntry(go);

            GlobalEvents.ShipPoolCreatedNewInstance(p.ship);

            inactiveList.Add(p);

            p.Despawn();
        }
    }

    void HideShip(ShipPoolEntry ship)
    {
        ship.Despawn();

        inactiveList.Add(ship);
        activeList.Remove(ship);
    }
}

[System.Serializable]
public class ShipPoolEntry
{
    public GameObject go;
    public Ship ship;
    public float deadTimer;

    public ShipPoolEntry(GameObject gameObject)
    {
        go = gameObject;
        ship = go.GetComponent<Ship>();
    }

    public void Spawn(Vector2 pos, float rotation)
    {
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);

        ship.health = ship.maxHealth;

        go.SetActive(true);
    }

    public void Despawn()
    {
        go.SetActive(false);
        ship.health = 0;
        deadTimer = 0;
    }
}