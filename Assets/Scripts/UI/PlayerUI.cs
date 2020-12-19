using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI inst;
    public static float ScrRatio;

    Camera cam;
    Transform camTrans;

    public ShipStatsUI shipStats;

    [Header("Skill")]
    public GameObject skillBox;
    public Image skill_fill;

    [Header("Combo")]
    public GameObject combo_go;
    public Text combo_label;
    public Image combo_fill;

    [Header("Messages")]
    public Text center_msg;
    public Image center_msg_bg;
    public Gradient colorRoll_gradient;
    Color colorRoll;
    float showTime;

    [Space]
    public GameObject jumper_msg;
    public Image jumper_fill;
    public Text jumper_label;

    Color epilepsiBlink;

    [Header("UI Shake")]
    public RectTransform shakePivot;
    [CinemachineImpulseChannelProperty]
    public int shakeListener_mask;
    public float shakeGain;

    //--- links ---
    Ship target;
    ShipWeapon target_weapon;

    private void Awake()
    {
        inst = this;

        cam = Camera.main;
        camTrans = cam.transform;

        cmsg = center_msg.rectTransform;

        ScrRatio = (float)Screen.width / Screen.height;
        shakePivot.sizeDelta = new Vector2(720f * ScrRatio, 720f);
    }

    private void Start()
    {
        GlobalEvents.OnPlayerChangedWeapon += OnPlayerChangedWeapon;
        GlobalEvents.OnPlayerChangedSkill += OnPlayerChangedSkill;
    }

    private void OnPlayerChangedWeapon(Weapon weap)
    {

    }

    private void OnPlayerChangedSkill(SkillBase skill)
    {

    }

    private void Update()
    {
        ScrRatio = (float)Screen.width / Screen.height;

        if (target == null) return;

        //--- skill ---
        if (target_weapon.skill != null)
        {
            if (!skillBox.activeSelf) skillBox.SetActive(true);
            float pCD = target_weapon.skill_cd;
            skill_fill.fillAmount = pCD;
        }
        else
        {
            if (skillBox.activeSelf) skillBox.SetActive(false);
        }
        //--------------

        //--- combo ---

        if (Action_Combo.combo > 1)
        {
            if (!combo_go.activeSelf) combo_go.SetActive(true);

            combo_fill.fillAmount = Action_Combo.comboTimer;
            combo_label.text = $"x{Action_Combo.combo}";
        }
        else
        {
            if (combo_go.activeSelf) combo_go.SetActive(false);
        }

        //-------------

        if (showTime >= 0)
        {
            colorRoll = colorRoll_gradient.Evaluate((Mathf.Sin(showTime) + 1) * 0.5f);
            showTime += Time.deltaTime * 4;
            center_msg.color = colorRoll;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            CameraController.current.ExplosionImpulse(Random.onUnitSphere * 50, 5f);
        }

        CinemachineImpulseManager.Instance.GetImpulseAt(camTrans.position, true, shakeListener_mask, out offsetPos, out offsetRot);
        shakePivot.position = offsetPos * shakeGain * cam.orthographicSize * 2;
    }

    Vector3 offsetPos;
    Quaternion offsetRot;

    public void AssignTarget(Ship ship)
    {
        target = ship;
        target_weapon = target.GetComponent<ShipWeapon>();

        shipStats.AssigneTarget(ship);
    }

    [Header("Canvases")]
    public Canvas ui_gameplay;
    public Canvas ui_controls;
    public Canvas ui_startScreen;
    public Canvas ui_pauseScreen;
    public Canvas ui_messages;

    [Header("Canvas groupes")]
    public CanvasGroup gr_gameplay;
    public CanvasGroup gr_controls;

    [Header("Confirmations")]
    public GameObject mainButtons;
    public GameObject newGameConfirm;
    public GameObject exitConfirm;

    [Header("Buttons")]
    public Button loadGame;

    public void AllowLoadGame(bool enable)
    {
        loadGame.interactable = enable;
    }

    public void Confirmation_NewGame(bool open)
    {
        mainButtons.SetActive(!open);
        newGameConfirm.SetActive(open);
    }

    public void Confirmation_ExitGame(bool open)
    {
        mainButtons.SetActive(!open);
        exitConfirm.SetActive(open);
    }

    public void ToggleControls(bool enable, bool instant = false)
    {
        if (instant)
        {
            ui_controls.enabled = enable;
            gr_controls.alpha = enable ? 1 : 0;
        }
        else
        {
            StartCoroutine(UIGroupFader(ui_controls, gr_controls, enable));
        }
    }

    public void ToggleGameplayUI(bool enable, bool instant = false)
    {
        if (instant)
        {
            ui_gameplay.enabled = enable;
            gr_gameplay.alpha = enable ? 1 : 0;
        }
        else
        {
            StartCoroutine(UIGroupFader(ui_gameplay, gr_gameplay, enable));
        }
    }

    Coroutine uiFadeCor;
    IEnumerator UIGroupFader(Canvas ui, CanvasGroup group, bool show)
    {
        if (show)
        {
            ui.enabled = true;
            group.alpha = 0;
            while (group.alpha < 1f)
            {
                group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.unscaledDeltaTime * 2);
                yield return new WaitForEndOfFrame();
            }
            group.alpha = 1;
        }
        else
        {
            group.alpha = 1;
            while (group.alpha > 0)
            {
                group.alpha = Mathf.MoveTowards(group.alpha, 0, Time.unscaledDeltaTime * 2);
                yield return new WaitForEndOfFrame();
            }
            group.alpha = 0;
            ui.enabled = false;
        }
    }
    public void ToggleStartScreen(bool enable) { ui_startScreen.enabled = enable; }
    public void TogglePauseScreen(bool enable) { ui_pauseScreen.enabled = enable; }

    public void ShowCenterMSG(string text, float speed, float hold, Color color)
    {
        if (cMSG_cor != null)
        {
            StopCoroutine(cMSG_cor);
        }
        showTime = -1;
        cMSG_cor = StartCoroutine(CenterMSGPopup(text, speed, hold, color));
    }

    public void ShowCenterMSG(string text, float speed, float hold, Gradient color)
    {
        if (cMSG_cor != null)
        {
            StopCoroutine(cMSG_cor);
        }
        colorRoll_gradient = color;
        showTime = 0.01f;
        cMSG_cor = StartCoroutine(CenterMSGPopup(text, speed, hold, color.Evaluate(0)));
    }

    Coroutine cMSG_cor;
    RectTransform cmsg;
    float scale = 0;
    IEnumerator CenterMSGPopup(string text, float showupTime, float hold, Color color)
    {
        center_msg.text = text;

        center_msg_bg.rectTransform.sizeDelta = new Vector2(center_msg_bg.rectTransform.sizeDelta.x,
            Mathf.Max(100, center_msg.preferredHeight + 32));

        center_msg.color = color;

        scale = 0;

        while (true)
        {
            scale = Mathf.MoveTowards(scale, 1.0f, Time.deltaTime / showupTime);

            center_msg_bg.fillAmount = Mathf.Clamp01(scale * 2);
            cmsg.localScale = Vector3.one * Mathf.Clamp01((scale - 0.5f) * 2);

            if (scale >= 1)
            {
                scale = 1;
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(hold);

        while (true)
        {
            scale = Mathf.MoveTowards(scale, 0.0f, Time.deltaTime / showupTime);

            center_msg_bg.fillAmount = Mathf.Clamp01(scale * 2);
            cmsg.localScale = Vector3.one * Mathf.Clamp01((scale - 0.5f) * 2);

            if (scale <= 0)
            {
                scale = 0;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}