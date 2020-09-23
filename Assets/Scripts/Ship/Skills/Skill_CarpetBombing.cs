using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarpetBombing", menuName = "Scriptables/Skills/CarpetBombing")]
public class Skill_CarpetBombing : SkillBase
{
    public Weapon weapon;
    public int rocketCount = 10;
    public float lockOnRadius = 50f;
    public float fireRate = 0.05f;

    Transform trans;

    public override void OnAssigned(Ship ship)
    {
        base.OnAssigned(ship);

        trans = ship.transform;
    }

    List<Ship> enemies;
    Vector3 pivot;
    int shotCount;
    public override void Activate()
    {
        base.Activate();

        shotCount = 0;

        pivot = owner.transform.position;
        enemies = new List<Ship>();

        for (int i = 0; i < ShipPool.current.activeList.Count; i++)
        {
            var sh = ShipPool.current.activeList[i].ship;
            var dist = (sh.transform.position - owner.transform.position).magnitude;
            if (sh != owner && sh.team != owner.team && sh.isAlive && dist < lockOnRadius)
            {
                enemies.Add(sh);
            }
        }

        CoroutineHost.Instance.StartCoroutine(Blast());
    }

    IEnumerator Blast()
    {
        var tempo = recoverSpeed;
        recoverSpeed = 0;

        if (enemies.Count > 0)
        {
            // sort by distance
            enemies.Sort(
                (x, y) => (x.transform.position - pivot).magnitude.CompareTo((y.transform.position - pivot).magnitude)
                );

            if (enemies.Count >= rocketCount)
            {
                for (int i = 0; i < rocketCount; i++)
                {
                    yield return new WaitForSeconds(fireRate);

                    //Debug.Log($"SpawnProjectile({i}) - enemies.Count = {enemies.Count}");

                    SpawnProjectile(enemies[i]);
                }
            }
            else
            {
                int enemyID = 0;
                for (int i = 0; i < rocketCount; i++)
                {
                    yield return new WaitForSeconds(fireRate);

                    //Debug.Log($"SpawnProjectile({enemyID}) - enemies.Count = {enemies.Count}");

                    SpawnProjectile(enemies[enemyID]);
                    enemyID = enemyID + 1 >= enemies.Count ? 0 : enemyID + 1;
                }
            }
        }
        else
        {
            for (int i = 0; i < rocketCount; i++)
            {
                yield return new WaitForSeconds(fireRate);
                SpawnProjectile(null);
            }
        }

        recoverSpeed = tempo;
    }

    void SpawnProjectile(Ship target)
    {
        float dir = owner.transform.eulerAngles.z + Random.Range(-weapon.spread, weapon.spread);

        Vector3 origin = trans.TransformPoint((shotCount % 2 == 0 ? Vector3.right : Vector3.left) * 0.5f);

        ProjectileSystem.current.SpawnHomingProjectile(origin, dir, owner, weapon, target);
        owner.sfx_source.PlayOneShot(weapon.sfx, weapon.volume);

        shotCount++;
    }
}
