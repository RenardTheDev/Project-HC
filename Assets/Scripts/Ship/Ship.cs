﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class Ship : MonoBehaviour
{
    Transform trans;

    public int pilotID;
    public Pilot pilot;

    public static Ship PLAYER;

    SpriteRenderer spr;

    public ShipWeapon weap;
    public ShipMotor motor;
    public ShipAI ai;

    public bool isVisible;

    public int equippedWeapID = 0;

    public string Name;
    public bool isPlayer;
    public int teamID;

    public bool isInvulnerable;
    public float health = 16;
    public float maxHealth = 16;

    public float shield = 0;
    public float maxShield = 0;
    public float shieldRegen = 0.1f;

    public float damageMult = 1.0f;
    public float damageResist = 0.0f;

    public bool isAlive { get => health > 0; }

    public CircleCollider2D coll;
    public CircleCollider2D hitbox;
    public AudioSource sfx_source;

    Rigidbody2D rig;

    float lastDamageTaken = -999;

    private void Awake()
    {
        trans = transform;

        spr = GetComponent<SpriteRenderer>();
        rig = GetComponent<Rigidbody2D>();
        motor = GetComponent<ShipMotor>();
        weap = GetComponent<ShipWeapon>();
        ai = GetComponent<ShipAI>();

        sfx_source = GetComponentInChildren<AudioSource>();

        ToggleVisibility(false);
    }

    public void AssignPilot(int pilotID)
    {
        this.pilotID = pilotID;
        pilot = GameDataManager.GetPilot(pilotID);

        Name = pilot.data.Name;
        isPlayer = pilot.data.isPlayer;
        teamID = pilot.data.teamID;
    }

    public void AssignPilot(Pilot pilot)
    {
        pilotID = pilot.data.ID;
        this.pilot = pilot;

        Name = pilot.data.Name;
        isPlayer = pilot.data.isPlayer;
        teamID = pilot.data.teamID;
    }

    public void ChangeSkin(Sprite newSkin, bool flipY)
    {
        spr.sprite = newSkin;
        spr.flipY = flipY;
    }

    public void OnSpawned()
    {
        if (pilot.data.isPlayer)
        {
            PLAYER = this;
            ai.ToggleAI(false);
            ToggleVisibility(true);
        }
        onShipSpawned?.Invoke();
    }

    public void ToggleVisibility(bool enable)
    {
        if (isVisible == enable) return;

        spr.enabled = enable;
        sfx_source.enabled = enable;
        isVisible = enable;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // collision shake effect //

        if (isPlayer)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                var c = collision.contacts[i];
                CameraController.current.CollisionImpulse(c.point, c.normal * c.normalImpulse * 0.02f);
            }
        }

        // collision damage //

        Vector2 hit = collision.relativeVelocity;
        float dot = Mathf.Pow(Vector2.Dot(hit.normalized, collision.contacts[0].normal), 2f);
        hit = hit * dot;
        float hitPower = Mathf.Clamp(collision.rigidbody.mass / rig.mass, 1f, float.MaxValue) * hit.magnitude * Time.fixedDeltaTime;

        if (collision.collider.TryGetComponent(out Ship ship))
        {
            if (hitPower > 1f)
            {
                ApplyDamage(ship, hitPower);
            }
            GlobalEvents.CollisionEntered(this, ship, hitPower, collision.contacts[0].point);
        }

        if (collision.collider.TryGetComponent(out AsteroidEntity aster))
        {
            if (hitPower > 1f)
            {
                ApplyDamage(this, hitPower);
            }
            GlobalEvents.CollisionEntered(this, aster, hitPower, collision.contacts[0].point);
        }
    }

    private void FixedUpdate()
    {
        if (health <= 0 || health > 20f && isVisible) return;

        if (health <= 20f)
        {
            ParticleEffects.current.SpawnFire(trans.position);
        }
    }

    private void Update()
    {
        if (isPlayer && Time.time > lastDamageTaken + 2f)
        {
            health = Mathf.MoveTowards(health, maxHealth, Time.deltaTime * 5);
        }
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

        dmg.hp = dmg.hp - dmg.hp * damageResist;

        if (shield > dmg.hp)
        {
            dmg.reaction = DamageReactionType.shield;
            if(!isInvulnerable) shield -= dmg.hp;
            ShipGetHit(dmg);
        }
        else
        {
            dmg.reaction = shield > 0 ? DamageReactionType.both : DamageReactionType.health;

            float diffDMG = dmg.hp - shield;
            if (!isInvulnerable) shield = 0;

            if (health > diffDMG)
            {
                if (!isInvulnerable) health -= diffDMG;
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

        ParticleEffects.current.SpawnExplosion(trans.position);
        SFXShotSystem.current.SpawnExplosionSFX(trans.position, 0.1f);

        CameraController.current.ExplosionImpulse(trans.position, isPlayer ? 10f : 1f);
    }

    public void Obliterate()
    {
        health = 0;
        gameObject.SetActive(false);

        ParticleEffects.current.SpawnExplosion(trans.position);
        SFXShotSystem.current.SpawnExplosionSFX(trans.position, 0.1f);

        var dmg = new Damage()
        {
            victim = this,
            hp = health,
            deathReason = DeathReason.obliterated
        };
        GlobalEvents.ShipKilled(dmg);
    }

    public void Hide()
    {
        health = 0;
        gameObject.SetActive(false);

        var dmg = new Damage()
        {
            victim = this,
            hp = health,
            deathReason = DeathReason.hidden
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
    public event Action onShipSpawned;

    public event Action onBoostReady;
    public void OnBoostReady() { onBoostReady?.Invoke(); }
    public event Action onBoostUsed;
    public void OnBoostUsed() { onBoostUsed?.Invoke(); }
}

public struct Damage
{
    public Ship attacker;
    public Ship victim;
    public float hp;

    public DamageReactionType reaction;

    public DeathReason deathReason;
}

public enum DamageReactionType
{
    none,
    health,
    shield,
    both
}

public enum DeathReason
{
    killed,
    obliterated,
    hidden
}