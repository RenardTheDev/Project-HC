using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTag : MonoBehaviour
{
    public Transform t_trans;
    protected RectTransform rt_tag;
    protected RectTransform rt_icon;

    public GameObject go_target;
    public Image ui_target;

    public Text nameTag;

    Camera cam;
    Transform camTrans;

    Vector3 pos;
    Vector2 vp_point;   // position in viewport //

    protected bool isOnScreen;

    private void Awake()
    {
        cam = Camera.main;
        camTrans = cam.transform;
        rt_tag = GetComponent<RectTransform>();
        rt_icon = ui_target.rectTransform;
    }

    public virtual void Assign(Transform target, string name)
    {
        t_trans = target;
        go_target.SetActive(true);

        nameTag.text = name;
    }

    public void Disable()
    {
        t_trans = null;
        gameObject.SetActive(false);
    }


    private void LateUpdate()
    {
        if (t_trans == null || Ship.PLAYER == null || GameManager.gameState != GameState.Game) return;

        UpdateVisibility();
        UpdatePosition();
    }

    public virtual void UpdateVisibility()
    {
        vp_point = cam.WorldToViewportPoint(t_trans.position);
        isOnScreen = vp_point.x > 0f && vp_point.x < 1f && vp_point.y > 0f && vp_point.y < 1f;
    }

    public virtual void UpdatePosition()
    {
        if (isOnScreen)
        {
            rt_tag.anchorMin = Vector2.zero;
            rt_tag.anchorMax = Vector2.zero;

            pos.x = 720 * vp_point.x * PlayerUI.ScrRatio;
            pos.y = 720 * vp_point.y;
            rt_tag.anchoredPosition = pos;
        }
        else
        {
            //Vector2 dir = (t_trans.position - Ship.PLAYER.transform.position).normalized;
            Vector2 dir = (vp_point - Vector2.one * 0.5f).normalized;

            rt_tag.anchorMin = Vector2.one * 0.5f;
            rt_tag.anchorMax = Vector2.one * 0.5f;

            pos.x = 350 * dir.x;
            pos.y = 350 * dir.y;
            rt_tag.anchoredPosition = pos;
        }
    }
}
