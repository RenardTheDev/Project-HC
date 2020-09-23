﻿using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FixedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Canvas parent;

    public bindState state = bindState.none;

    private void Awake()
    {
        parent = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (!parent.gameObject.activeSelf && state != bindState.none) StateChange(bindState.none);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StateChange(bindState.down);

        stateChangeCor = StartCoroutine(ChangeStateAfterFrame(bindState.hold));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StateChange(bindState.up);

        if (stateChangeCor != null) StopCoroutine(stateChangeCor);
        stateChangeCor = StartCoroutine(ChangeStateAfterFrame(bindState.none));
    }

    Coroutine stateChangeCor;
    IEnumerator ChangeStateAfterFrame(bindState state)
    {
        yield return new WaitForEndOfFrame();
        StateChange(state);
    }

    private void OnDisable()
    {
        StateChange(bindState.none);
    }

    void StateChange(bindState newState)
    {
        state = newState;
    }
}