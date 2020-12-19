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

    Vector3 onScrPos;
    Vector3 onCirclePos;
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

    float blend;
    public virtual void UpdatePosition()
    {
        rt_tag.anchorMin = Vector2.zero;
        rt_tag.anchorMax = Vector2.zero;

        Vector2 dir = Vector3.zero;
        if (isOnScreen)
        {
            blend = 1f;
        }
        else
        {
            blend = Mathf.MoveTowards(blend, 0f, Time.deltaTime * 2);
        }

        // on screen //

        onScrPos.x = 720 * vp_point.x * PlayerUI.ScrRatio;
        onScrPos.y = 720 * vp_point.y;

        // on circle //

        vp_point.x = vp_point.x * PlayerUI.ScrRatio;
        dir = (vp_point - (new Vector2(PlayerUI.ScrRatio, 1f) * 0.5f)).normalized;
        onCirclePos.x = 360 * PlayerUI.ScrRatio + 350 * dir.x;
        onCirclePos.y = 360 + 350 * dir.y;

        rt_tag.anchoredPosition = Vector3.Lerp(onCirclePos, onScrPos, blend);
    }
}