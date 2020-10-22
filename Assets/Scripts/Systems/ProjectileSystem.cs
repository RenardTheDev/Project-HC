using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSystem : MonoBehaviour
{
    public static ProjectileSystem current;
    public GameObject _bulletPrefab;
    //public GameObject _MisslePrefab;

    public List<Projectile> activeList;
    public List<Projectile> inactiveList;

    public LayerMask hitmask;

    private void Awake()
    {
        current = this;

        activeList = new List<Projectile>();
        inactiveList = new List<Projectile>();
    }

    private void Update()
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            var p = activeList[i];

            if (p.sprRender.enabled)
            {
                if (p.travel < p.weapon.projectile.distance)
                {
                    UpdateProjectile(p);
                }
                else
                {
                    if (p.trail.emitting)
                    {
                        DelayedProjectileDespawn(p);
                    }
                    else
                    {
                        ProjectileDespawn(p);
                    }
                }
            }
        }
    }

    void UpdateProjectile(Projectile p)
    {
        RaycastHit2D finalHit = new RaycastHit2D();
        RaycastHit2D[] HIT = Physics2D.CircleCastAll(p.pos, p.radius, p.go.transform.up, p.speed * Time.deltaTime, hitmask);
        bool stopped = false;

        foreach (RaycastHit2D hit in HIT)
        {
            Ship ship = hit.collider.GetComponentInParent<Ship>();
            if (ship != null)
            {
                if (ship != p.owner && ship.teamID != p.owner.teamID)
                {
                    ship.ApplyDamage(p.owner, p.damage);

                    stopped = true;
                    finalHit = hit;
                    break;
                }
            }
            else
            {
                if (hit.collider.TryGetComponent(out AsteroidEntity aster))
                {
                    aster.ApplyDamage(p.owner, p.damage);
                }
                stopped = true;
                finalHit = hit;
                break;
            }
        }

        if (stopped)
        {
            switch (p.weapon.projectile.impact)
            {
                case ImpactType.explosive:
                    ParticleEffects.current.SpawnSmallExplosion(finalHit.point);
                    SFXShotSystem.current.SpawnExplosiveProjSFX(finalHit.point, 0.05f);
                    break;
                case ImpactType.plasma:
                    ParticleEffects.current.SoawnImpactPlasma(finalHit.point, p.weapon.projectile.color);
                    break;
            }

            if (p.trail.emitting)
            {
                DelayedProjectileDespawn(p);
            }
            else
            {
                ProjectileDespawn(p);
            }
        }
        else
        {
            var travel = p.direction * p.speed * Time.deltaTime;

            if (p.weapon is Weapon_Homing && p.target != null && p.target.isAlive)
            {
                var dir = (p.target.transform.position - p.pos).normalized;
                var targetAngle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
                if(Random.value > 0.75f)
                {
                    targetAngle += Random.Range(-p.weapon.spread, p.weapon.spread);
                }

                float speed = Mathf.Lerp(0.25f, 1.0f, Mathf.Clamp01(p.time * p.time));

                float diffAngle = Mathf.DeltaAngle(targetAngle, p.rot);
                float diff = Mathf.Lerp(0.5f, 1.0f, 1f - Mathf.Clamp01(Mathf.Abs(diffAngle / 90)));

                var angle = Mathf.SmoothDampAngle(p.go.transform.eulerAngles.z, targetAngle, ref p.turnSmoothVelocity, ((Weapon_Homing)p.weapon).turnSmoothTime);
                p.go.transform.rotation = Quaternion.Euler(0, 0, angle);

                travel = travel * Mathf.Clamp(speed * diff, 0.5f, 1.0f);
            }

            p.travel += travel.magnitude;
            p.go.transform.position = p.pos + travel;
            p.time += Time.deltaTime;
        }
    }

    public Projectile SpawnProjectile(Vector2 pos, float rot, Ship shooter, Weapon weapon)
    {
        Projectile p = GetProjectile(weapon);
        if (p == null)
        {
            p = CreateNewProjectile(weapon);
        }
        else
        {
            inactiveList.Remove(p);
            activeList.Add(p);
        }

        p.Spawn(pos, rot, shooter, weapon);

        return p;
    }

    public Projectile SpawnHomingProjectile(Vector2 pos, float rot, Ship shooter, Weapon weapon, Ship target)
    {
        Projectile p = SpawnProjectile(pos, rot, shooter, weapon);
        p.target = target;

        return p;
    }

    Projectile GetProjectile(Weapon weapon)
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

    Projectile CreateNewProjectile(Weapon weapon)
    {
        var go = Instantiate(_bulletPrefab, transform);
        Projectile p = new Projectile(go);

        activeList.Add(p);

        return p;
    }

    void ProjectileDespawn(Projectile p)
    {
        p.owner = null;
        p.weapon = null;
        p.travel = 0;

        inactiveList.Add(p);
        activeList.Remove(p);

        if (p.despawn_coroutine != null) StopCoroutine(p.despawn_coroutine);

        p.Despawn();
    }

    void DelayedProjectileDespawn(Projectile p)
    {
        p.despawn_coroutine = StartCoroutine(_delayedDespawn(p, p.trail.time + 0.1f));
    }

    IEnumerator _delayedDespawn(Projectile p, float time)
    {
        p.sprRender.enabled = false;
        yield return new WaitForSeconds(time);

        ProjectileDespawn(p);
    }
}

[System.Serializable]
public class Projectile
{
    public GameObject go;
    public SpriteRenderer sprRender;
    public TrailRenderer trail;

    public Coroutine despawn_coroutine;

    public Ship owner;
    public Weapon weapon;

    public float travel;
    public float time;

    //--- homing feature ---
    public Ship target;
    public float turnSmoothVelocity;

    //--- links ---
    public Vector3 pos { get => go.transform.position; }
    public float rot { get => go.transform.rotation.eulerAngles.z; }
    public Vector3 direction { get => go.transform.up; }
    public float speed { get => weapon.projectile.speed; }
    public float radius { get => weapon.projectile.radius; }
    public float damage { get => weapon.lvld_damage; }

    public Projectile(GameObject go)
    {
        this.go = go;
        sprRender = go.GetComponent<SpriteRenderer>();
        trail = go.GetComponent<TrailRenderer>();
    }

    public void Spawn(Vector3 position, float rotation, Ship shooter, Weapon weap)
    {
        owner = shooter;
        weapon = weap;

        sprRender.sprite = weapon.bulletSprite;

        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);

        if(weap is Weapon_Homing)
        {
            var wh = (Weapon_Homing)weap;
            trail.emitting = true;
            trail.widthCurve = wh.trailWidthCurve;
            trail.colorGradient = wh.trailGradient;
        }
        else
        {
            trail.emitting = false;
        }

        sprRender.color = weap.projectile.color;
        sprRender.enabled = true;

        go.SetActive(true);

        time = 0;
    }

    public void Despawn()
    {
        trail.emitting = false;
        go.SetActive(false);
    }
}