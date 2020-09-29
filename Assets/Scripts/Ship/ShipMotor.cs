using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipMotor : MonoBehaviour
{
    Ship ship;
    Rigidbody2D rig;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        rig = GetComponent<Rigidbody2D>();
    }

    [Header("Params")]
    public float accel = 10f;
    public float maxSpeed = 50f;

    //--- controls ---
    public Vector2 move;
    public Vector2 aim;

    private void Update()
    {
        if (ship.isPlayer)
        {
            move = PlayerInputs._move;
            aim = PlayerInputs._aim;
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
    private void FixedUpdate()
    {
        if (ship.isPlayer && SettingsUI.current.settings.useNewControls)
        {
            rotationTarget = rotateToMovement ? move : aim;
            if (rotationTarget.magnitude > 0.1f)
            {
                if (rotateToMovement)
                {
                    rotationTargetNormal = rotationTarget.normalized;
                    targetAngle = Mathf.Atan2(-rotationTargetNormal.x, rotationTargetNormal.y) * Mathf.Rad2Deg;
                    angle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                }
                else
                {
                    rotationTargetNormal = rotationTarget.normalized;
                    angle = Mathf.Atan2(-rotationTargetNormal.x, rotationTargetNormal.y) * Mathf.Rad2Deg;
                }
            }

            if (aim.magnitude > 0.1f)
            {
                rotateToMovement = false;
            }
            else
            {
                rotateToMovement = true;
            }

            transform.rotation = Quaternion.Euler(0, 0, angle);
            if (move.magnitude > 0.1f)
            {
                if (rig.velocity.magnitude < maxSpeed)
                {
                    rig.AddForce(move * accel, ForceMode2D.Impulse);
                }
                else
                {
                    rig.AddForce((move + rig.velocity.normalized) * accel, ForceMode2D.Impulse);
                }
            }
        }
        else
        {
            if (move.magnitude > 0.1f)
            {
                rotationTargetNormal = move.normalized;
                targetAngle = Mathf.Atan2(-rotationTargetNormal.x, rotationTargetNormal.y) * Mathf.Rad2Deg;
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            }

            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (rig.velocity.magnitude < maxSpeed)
            {
                rig.AddRelativeForce(Vector2.up * move.magnitude * accel, ForceMode2D.Impulse);
            }
            else
            {
                rig.AddRelativeForce(Vector2.up * (move + rig.velocity.normalized).magnitude * accel, ForceMode2D.Impulse);
            }
        }
    }
}