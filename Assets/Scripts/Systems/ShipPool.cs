using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPool : MonoBehaviour
{
    public static ShipPool current;
    public GameObject _shipPrefab;

    public List<ShipPoolEntry> activeList;
    public List<ShipPoolEntry> inactiveList;

    public Sprite[] smallShipSprite_player;
    public Sprite[] smallShipSprite_enemy;

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

    public Ship SpawnShip(Vector2 pos, float rotation, bool isPlayer, string name = "Random pilot", int team = 0)
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

        s.Spawn(pos, rotation, isPlayer, name, team);

        s.spr.sprite = isPlayer ?
            smallShipSprite_player[Random.Range(0, smallShipSprite_player.Length)] :
            smallShipSprite_enemy[Random.Range(0, smallShipSprite_enemy.Length)];

        s.spr.flipY = !isPlayer;

        GlobalEvents.ShipSpawned(s.ship);

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

public class ShipPoolEntry
{
    public GameObject go;
    public Ship ship;
    public SpriteRenderer spr;
    public float deadTimer;

    public ShipPoolEntry(GameObject gameObject)
    {
        go = gameObject;
        ship = go.GetComponent<Ship>();
        spr = go.GetComponent<SpriteRenderer>();
    }

    public void Spawn(Vector2 pos, float rotation, bool isPlayer, string name, int team)
    {
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);

        ship.health = ship.maxHealth;
        ship.team = team;
        ship.Name = name;

        ship.MarkAsPlayer(isPlayer);

        go.SetActive(true);
    }

    public void Despawn()
    {
        go.SetActive(false);
        deadTimer = 0;
    }
}