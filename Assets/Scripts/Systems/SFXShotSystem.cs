using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXShotSystem : MonoBehaviour
{
    public static SFXShotSystem current;

    public GameObject prefab;

    public List<sfxShotEntry> activeList = new List<sfxShotEntry>();
    public List<sfxShotEntry> inactiveList = new List<sfxShotEntry>();

    public AudioClip explosionClip;
    public AudioClip explosiveProjClip;

    public AudioClip collision_ship2aster;
    public AudioClip collision_ship2ship;
    public AudioClip collision_aster2aster;

    private void Awake()
    {
        current = this;
        GlobalEvents.OnCollisionEntered += OnCollisionEntered;
    }

    private void OnCollisionEntered(object obj1, object obj2, float hitPower, Vector3 point)
    {
        float volume = Mathf.Lerp(0f, 0.2f, hitPower / 5f);
        if (volume <= 0.05f) return;

        if (obj1 is Ship)
        {
            if (obj2 is Ship)
            {
                SpawnSFX(point, volume, collision_ship2ship, null, 0.9f, 0, 10, 25);
            }
            if (obj2 is AsteroidEntity)
            {
                SpawnSFX(point, volume, collision_ship2aster, null, 0.9f, 0, 10, 25);
            }
        }
        if (obj1 is AsteroidEntity)
        {
            if (obj2 is AsteroidEntity)
            {
                SpawnSFX(point, volume, collision_aster2aster, null, 0.9f, 0, 10, 25);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            var shot = activeList[i];
            if (Time.time > shot.destroyAfter)
            {
                DespawnSFXShot(shot);
            }
        }
    }

    public void SpawnExplosionSFX(Vector3 pos, float volume)
    {
        SpawnSFX(pos, volume, explosionClip, null, 0.9f, 0.1f, 10, 100);
    }

    public void SpawnExplosiveProjSFX(Vector3 pos, float volume)
    {
        SpawnSFX(pos, volume, explosiveProjClip, null, 0.9f, 0.1f, 10, 100);
    }

    public void SpawnSFX(Vector3 pos, float volume, AudioClip sfx, Transform parent, float pitch = 1f, float pitchRandomize = 0f, float minDistance = 10f, float maxDistance = 100f)
    {
        var shot = GetSFXShot();

        if (shot == null)
        {
            shot = CreateSFXShot();
        }
        else
        {
            inactiveList.Remove(shot);
            activeList.Add(shot);
        }

        shot.Spawn(pos, volume, sfx, pitch, pitchRandomize, minDistance, maxDistance);
        if (parent != null) shot.trans.parent = parent;
    }

    sfxShotEntry GetSFXShot()
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

    sfxShotEntry CreateSFXShot()
    {
        var go = Instantiate(prefab, transform);
        var shot = new sfxShotEntry(go);
        activeList.Add(shot);

        return shot;
    }

    void DespawnSFXShot(sfxShotEntry shot)
    {
        activeList.Remove(shot);
        inactiveList.Add(shot);

        shot.trans.parent = transform;
        shot.Despawn();
    }
}

[System.Serializable]
public class sfxShotEntry
{
    public GameObject go;
    public Transform trans;
    public AudioSource source;

    public float destroyAfter;

    public sfxShotEntry(GameObject gameObject)
    {
        go = gameObject;
        trans = go.transform;
        source = go.GetComponent<AudioSource>();
    }

    public void Spawn(Vector3 pos, float volume, AudioClip sfx, float pitch, float pitchRandomize, float minDistance, float maxDistance)
    {
        go.SetActive(true);
        trans.position = pos;

        source.pitch = pitch + Random.Range(-pitchRandomize, pitchRandomize) * 0.5f;

        source.minDistance = minDistance;
        source.maxDistance = maxDistance;

        source.PlayOneShot(sfx, volume);

        destroyAfter = Time.time + sfx.length + 0.1f;
    }

    public void Despawn()
    {
        source.Stop();
        go.SetActive(false);

        destroyAfter = 0;
    }
}