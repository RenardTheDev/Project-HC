using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShipUI : MonoBehaviour
{
    public static PlayerShipUI current;
    public static float ScrRatio;

    public ShipStatsUI shipStats;
    public PlayerUpgradeScreen shipUpgrades;

    public Button btn_continue;
    public Text btn_continue_label;
    public Text btn_gameoverRestart_label;

    [Header("Skill")]
    public GameObject skillBox;
    public Image skill_fill;

    [Header("Score")]
    public Text score_field;
    public float score_show;

    [Header("Cash")]
    public Text cash_field;
    public float cash_show;

    [Header("Combo")]
    public GameObject combo_go;
    public Text combo_label;
    public Image combo_fill;

    [Header("Game")]
    public Text wave_counter;
    public Text enemies_counter;
    public Image enemies_filler;

    [Header("Center MSG")]
    public Text center_msg;
    public Image center_msg_bg;
    public Gradient colorRoll_gradient;
    Color colorRoll;
    float showTime;

    [Header("GameOverScreen")]
    public GameObject gameOverButtons;
    public RectTransform resultBox;
    public Text scoring;
    public Text records;

    [Header("Weapon")]
    public Transform weapUI_parent;
    public GameObject weaponUI_go;

    Color epilepsiBlink;

    //--- links ---
    Ship target;
    ShipWeapon target_weapon;

    private void Awake()
    {
        current = this;

        cmsg = center_msg.rectTransform;
    }

    private void Start()
    {
        GlobalEvents.onPlayerChangedWeapon += OnPlayerChangedWeapon;
        GlobalEvents.onPlayerChangedSkill += OnPlayerChangedSkill;
    }

    private void OnPlayerChangedWeapon(Weapon weap)
    {
        if (weaponUI_go != null) Destroy(weaponUI_go, 0);
        weaponUI_go = Instantiate(weap.ui_prefab, weapUI_parent);
        weaponUI_go.SendMessage("AssignWeapon", weap);
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

        //--- score ---
        int pScore = GameData.score;
        score_show = Mathf.CeilToInt(Mathf.Lerp(score_show, pScore, Time.deltaTime * 10));
        int digits = $"{score_show:0}".Length;
        score_field.text = "";
        if (9 - digits > 0)
        {
            score_field.text = "<color=grey>";
            for (int i = 0; i < 9 - digits; i++) score_field.text += "0";
            score_field.text += "</color>";
        }
        score_field.text += $"{score_show:0}";
        //-------------

        //--- cash ---

        int pCash = GameData.cash;
        cash_show = Mathf.Lerp(cash_show, pCash, Time.deltaTime * 10);
        cash_field.text = $"{cash_show:0} <size={(int)(cash_field.fontSize * 0.75f)}>HC</size>";
        //-------------

        //--- combo ---

        if (GameManager.current.combo > 1)
        {
            if (!combo_go.activeSelf) combo_go.SetActive(true);

            combo_fill.fillAmount = GameManager.current.comboTimer;
            combo_label.text = $"x{GameManager.current.combo}";
        }
        else
        {
            if (combo_go.activeSelf) combo_go.SetActive(false);
        }

        //-------------

        //--- game info ---
        if (GameData.wave == 0)
        {
            wave_counter.text = "0";
            enemies_counter.text = "0";
            enemies_filler.fillAmount = 0;
        }
        else
        {
            wave_counter.text = $"wave {GameData.wave}";
            enemies_counter.text = $"{GMSurvival.current.enemiesLeft}";
            enemies_filler.fillAmount = (float)GMSurvival.current.enemiesLeft / GMSurvival.current.enemiesForWave;
        }
        //-----------------

        epilepsiBlink.r = Mathf.Round(Random.value);
        epilepsiBlink.g = Mathf.Round(Random.value);
        epilepsiBlink.b = Mathf.Round(Random.value);
        epilepsiBlink.a = 1f;

        records.color = epilepsiBlink;

        if (showTime >= 0)
        {
            colorRoll = colorRoll_gradient.Evaluate((Mathf.Sin(showTime) + 1) * 0.5f);
            showTime += Time.deltaTime * 4;
            center_msg.color = colorRoll;
        }
    }

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
    public Canvas ui_gameoverScreen;
    public Canvas ui_pauseScreen;
    public Canvas ui_respawn;

    [Header("Canvas groupes")]
    public CanvasGroup gr_gameplay;
    public CanvasGroup gr_controls;

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
    public void ToggleGameoverScreen(bool enable)
    {
        ui_gameoverScreen.enabled = enable;
        gameOverButtons.gameObject.SetActive(false);
        resultBox.sizeDelta = new Vector2(900, 0);
    }
    public void TogglePauseScreen(bool enable) { ui_pauseScreen.enabled = enable; }
    public void ToggleRespawnScreen(bool enable) { ui_respawn.enabled = enable; }

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

    public void StartResultBoxAnimation()
    {
        StartCoroutine(ResultBoxAnimation());

        scoring.text = $"{GameData.wave}\n" +
            $"{GameData.score}\n" +
            $"{GameData.kills}";

        records.text = $"{(GameData.wave > GameManager.records.wave ? "New record!" : $"<color=white>record {GameManager.records.wave}</color>")}\n" +
            $"{(GameData.score > GameManager.records.score ? "New record!" : $"<color=white>record {GameManager.records.score}</color>")}\n" +
            $"{(GameData.kills > GameManager.records.kills ? "New record!" : $"<color=white>record {GameManager.records.kills}</color>")}";
    }

    IEnumerator ResultBoxAnimation()
    {
        gameOverButtons.gameObject.SetActive(false);
        resultBox.sizeDelta = new Vector2(900, 0);
        ToggleGameoverScreen(true);

        while (true)
        {
            resultBox.sizeDelta = Vector2.MoveTowards(resultBox.sizeDelta, new Vector2(900, 400), Time.deltaTime * 200);
            if(resultBox.sizeDelta.y>= 400)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(3f);

        gameOverButtons.gameObject.SetActive(true);
    }
}