using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tesla", menuName = "Scriptables/Skills/TeslaSkill")]
public class Skill_Tesla : SkillBase
{
    public float activeRadius = 20f;
    public int jumpCount = 3;
    public float damage;

    public AudioClip SFX;
    public float volume = 0.1f;

    List<Ship> hitList;
    public override void Activate()
    {
        base.Activate();

        //SFXShotSystem.current.SpawnSFX(owner.transform.position, volume, SFX, 1f, 0.1f);
        owner.sfx_source.PlayOneShot(SFX, volume);

        hitList = new List<Ship>();
        Vector3 start = owner.transform.position;
        for (int i = 0; i < jumpCount; i++)
        {
            var enemy = GetNextEnemy(start);
            if (enemy!=null)
            {
                StrikeManager.current.SpawnStrike(start, enemy.transform.position);
                enemy.ApplyDamage(owner, damage);
                start = enemy.transform.position;
            }
            else
            {
                break;
            }
        }
    }

    List<Ship> enemies = new List<Ship>();
    Ship GetNextEnemy(Vector3 pivot)
    {
        enemies.Clear();

        Ship sh;

        for (int i = 0; i < ShipPool.current.activeList.Count; i++)
        {
            sh = ShipPool.current.activeList[i].ship;
            var dist = (sh.transform.position - pivot).magnitude;
            if (sh != owner && sh.team != owner.team && sh.isAlive && !hitList.Contains(sh) && dist < activeRadius)
            {
                enemies.Add(sh);
            }
        }


        if (enemies.Count > 0)
        {
            enemies.Sort(
                (x, y) => (x.transform.position - pivot).magnitude.CompareTo((y.transform.position - pivot).magnitude)
                );

            hitList.Add(enemies[0]);
            return enemies[0];
        }
        else
        {
            return null;
        }
    }
}
