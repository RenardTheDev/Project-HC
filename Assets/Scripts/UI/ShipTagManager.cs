using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipTagManager : MonoBehaviour
{
    public static ShipTagManager current;

    public GameObject prefab;

    public List<ShipTag> freeTags = new List<ShipTag>();
    public List<ShipTag> occuTags = new List<ShipTag>();

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        GlobalEvents.onShipSpawned += OnShipSpawned;
        GlobalEvents.onShipKilled += OnShipKilled;
    }

    private void OnShipSpawned(Ship ship)
    {
        if (ship.isPlayer) return;

        var tag = getFreeTag();
        tag.gameObject.SetActive(true);
        tag.Assign(ship);
    }

    private void OnShipKilled(Damage dmg)
    {
        for (int i = 0; i < occuTags.Count; i++)
        {
            var tag = occuTags[i];
            if (tag.t_ship == dmg.victim)
            {
                tag.Disable();

                occuTags.Remove(tag);
                freeTags.Add(tag);
            }
        }
    }

    ShipTag getFreeTag()
    {
        if (freeTags.Count > 0)
        {
            var tag = freeTags[0];
            freeTags.Remove(tag);
            occuTags.Add(tag);
            return tag;
        }
        else
        {
            return createNewTag();
        }
    }

    ShipTag createNewTag()
    {
        var go = Instantiate(prefab, transform);

        ShipTag tag = go.GetComponent<ShipTag>();
        occuTags.Add(tag);

        return tag;
    }

    private void Update()
    {
        
    }
}
