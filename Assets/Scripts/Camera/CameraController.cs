﻿using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController current;
    CinemachineVirtualCamera cm_cam;

    public Transform target;
    Rigidbody2D target_rb;

    Vector3 aheadOffset;
    public float aheadSize = 1;
    public float aheadDump = 0.1f;

    public CinemachineImpulseSource explosionImpulse;

    private void Awake()
    {
        current = this;
        if (target) cm_cam.m_Follow = target;
    }

    private void Start()
    {
        GlobalEvents.onShipKilled += OnShipKilled;
    }

    private void OnShipKilled(Damage data)
    {
        if (data.victim != null && data.victim.isPlayer)
        {
            ClearTarget();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        aheadOffset = Vector3.Lerp(aheadOffset, Vector3.ClampMagnitude(target_rb.velocity, aheadSize), Time.deltaTime * aheadDump);
        transform.position = target.position + Vector3.back * 10f + aheadOffset;
    }

    public void AssignTarget(Transform newTarget)
    {
        target = newTarget;
        target_rb = target.GetComponent<Rigidbody2D>();
    }

    public void ClearTarget()
    {
        target = null;
        target_rb = null;
    }

    public void ExplosionImpulse(Vector3 pos, float mult = 1f)
    {
        explosionImpulse.transform.position = pos;
        Vector3 veclocity = pos - transform.position;

        explosionImpulse.GenerateImpulseAt(pos, veclocity.normalized * mult);
    }
}
