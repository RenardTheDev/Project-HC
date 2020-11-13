using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationEntity : MonoBehaviour
{
    public Station station;
    public Transform spawnPoint;

    public Vector2 spawn { get => spawnPoint.position; }
    public string Name { get => station.Name; }
    public int teamID { get => station.teamID; }

    private void Awake()
    {
        GlobalEvents.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.MainMenu)
        {
            GlobalEvents.OnGameStateChanged -= OnGameStateChanged;
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        
    }
}
