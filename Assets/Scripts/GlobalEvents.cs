using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEvents : MonoBehaviour
{
    public static GlobalEvents current;

    private void Awake()
    {
        if (current == null)
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static event Action<Damage> OnShipGetHit;
    public static void ShipGetHit(Damage dmg) => OnShipGetHit?.Invoke(dmg);


    public static event Action<Damage> OnShipKilled;
    public static void ShipKilled(Damage dmg) => OnShipKilled?.Invoke(dmg);


    public static event Action<Ship> OnShipSpawned;
    public static void ShipSpawned(Ship ship) => OnShipSpawned?.Invoke(ship);


    public static event Action<Ship> OnShipPoolCreatedNewInstance;
    public static void ShipPoolCreatedNewInstance(Ship ship) => OnShipPoolCreatedNewInstance?.Invoke(ship);


    public static event Action<AsteroidEntity> OnAsteroidDestroyed;
    public static void AsteroidDestroyed(AsteroidEntity aster) => OnAsteroidDestroyed?.Invoke(aster);


    public static event Action<Weapon> OnPlayerChangedWeapon;
    public static void PlayerWeaponChanged(Weapon weap) => OnPlayerChangedWeapon?.Invoke(weap);


    public static event Action<SkillBase> OnPlayerChangedSkill;
    public static void PlayerSkillChanged(SkillBase skill) => OnPlayerChangedSkill?.Invoke(skill);


    public static event Action OnGameOver;
    public static void GameOver() => OnGameOver?.Invoke();


    public static event Action<GameState, GameState> OnGameStateChanged;
    public static void GameStateChanged(GameState oldstate, GameState newstate) => OnGameStateChanged?.Invoke(oldstate, newstate);


    public static event Action<object, object, float, Vector3> OnCollisionEntered;
    public static void CollisionEntered(object objA, object objB, float hitPower, Vector3 point) => OnCollisionEntered?.Invoke(objA, objB, hitPower, point);


    // World Creation //
    public static event Action<StationEntity> OnStationSpawned;
    public static void StationSpawned(StationEntity station) => OnStationSpawned?.Invoke(station);

    //---settings---
    public static event Action<bool> OnControlsChanged;
    public static void ControlsChanged(bool state) => OnControlsChanged?.Invoke(state);
}