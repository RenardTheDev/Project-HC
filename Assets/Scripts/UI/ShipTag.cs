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
    public Image ui_target;

    public Text nameTag;

    Camera cam;
    Transform camTrans;

    private void Awake()
    {
        cam = Camera.main;
        camTrans = cam.transform;
        rt_tag = GetComponent<RectTransform>();
    }

    public void Assign(Ship ship)
    {
        t_ship = ship;
        t_trans = ship.transform;

        point = cam.WorldToViewportPoint(t_trans.position);

        isOnScreen = point.x > 0f && point.x < 1f && point.y > 0f && point.y < 1f;
        farAway = cam.orthographicSize > 30;

        go_target.SetActive(!isOnScreen || farAway);
        go_health.SetActive(isOnScreen && !farAway);
        go_shield.SetActive(isOnScreen && !farAway && t_ship.maxShield > 0);
        nameTag.gameObject.SetActive(isOnScreen && !farAway);

        nameTag.text = t_ship.Name;
        nameTag.color = GameManager.current.team[t_ship.teamID].teamColor;
        ui_target.color= GameManager.current.team[t_ship.teamID].teamColor;
        go_target.transform.rotation = Quaternion.Euler(0, 0, t_ship.teamID == 0 ? 0 : 45);

        UpdatePosition();
    }

    public void Disable()
    {
        t_ship = null;
        t_trans = null;

        gameObject.SetActive(false);
    }

    Vector3 pos;
    Vector2 point;

    bool _onScreen;
    bool isOnScreen;
    bool _farAway;
    bool farAway;

    private void FixedUpdate()
    {
        if (t_ship == null || Ship.PLAYER == null && GameManager.gameState != GameState.Game) return;

        float dist = (t_trans.position - camTrans.position).sqrMagnitude;
        farAway = dist >= 1000000;

        if (dist < 1000000)
        {
            point = cam.WorldToViewportPoint(t_trans.position);
            isOnScreen = point.x > 0f && point.x < 1f && point.y > 0f && point.y < 1f;

            if (isOnScreen != _onScreen || farAway != _farAway)
            {
                if (isOnScreen) { OnShipApearedOnScreen(); }
                else { OnShipDispearedFromScreen(); }

                go_target.SetActive(!isOnScreen || farAway);
                go_health.SetActive(isOnScreen && !farAway);
                go_shield.SetActive(isOnScreen && !farAway && t_ship.maxShield > 0);
                nameTag.gameObject.SetActive(isOnScreen && !farAway);

                _onScreen = isOnScreen;
                _farAway = farAway;
            }
        }
        else
        {
            if (go_target.activeSelf) go_target.SetActive(false);
            if (go_health.activeSelf) go_health.SetActive(false);
            if (go_shield.activeSelf) go_shield.SetActive(false);

            isOnScreen = false;
            farAway = false;

            _onScreen = isOnScreen;
            _farAway = farAway;
            return;
        }

        //ui_Hunter.enabled = false;

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

            pos.x = 720 * point.x * PlayerUI.ScrRatio;
            pos.y = 720 * point.y;
            rt_tag.anchoredPosition = pos;
        }
        else
        {
            //Vector2 dir = (t_trans.position - Ship.PLAYER.transform.position).normalized;
            Vector2 dir = (point - Vector2.one * 0.5f).normalized;

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
