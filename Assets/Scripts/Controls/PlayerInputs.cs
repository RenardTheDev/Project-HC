using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    public static PlayerInputs current;

    [Header("Touch controls")]
    public FixedJoystick t_move;
    public FixedJoystick t_aimShoot;
    public FixedButton t_fire;
    public FixedButton t_skill;

    public float shootThreshold = 0.25f;

    [Header("Values")]
    //--- controls values ---
    public static Vector2 _move;
    public static Vector2 _aim;
    public static bool _shoot;
    public static bindState _fire;
    public static bindState _skill;

    public static float screenRatio;
    public static float screenHeight;
    public static float screenWidth;

    private void Awake()
    {
        current = this;
    }

    private void Update()
    {
        _move = t_move.InputVector;
        _aim = t_aimShoot.InputVector;
        _shoot = t_aimShoot.InputVector.magnitude > shootThreshold;
        _fire = t_fire.state;
        _skill = t_skill.state;

        screenHeight = Screen.height;
        screenWidth = Screen.width;
        screenRatio = screenWidth / screenHeight;
    }
}

public enum bindState
{
    none,
    down,
    hold,
    up
}