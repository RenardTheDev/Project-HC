using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPool : MonoBehaviour
{
    public static ShipPool current;
    //public GameObject _shipPrefab;

    public List<ShipPoolEntry> activeList;
    public List<ShipPoolEntry> inactiveList;

    private void Awake()
    {
        current = this;

        activeList = new List<ShipPoolEntry>();
        inactiveList = new List<ShipPoolEntry>();

        InvokeRepeating("UpdateCaching", 1000f, 1000f);
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
                if (activeList[i].deadTimer >= 1)
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

    void UpdateCaching()
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i].ship.health > 0)
            {
                activeList[i].pos = activeList[i].trans.position;
            }
        }
    }

    public Ship SpawnShip(Pilot pilot)
    {
        return SpawnShip(new Vector2(pilot.data.saved_pos[0], pilot.data.saved_pos[1]), pilot.data.saved_pos[2], pilot);
    }

    public Ship SpawnShip(Vector2 pos, float rotation, Pilot pilot)
    {
        Vector2 spawn = pos;
        for (int i = 0; i < 25; i++)
        {
            if (Physics2D.OverlapCircleAll(spawn, 1f).Length == 0)
            {
                break;
            }
            else
            {
                spawn = pos + Random.insideUnitCircle * 5f;
            }
        }

        Ship s = SpawnShip(pos, rotation);
        pilot.AssignShip(s);

        if (pilot.data.skinID >= GameManager.inst.team[pilot.data.teamID].teamSkins.Length) pilot.data.skinID = 0;

        s.ChangeSkin(GameManager.inst.team[pilot.data.teamID].teamSkins[pilot.data.skinID], false);
        s.AssignPilot(pilot);

        s.weap.AssignWeapon(GameManager.inst.weapons[pilot.data.weapID]);

        if (s.pilot.data.isPlayer)
        {
            CameraController.current.AssignTarget(s.transform);
            PlayerUI.current.AssignTarget(s);

            s.GetComponent<ShipAI>().enabled = false;
        }
        else
        {
            s.GetComponent<ShipAI>().enabled = true;
        }

        GlobalEvents.ShipSpawned(s);
        s.OnSpawned();
        return s;
    }

    public void HideShips()
    {
        foreach (var item in activeList)
        {
            if (item.ship.isAlive) item.ship.Hide();
        }
    }

    public void HideShip(Pilot pilot)
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            if(activeList[i].ship.pilotID == pilot.data.ID)
            {
                activeList[i].ship.Hide();
                break;
            }
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
        var go = Instantiate(PrefabManager.inst.ship);
        ShipPoolEntry p = new ShipPoolEntry(go);

        GlobalEvents.ShipPoolCreatedNewInstance(p.ship);

        activeList.Add(p);

        return p;
    }

    void PreloadShips(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(PrefabManager.inst.ship);
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

    // cache //
    public Transform trans;
    public Vector3 pos;

    public ShipPoolEntry(GameObject gameObject)
    {
        go = gameObject;
        trans = go.transform;
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