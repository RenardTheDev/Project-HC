using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipMotor : MonoBehaviour
{
    Ship ship;
    Rigidbody2D rig;


    [Header("Params")]
    public float accel = 10f;
    public float maxSpeed = 20f;

    //--- controls ---
    public Vector2 move;
    public Vector2 aim;

    // booster //
    public float boostPower = 10;
    public float boost = 0;
    public float boostConsumeRate = 0.1f;
    public float boostRestoreRate = 0.1f;
    public bool boosting=false;
    public bool boostReady=false;

    // CACHE //
    Transform trans;
    public Vector2 velocity;
    public float veloMagnitude;

    private void Awake()
    {
        trans = transform;
        ship = GetComponent<Ship>();
        rig = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (ship.isPlayer)
        {
            if (boosting)
            {
                if (PlayerInputs._move.sqrMagnitude > 0.01f)
                {
                    move = PlayerInputs._move.normalized;
                }
                else
                {
                    if (move.sqrMagnitude < 0.01f)
                    {
                        move = trans.up;
                    }
                }
            }
            else
            {
                move = PlayerInputs._move;
            }
            aim = PlayerInputs._aim;
            if (PlayerInputs._accel == bindState.down || PlayerInputs._accel == bindState.hold) UseBoost();
        }
    }

    public float targetAngle;
    public float angle;
    public Vector2 rotationTarget;
    Vector2 rotationTargetNormal;
    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;
    //public float lastAimed;
    public bool rotateToMovement;

    float lastUpdate;

    private void FixedUpdate()
    {
        if (!ship.isAlive) return;

        if (!ship.isVisible)
        {
            if (lastUpdate > Time.time - 1)
            {
                return;
            }
        }
        lastUpdate = Time.time;

        if (aim.sqrMagnitude > 0.01f && !boosting)
        {
            rotateToMovement = false;
        }
        else
        {
            rotateToMovement = true;
        }

        rotationTarget = rotateToMovement ? move : aim;
        if (rotationTarget.sqrMagnitude > 0.01f)
        {
            if (rotateToMovement)
            {
                rotationTargetNormal = rotationTarget.normalized;
                targetAngle = Mathf.Atan2(-rotationTargetNormal.x, rotationTargetNormal.y) * Mathf.Rad2Deg;
                angle = Mathf.SmoothDampAngle(trans.eulerAngles.z, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            }
            else
            {
                rotationTargetNormal = rotationTarget.normalized;
                angle = Mathf.Atan2(-rotationTargetNormal.x, rotationTargetNormal.y) * Mathf.Rad2Deg;
            }

            rig.MoveRotation(angle);
        }

        if (move.sqrMagnitude > 0.01f)
        {
            if (boosting)
            {
                rig.drag = rig.velocity.sqrMagnitude > Mathf.Pow(maxSpeed * boostPower, 2) ? 10 : 1;
                rig.AddForce(move * accel * boostPower, ForceMode2D.Impulse);
            }
            else
            {
                rig.drag = rig.velocity.sqrMagnitude > Mathf.Pow(maxSpeed, 2) ? 10 : 1;
                rig.AddForce(move * accel, ForceMode2D.Impulse);
            }
        }

        velocity = rig.velocity;
        veloMagnitude = velocity.magnitude;

        if (boosting)
        {
            boost = Mathf.MoveTowards(boost, 0, Time.fixedDeltaTime * boostConsumeRate);
            if (boost <= 0)
            {
                boosting = false;
            }
        }
        else
        {
            if (!boostReady)
            {
                boost = Mathf.MoveTowards(boost, 1, Time.fixedDeltaTime * boostRestoreRate);
                if (boost >= 1) { boostReady = true; boost = 1; ship.OnBoostReady(); }
            }
        }
    }

    public void UseBoost()
    {
        if (boostReady)
        {
            boosting = true;
            boostReady = false;
            ship.OnBoostUsed();
        }
    }
}