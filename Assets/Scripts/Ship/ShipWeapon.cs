using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipWeapon : MonoBehaviour
{
    Ship ship;

    public Weapon gun;
    public SkillBase skill;

    public bool trigger;
    public bool skill_trig;

    public float skill_cd;
    public float skill_cd_mult = 1f;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    private void Update()
    {
        if (ship.isPlayer)
        {
            trigger = PlayerInputs._fire == bindState.hold || PlayerInputs._fire == bindState.down || PlayerInputs._shoot;
            skill_trig = PlayerInputs._skill == bindState.hold || PlayerInputs._skill == bindState.down;

            if (skill != null)
            {
                if (skill_cd < 1f)
                {
                    skill_cd = Mathf.MoveTowards(skill_cd, 1f, Time.deltaTime * skill.recoverSpeed * skill_cd_mult);
                }
            }
        }

        if (gun != null)
        {
            gun.Update();
        }
    }

    public void AssignSkill(SkillBase newSkill)
    {
        skill = Instantiate(newSkill);
        skill.OnAssigned(ship);

        if (ship.isPlayer) GlobalEvents.PlayerSkillChanged(skill);
    }

    public void AssignWeapon(Weapon newWeapon)
    {
        gun = Instantiate(newWeapon);
        gun.OnAssigned(ship);

        ship.equippedWeapID = newWeapon.WEAPON_ID;

        gun.SetLevel(ship.weapUpgrades[newWeapon.WEAPON_ID]);

        if (ship.isPlayer) GlobalEvents.PlayerWeaponChanged(gun);
    }

    public void UpdateWeaponUpgrade()
    {
        gun.SetLevel(ship.weapUpgrades[gun.WEAPON_ID]);
    }

    private void FixedUpdate()
    {
        if (gun != null)
        {
            if (trigger) gun.Trigger();
        }

        if (skill != null)
        {
            if (skill_trig && skill_cd >= 1f)
            {
                skill.Activate();
                skill_cd = 0;
            }
        }
    }
}