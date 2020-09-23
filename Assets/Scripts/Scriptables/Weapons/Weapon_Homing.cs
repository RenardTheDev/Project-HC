using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HomingWeapon", menuName = "Scriptables/Weapon/Homing feature")]
public class Weapon_Homing : Weapon
{
    [Header("Homing weapon values")]
    public float turnSmoothTime = 0.1f;
    public AnimationCurve trailWidthCurve;
    public Gradient trailGradient;
}
