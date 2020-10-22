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

    public static event Action<Damage> onShipGetHit;
    public static void ShipGetHit(Damage dmg)
    {
        onShipGetHit?.Invoke(dmg);
    }

    public static event Action<Damage> onShipKilled;
    public static void ShipKilled(Damage dmg)
    {
        onShipKilled?.Invoke(dmg);
    }

    public static event Action<Ship> onShipSpawned;
    public static void ShipSpawned(Ship ship)
    {
        onShipSpawned?.Invoke(ship);
    }

    public static event Action<Ship> onShipPoolCreatedNewInstance;
    public static void ShipPoolCreatedNewInstance(Ship ship)
    {
        onShipPoolCreatedNewInstance?.Invoke(ship);
    }

    public static event Action<AsteroidEntity> onAsteroidDestroyed;
    public static void AsteroidDestroyed(AsteroidEntity aster)
    {
        onAsteroidDestroyed?.Invoke(aster);
    }

    public static event Action<Weapon> onPlayerChangedWeapon;
    public static void PlayerWeaponChanged(Weapon weap)
    {
        onPlayerChangedWeapon?.Invoke(weap);
    }

    public static event Action<SkillBase> onPlayerChangedSkill;
    public static void PlayerSkillChanged(SkillBase skill)
    {
        onPlayerChangedSkill?.Invoke(skill);
    }

    public static event Action OnGameOver;
    public static void GameOver()
    {
        OnGameOver?.Invoke();
    }

    public static event Action<GameState> OnGameStateChanged;
    public static void GameStateChanged(GameState newstate)
    {
        OnGameStateChanged?.Invoke(newstate);
    }

    public static event Action<object, object, float, Vector3> OnCollisionEntered;
    public static void CollisionEntered(object objA, object objB, float hitPower, Vector3 point)
    {
        OnCollisionEntered?.Invoke(objA, objB, hitPower, point);
    }

    //---settings---
    public static event Action<bool> OnControlsChanged;
    public static void ControlsChanged(bool state)
    {
        OnControlsChanged?.Invoke(state);
    }
}
