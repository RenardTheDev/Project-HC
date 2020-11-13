using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSystem : MonoBehaviour
{
    public static AsteroidSystem current;
    
    public GameObject[] prefab_big;
    public GameObject[] prefab_med;
    public GameObject[] prefab_small;
    public GameObject[] prefab_tiny;

    public List<Asteroid> activeList = new List<Asteroid>();
    public Dictionary<AsteroidSize, List<Asteroid>> inactiveDict = new Dictionary<AsteroidSize, List<Asteroid>>();

    public AudioClip sfx_destructed;

    private void Awake()
    {
        current = this;

        inactiveDict.Add(AsteroidSize.big, new List<Asteroid>());
        inactiveDict.Add(AsteroidSize.medium, new List<Asteroid>());
        inactiveDict.Add(AsteroidSize.small, new List<Asteroid>());
        inactiveDict.Add(AsteroidSize.tiny, new List<Asteroid>());
    }

    private void Start()
    {
        GlobalEvents.OnAsteroidDestroyed += OnAsteroidDestroyed;
    }

    private void OnAsteroidDestroyed(AsteroidEntity aster)
    {
        SFXShotSystem.current.SpawnSFX(aster.transform.position, 0.1f, sfx_destructed, null, 0.9f, 0.1f);
    }

    private void Update()
    {
        if (Ship.PLAYER == null) return;

        for (int i = 0; i < activeList.Count; i++)
        {
            var aster = activeList[i];
            if ((Ship.PLAYER.transform.position - aster.pos).magnitude > 50 || aster.health <=0)
            {
                activeList.Remove(aster);
                inactiveDict[aster.size].Add(aster);

                aster.Despawn();
            }
        }
    }

    public void SpawnAsteroid(Vector3 pos, AsteroidSize size, bool shootToPlayer)
    {
        if (activeList.Count >= 15) return;

        var aster = GetAsteroid(size);

        if (aster == null)
        {
            aster = CreateAsteroid(size);
        }
        else
        {
            inactiveDict[aster.size].Remove(aster);
            activeList.Add(aster);
        }

        aster.Spawn(pos, shootToPlayer);
    }

    public void HideAsteroids()
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            activeList[i].Despawn();
        }
    }

    Asteroid GetAsteroid(AsteroidSize size)
    {
        if (inactiveDict[size].Count > 0)
        {
            return inactiveDict[size][0];
        }
        else
        {
            return null;
        }
    }

    Asteroid CreateAsteroid(AsteroidSize size)
    {
        GameObject go = null;

        switch (size)
        {
            case AsteroidSize.big:
                go = Instantiate(prefab_big[Random.Range(0, prefab_big.Length)], transform);
                break;
            case AsteroidSize.medium:
                go = Instantiate(prefab_med[Random.Range(0, prefab_med.Length)], transform);
                break;
            case AsteroidSize.small:
                go = Instantiate(prefab_small[Random.Range(0, prefab_small.Length)], transform);
                break;
            case AsteroidSize.tiny:
                go = Instantiate(prefab_tiny[Random.Range(0, prefab_tiny.Length)], transform);
                break;
        }

        var shot = new Asteroid(go, size);
        activeList.Add(shot);

        return shot;
    }
}

public class Asteroid
{
    public GameObject go;
    public Transform trans;
    public Rigidbody2D rig;
    public AsteroidEntity entity;
    public AsteroidSize size;

    public Vector3 pos { get => trans.position; }
    public float health { get => entity.health; }

    public Asteroid(GameObject go, AsteroidSize size)
    {
        this.go = go;
        this.size = size;
        trans = go.transform;
        rig = go.GetComponent<Rigidbody2D>();
        entity = go.GetComponent<AsteroidEntity>();
    }

    public void Spawn(Vector3 pos, bool shootToPlayer)
    {
        go.SetActive(true);
        trans.position = pos;
        trans.rotation = Quaternion.Euler(0, 0, 360 * Random.value);

        if (Ship.PLAYER != null && shootToPlayer)
        {
            if (Random.value > 0.75f)
            {
                Vector3 dir = (Ship.PLAYER.transform.position - pos).normalized;
                rig.velocity = dir * 15 * Random.value;
            }
        }

        rig.AddTorque(Random.value * 200 - 100);

        entity.health = entity.maxHealth;
    }

    public void Despawn()
    {
        go.SetActive(false);
    }
}

/*public enum asterSize
{
    tiny,
    small,
    med,
    big
}*/