using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour : MonoBehaviour
{
    Ship ship;
    ShipMotor motor;
    ShipWeapon weap;

    // BEHAVIOUR //

    public List<Node> behaviourTree;

    // CACHE //

    Camera cam;

    Transform camTrans;
    Vector3 camPos;

    Transform trans;
    Vector3 myPos;
    Vector3 destination;
    Vector2 baseStationPos;

    Ship currEnemy;
    Transform enemyTrans;
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
        ship.onShipSpawned += OnShipSpawned;

        overlapSearch = new Collider2D[30];
    }

    private void Start()
    {// PATROL SEQUENCE //
        var patrolSequence = new List<Node>();
        patrolSequence.Add(new ActionNode(() =>   //before patrol check for enemies
        {
            if (CheckForEnemies())
            {
                return NodeStates.FAILURE;
            }
            else
            {
                return NodeStates.SUCCESS;
            }
        }));

        patrolSequence.Add(new ActionNode(() =>   // get random patrol point
        {
            destination = baseStationPos + Random.insideUnitCircle * 1000f;
            return NodeStates.SUCCESS;
        }));

        patrolSequence.Add(new ActionNode(() =>   // roam to point
        {
            if (GetDistanceToDestination() > 5)
            {
                motor.move = GetDestDirection();
                return NodeStates.RUNNING;
            }
            else
            {
                return NodeStates.SUCCESS;
            }
        }));

        // ATTACK SEQUENCE //

        var attackSequnce = new List<Node>();
        attackSequnce.Add(new ActionNode(() => // check for health
        {
            if (ship.health < ship.maxHealth * 0.2f)
            {
                return NodeStates.FAILURE;
            }
            else
            {
                return NodeStates.SUCCESS;
            }
        }));
        attackSequnce.Add(new ActionNode(() => // attack enemy until it killed
        {
            if (currEnemy.isAlive)
            {
                if (ship.isAlive) return NodeStates.FAILURE;

                destination = enemyPos;

                if (weap.trigger)
                {
                    if (Random.value > 0.5f) weap.trigger = false;
                }
                else
                {
                    if (Random.value > 0.9f)
                    {
                        weap.trigger = true;
                    }
                }

                return NodeStates.RUNNING;
            }
            else
            {
                return NodeStates.SUCCESS;
            }
        }));

        // FLEE SEQUENCE //

        var fleeSequence = new List<Node>();
        fleeSequence.Add(new ActionNode(() =>   // avoid enemy until get healed or enemy is dead or far away
        {
            if (!currEnemy.isAlive || ship.health > ship.maxHealth * 0.4f || GetDistanceToEnemy() > 25)
            {
                return NodeStates.SUCCESS;
            }
            if (!ship.isAlive)
            {
                return NodeStates.FAILURE;
            }

            if (Random.value > 0.5f)
            {
                Vector3 rnd = Random.insideUnitCircle * 10f;
                destination = currEnemy.transform.position - GetEnemyDirection() * 15 + rnd;
            }
            return NodeStates.RUNNING;
        }));

        var treeBase = new List<Node>();
        treeBase.Add(
            new Selector(new List<Node>()
            {
                new Sequence(patrolSequence),
                new Sequence(attackSequnce)
            }));

        behaviourTree = new List<Node>();
        behaviourTree.Add(new Selector(treeBase));

        StartCoroutine(Logic());
    }

    private void OnShipSpawned()
    {
        baseStationPos = ship.pilot.baseStation.data.positionV2;
    }

    IEnumerator Logic()
    {
        while (true)
        {
            behaviourTree[0].Evaluate();
        }
    }

    private void OnShipKilled(Damage dmg)
    {
        currEnemy = null;
        enemies.Clear();
    }

    float lastEnemySwitch;
    float enemySwitchTime = 5f;
    private void OnShipGetHit(Damage dmg)
    {
        if (dmg.attacker == null) return;

        if (dmg.attacker.teamID != ship.teamID)
        {
            if (dmg.attacker.health > 0)
            {
                if (Time.time > lastEnemySwitch + enemySwitchTime)
                {
                    currEnemy = dmg.attacker;
                    lastEnemySwitch = Time.time;
                }
            }
        }
    }

    private void Update()
    {
        myPos = trans.position;
        if (enemyTrans != null) enemyPos = enemyTrans.position;
    }

    public LayerMask searchMask;
    Collider2D[] overlapSearch;
    Collider2D searchOverlap;
    List<Ship> enemies = new List<Ship>();
    bool CheckForEnemies()
    {
        currEnemy = null;
        enemyTrans = null;

        enemies.Clear();

        int overlaps = Physics2D.OverlapCircleNonAlloc(myPos, 100, overlapSearch, searchMask);
        if (overlaps > 0)
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
            return true;
        }

        return false;
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
        return dir.normalized;
    }

    Vector3 GetEnemyDirection()
    {
        if (currEnemy != null && currEnemy.health > 0)
        {
            Vector3 dir = enemyPos - myPos;
            return dir.normalized;
        }
        else
        {
            return GetDestDirection();
        }
    }
}