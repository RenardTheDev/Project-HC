using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipTag : MonoBehaviour
{
    public Ship t_ship;
    public Transform t_trans;

    public bool isActive;

    RectTransform rt_tag;

    public GameObject go_target;
    public GameObject go_health;
    public GameObject go_shield;

    public Image ui_health;
    public Image ui_shield;
    public Image ui_Hunter;

    private void Awake()
    {
        rt_tag = GetComponent<RectTransform>();
    }

    public void Assign(Ship ship)
    {
        t_ship = ship;
        t_trans = ship.transform;

        point = Camera.main.WorldToViewportPoint(t_trans.position);
        isOnScreen = point.x > 0f && point.x < 1f && point.y > 0f && point.y < 1f;

        go_target.SetActive(!isOnScreen);
        go_health.SetActive(isOnScreen);
        go_shield.SetActive(isOnScreen && t_ship.maxShield > 0);

        ui_Hunter.enabled = t_ship.isHunter;

        UpdatePosition();
    }

    public void Disable()
    {
        t_ship = null;
        t_trans = null;

        gameObject.SetActive(false);
    }

    Vector3 pos;
    Vector3 point;

    bool _onScreen;
    bool isOnScreen;

    private void FixedUpdate()
    {
        if (t_ship == null || Ship.PLAYER == null) return;

        point = Camera.main.WorldToViewportPoint(t_trans.position);
        isOnScreen = point.x > 0f && point.x < 1f && point.y > 0f && point.y < 1f;

        if (isOnScreen != _onScreen)
        {
            if (isOnScreen) { OnShipApearedOnScreen(); }
            else { OnShipDispearedFromScreen(); }

            go_target.SetActive(!isOnScreen);
            go_health.SetActive(isOnScreen);
            go_shield.SetActive(isOnScreen && t_ship.maxShield > 0);

            _onScreen = isOnScreen;
        }

        ui_Hunter.enabled = t_ship.isHunter;

        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (_onScreen)
        {
            ui_health.fillAmount = t_ship.health / t_ship.maxHealth;
            ui_shield.fillAmount = t_ship.shield / t_ship.maxShield;

            rt_tag.anchorMin = Vector2.zero;
            rt_tag.anchorMax = Vector2.zero;

            pos.x = 720 * point.x * PlayerShipUI.ScrRatio;
            pos.y = 720 * point.y;
            rt_tag.anchoredPosition = pos;
        }
        else
        {
            Vector2 dir = (t_trans.position - Ship.PLAYER.transform.position).normalized;

            rt_tag.anchorMin = Vector2.one * 0.5f;
            rt_tag.anchorMax = Vector2.one * 0.5f;

            pos.x = 350 * dir.x;
            pos.y = 350 * dir.y;
            rt_tag.anchoredPosition = pos;
        }
    }

    void OnShipApearedOnScreen()
    {

    }

    void OnShipDispearedFromScreen()
    {

    }
}
