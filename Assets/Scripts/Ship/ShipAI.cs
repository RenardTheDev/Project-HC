using System.Collections;
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
    Collider2D[] overlapSearch;

    public AIState state = AIState.idle;

    Vector3 destination;
    public bool isAvoiding;

    float avoidTime = 0;

    public bool isPlayer { get => ship.isPlayer; }
    public float health { get => ship.health; }

    // cache //
    Transform trans;
    Transform enemyTrans;
    Vector3 myPos;
    Vector3 enemyPos;

    private void Awake()
    {
        trans = transform;

        ship = GetComponent<Ship>();
        motor = GetComponent<ShipMotor>();
        weap = GetComponent<ShipWeapon>();

        enemies = new List<Ship>();

        ship.onShipGetHit += OnShipGetHit;
        ship.onShipKilled += OnShipKilled;

        overlapSearch = new Collider2D[30];
    }

    private void OnShipKilled(Damage dmg)
    {
        currEnemy = null;
        enemies.Clear();

        state = AIState.idle;
    }

    float lastEnemySwitch;
    float enemySwitchTime = 5f;
    private void OnShipGetHit(Damage dmg)
    {
        if (dmg.attacker == null) return;

        if (dmg.attacker.teamID != ship.teamID)
        {
            if(dmg.attacker.health > 0)
            {
                if(Time.time > lastEnemySwitch + enemySwitchTime)
                {
                    currEnemy = dmg.attacker;
                    lastEnemySwitch = Time.time;
                }
            }
        }
    }

    float logicUpdateInterval = 0.25f;
    private void Start()
    {
        InvokeRepeating("LogicUpdate", Random.value * logicUpdateInterval, logicUpdateInterval);
    }

    private void OnDrawGizmosSelected()
    {
        if (currEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(trans.position, GetEnemyDirection());
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(trans.position, destination);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(trans.position, GetDestDirection());

        if (currEnemy != null) Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(destination, 1f);
    }

    private void LogicUpdate()
    {
        if (!ship.isAlive) return;
        //ObstacleCheck();

        myPos = trans.position;
        if (enemyTrans != null) enemyPos = enemyTrans.position;

        switch (state)
        {
            case AIState.idle: 
                {
                    if (ship.pilot.important)
                    {
                        destination = ship.pilot.baseStation.positionV2 + Random.insideUnitCircle * 1000f;
                    }
                    else
                    {
                        destination = ship.pilot.baseStation.positionV2 + Random.insideUnitCircle * 1000f;
                    }
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

                            avoidTime += Time.deltaTime;
                            if (GetDistanceToEnemy() > 10 || avoidTime > 5)
                            {
                                isAvoiding = false;
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
                                if (Random.value > 0.975f)
                                {
                                    weap.trigger = true;
                                }
                            }

                            if (GetDistanceToEnemy() < 3)
                            {
                                Vector3 rnd = Random.insideUnitCircle * 10f;
                                destination = trans.position - GetEnemyDirection() * 30 + rnd;
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
        var HIT = Physics2D.CircleCastAll(trans.position, 0.5f, trans.up, 5f);
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
                avoidVector = hit.normal * Mathf.Max(0, 5 - hit.distance);
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
                break;
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

    Collider2D searchOverlap;
    void CheckForEnemies()
    {
        currEnemy = null;
        enemyTrans = null;

        enemies.Clear();

        int overlaps = Physics2D.OverlapCircleNonAlloc(myPos, 100, overlapSearch, searchMask);
        if (overlaps == 0)
        {
            return;
        }
        else
        {
            for (int i = 0; i < overlaps; i++)
            {
                searchOverlap = overlapSearch[i];
                if (searchOverlap.TryGetComponent(out Ship sh))
                {
                    if (sh.teamID != ship.teamID && sh.isAlive)
                    { enemies.Add(sh); }
                }
            }
        }

        if (enemies.Count > 0)
        {
            currEnemy = enemies[Random.Range(0, enemies.Count)];
            enemyTrans = currEnemy.transform;
        }
    }

    float GetDistanceToDestination()
    {
        return (myPos - destination).magnitude;
    }

    float GetDistanceToEnemy()
    {
        if (currEnemy != null && currEnemy.health > 0)
        {
            return (myPos - enemyPos).magnitude;
        }
        else
        {
            return GetDistanceToDestination();
        }
    }

    Vector3 GetDestDirection()
    {
        Vector3 dir = destination - myPos;
        return (dir.normalized + avoidVector).normalized;
    }

    Vector3 GetEnemyDirection()
    {
        if (currEnemy != null && currEnemy.health > 0)
        {
            Vector3 dir = enemyPos - myPos;
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