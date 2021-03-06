﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    Ship ship;
    ShipMotor motor;
    ShipWeapon weap;

    List<Ship> enemies;
    Ship currEnemy;
    public LayerMask searchMask;

    public AIState state = AIState.idle;

    Vector3 destination;
    public bool isAvoiding;
    public bool isAttacker;

    float avoidTime = 0;

    public bool isPlayer { get => ship.isPlayer; }
    public float health { get => ship.health; }


    private void Awake()
    {
        ship = GetComponent<Ship>();
        motor = GetComponent<ShipMotor>();
        weap = GetComponent<ShipWeapon>();

        enemies = new List<Ship>();

        ship.onShipGetHit += OnShipGetHit;
        ship.onShipKilled += OnShipKilled;
    }

    private void OnShipKilled(Damage dmg)
    {
        //GMSurvival.current.currAttackingEnemies--;
    }

    float lastEnemySwitch;
    float enemySwitchTime = 5f;
    private void OnShipGetHit(Damage dmg)
    {
        if (dmg.attacker.team != ship.team)
        {
            if(dmg.attacker.health > 0)
            {
                if(Time.time > lastEnemySwitch + enemySwitchTime)
                {
                    currEnemy = dmg.attacker;
                }
            }
        }
    }

    private void Start()
    {

    }

    private void OnDrawGizmosSelected()
    {
        if (currEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, GetEnemyDirection());
        }
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, GetDestDirection());
    }

    private void Update()
    {
        ObstacleCheck();

        switch (state)
        {
            case AIState.idle:
                {
                    destination = Random.insideUnitCircle * 100f;
                    ChangeState(AIState.roam);
                    break;
                }
            case AIState.roam:
                {
                    if (GetDistanceToDestination() > 5)
                    {
                        CheckForEnemies();
                        if (currEnemy != null) ChangeState(AIState.attack);
                        motor.move = GetDestDirection().normalized;
                    }
                    else
                    {
                        ChangeState(AIState.idle);
                    }
                    break;
                }
            case AIState.attack:
                {
                    if (currEnemy != null && currEnemy.health > 0)
                    {
                        if (isAvoiding)
                        {
                            if (weap.trigger) weap.trigger = false;

                            if (Random.value > 0.97f)
                            {
                                Vector3 rnd = Random.insideUnitCircle * 10f;
                                destination = currEnemy.transform.position - GetEnemyDirection() * 15 + rnd;
                            }

                            if (isAttacker)
                            {
                                avoidTime += Time.deltaTime;
                                if (GetDistanceToEnemy() > 20 || avoidTime > 5)
                                {
                                    isAvoiding = false;
                                    avoidTime = 0;
                                }
                            }
                            else
                            {
                                avoidTime = 0;
                            }
                        }
                        else
                        {
                            destination = currEnemy.transform.position;

                            if (weap.trigger)
                            {
                                if (Random.value > 0.5f) weap.trigger = false;
                            }
                            else
                            {
                                if (Random.value > (0.99f - Mathf.Clamp(GameData.wave * 0.005f, 0.01f, 0.5f)))
                                {
                                    weap.trigger = true;
                                }
                            }

                            if (GetDistanceToEnemy() < 3)
                            {
                                Vector3 rnd = Random.insideUnitCircle * 10f;
                                destination = transform.position - GetEnemyDirection() * 30 + rnd;
                                isAvoiding = true;
                            }
                        }

                        motor.move = GetDestDirection().normalized;
                    }
                    else
                    {
                        ChangeState(AIState.idle);

                        weap.trigger = false;
                    }
                    break;
                }
        }
    }

    //public LayerMask obstacleMask;
    Vector3 avoidVector;
    void ObstacleCheck()
    {
        var HIT = Physics2D.CircleCastAll(transform.position, 0.5f, transform.up, 3f);
        avoidVector = Vector3.zero;
        for (int i = 0; i < HIT.Length; i++)
        {
            var hit = HIT[i];

            if (hit.transform.gameObject == gameObject || (currEnemy != null && hit.transform.gameObject == currEnemy.gameObject))
            {
                continue;
            }
            else
            {
                avoidVector = hit.normal;
                break;
            }
        }
    }

    void ChangeState(AIState newState)
    {
        switch (newState)
        {
            case AIState.idle:
                break;
            case AIState.roam:
                break;
            case AIState.attack:
                {
                    isAvoiding = !isAttacker;
                    break;
                }
            case AIState.stayAround:
                break;
        }

        state = newState;
    }

    public void ToggleAI(bool enable)
    {
        if (enable)
        {
            enabled = true;
        }
        else
        {
            enabled = false;
        }
    }

    void CheckForEnemies()
    {
        enemies.Clear();

        for (int i = 0; i < ShipPool.current.activeList.Count; i++)
        {
            var sh = ShipPool.current.activeList[i].ship;
            if (sh.team != ship.team && sh.isAlive)
            { enemies.Add(sh); }
        }

        if (enemies.Count > 0)
        {
            currEnemy = enemies[Random.Range(0, enemies.Count)];
        }
        else
        {
            currEnemy = null;
        }
    }

    float GetDistanceToDestination()
    {
        return (transform.position - destination).magnitude;
    }

    float GetDistanceToEnemy()
    {
        if (currEnemy != null && currEnemy.health > 0)
        {
            return (transform.position - currEnemy.transform.position).magnitude;
        }
        else
        {
            return GetDistanceToDestination();
        }
    }

    Vector3 GetDestDirection()
    {
        Vector3 dir = destination - transform.position;
        //Debug.DrawRay(transform.position, dir, Color.green);

        return (dir.normalized + avoidVector).normalized;
    }

    Vector3 GetEnemyDirection()
    {
        if (currEnemy != null && currEnemy.health > 0)
        {
            Vector3 dir = currEnemy.transform.position - transform.position;
            //Debug.DrawRay(transform.position, dir, Color.red);
            return (dir.normalized + avoidVector).normalized;
        }
        else
        {
            return GetDestDirection();
        }
    }
}

public enum AIState
{
    idle,
    roam,
    attack,
    stayAround
}