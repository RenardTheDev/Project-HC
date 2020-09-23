using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public static Ship PLAYER;

    public ShipWeapon weap;
    public ShipMotor motor;

    public Dictionary<int, int> shipUpgrades = new Dictionary<int, int>();
    public Dictionary<int, int> weapUpgrades = new Dictionary<int, int>();

    public int equippedWeapID = 0;

    public bool isPlayer;
    public bool isHunter;

    public float health = 16;
    public float maxHealth = 16;

    public float shield = 0;
    public float maxShield = 0;
    public float shieldRegen = 0.1f;

    public float damageMult = 1.0f;
    public float damageResist = 0.0f;

    public bool isAlive { get => health > 0; }

    public string Name = "Pilot#0000";

    public int team = -1;

    public CircleCollider2D coll;
    public CircleCollider2D hitbox;
    public AudioSource sfx_source;

    Rigidbody2D rig;

    float lastDamageTaken = -999;

    private void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        motor = GetComponent<ShipMotor>();
        weap = GetComponent<ShipWeapon>();
        sfx_source = GetComponentInChildren<AudioSource>();

        if (CompareTag("Player"))
        {
            MarkAsPlayer(true);
        }
    }

    public void MarkAsPlayer(bool mark)
    {
        tag = mark ? "Player" : "Untagged";
        isPlayer = mark;

        if (mark)
        {
            PLAYER = this;
            hitbox.radius = coll.radius;
        }
        else
        {
            if(team != PLAYER.team)
            {
                hitbox.radius = coll.radius * GameManager._enemyHitboxScale;
            }
            else
            {
                hitbox.radius = coll.radius;
            }
        }
    }

    public void ApplyUpgrades()
    {
        foreach (var item in shipUpgrades)
        {
            UpdateUpgrade((UpgradeType)item.Key);
        }
    }

    public void UpdateUpgrade(UpgradeType type)
    {
        ShipUpgrade upg = ShipUpgrade.dict[type];
        int currentLevel = shipUpgrades[(int)type];

        switch (type)
        {
            case UpgradeType.health:
                var lastMaxHP = maxHealth;
                maxHealth = 100 + ShipUpgrade.dict[type].increment * (currentLevel - 1);
                var ratio = health / lastMaxHP;
                health = maxHealth * ratio;
                break;
            case UpgradeType.hp_pickup:
                //--- no pickups for now ---
                break;
            case UpgradeType.shield:
                maxShield = upg.increment * (currentLevel - 1);
                break;
            case UpgradeType.sh_regen:
                shieldRegen = upg.increment * (currentLevel - 1);
                break;
            case UpgradeType.speed:
                motor.maxSpeed = 50 + 50 * upg.increment * 0.01f * (currentLevel - 1);
                motor.accel = 20 + 20 * upg.increment * 0.01f * (currentLevel - 1);
                break;
            case UpgradeType.resist:
                damageResist = upg.increment * 0.01f * (currentLevel - 1);
                break;
            case UpgradeType.damage:
                damageMult = 1f + upg.increment * 0.01f * (currentLevel - 1);
                break;
            case UpgradeType.skill_cd:
                weap.skill_cd_mult = 1f - upg.increment * 0.01f * (currentLevel - 1);
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.DrawRay(transform.position, collision.relativeVelocity, Color.magenta, 5f);

        /*float damage = collision.relativeVelocity.magnitude * collision.rigidbody.mass * 0.02f;
        if (collision.relativeVelocity.magnitude > 5f)
        {
            if (collision.collider.TryGetComponent(out Ship ship))
            {
                Rigidbody2D enemyRig = ship.GetComponent<Rigidbody2D>();
                if (rig.velocity.sqrMagnitude >= enemyRig.velocity.sqrMagnitude)
                {
                    ship.ApplyDamage(this, damage);
                }
                else
                {
                    ApplyDamage(ship, damage);
                }
            }

            if (collision.collider.TryGetComponent(out AsteroidEntity aster))
            {
                Rigidbody2D enemyRig = aster.rig;
                if (rig.velocity.sqrMagnitude >= enemyRig.velocity.sqrMagnitude)
                {
                    aster.ApplyDamage(damage);
                }
                else
                {
                    ApplyDamage(null, damage);
                }
            }
        }*/
    }

    private void FixedUpdate()
    {
        if (health <= 0 || health > 20f) return;

        if (health <= 20f)
        {
            ParticleEffects.current.SpawnFire(transform.position);
        }
    }

    private void Update()
    {
        if (maxShield > 0 && Time.time > lastDamageTaken + 2f)
        {
            shield = Mathf.MoveTowards(shield, maxShield, Time.deltaTime * (1 + shieldRegen));
        }

        if (!isPlayer) return;
    }

    public void ApplyDamage(Ship attacker, float amount)
    {
        if (health <= 0) return;

        Damage dmg = new Damage() { attacker = attacker, victim = this, hp = amount };

        if (attacker.isPlayer && !isPlayer)
        {
            dmg.hp = dmg.hp * GameManager._playerDamageMult;
        }
        else if (!attacker.isPlayer && isPlayer)
        {
            dmg.hp = dmg.hp * GameManager._enemyDamageMult;
        }

        dmg.hp = dmg.hp * dmg.attacker.damageMult - dmg.hp * damageResist;

        if (shield > dmg.hp)
        {
            dmg.reaction = DamageReactionType.shield;
            shield -= dmg.hp;
            ShipGetHit(dmg);
        }
        else
        {
            dmg.reaction = shield > 0 ? DamageReactionType.both : DamageReactionType.health;

            float diffDMG = dmg.hp - shield;
            shield = 0;

            if (health > diffDMG)
            {
                health -= diffDMG;
                ShipGetHit(dmg);
            }
            else
            {
                DestroyShip(dmg);
            }
        }

        if (dmg.hp > 0) { lastDamageTaken = Time.time; }
    }

    public void DestroyShip(Damage dmg)
    {
        dmg.hp = health;
        health = 0;
        ShipKilled(dmg);

        gameObject.SetActive(false);

        ParticleEffects.current.SpawnExplosion(transform.position);
        SFXShotSystem.current.SpawnExplosionSFX(transform.position, 0.1f);

        CameraController.current.ExplosionImpulse(transform.position, isPlayer ? 10f : 1f);
    }

    public void Obliterate()
    {
        health = 0;
        gameObject.SetActive(false);

        ParticleEffects.current.SpawnExplosion(transform.position);
        SFXShotSystem.current.SpawnExplosionSFX(transform.position, 0.1f);

        var dmg = new Damage()
        {
            victim = this,
            hp = health,
            isObliteration = true
        };
        GlobalEvents.ShipKilled(dmg);
    }

    //--- local events ---
    public event System.Action<Damage> onShipGetHit;
    public void ShipGetHit(Damage dmg)
    {
        onShipGetHit?.Invoke(dmg);
        GlobalEvents.ShipGetHit(dmg);
    }

    public event System.Action<Damage> onShipKilled;
    public void ShipKilled(Damage dmg)
    {
        onShipKilled?.Invoke(dmg);
        GlobalEvents.ShipKilled(dmg);
    }
}

public struct Damage
{
    public Ship attacker;
    public Ship victim;
    public float hp;

    public DamageReactionType reaction;

    public bool isObliteration;
}

public enum DamageReactionType
{
    none,
    health,
    shield,
    both
}