using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipStatsUI : MonoBehaviour
{
    public Ship target;

    public Image hit_glow;

    [Header("Health")]
    public Color hp_Color;
    public Image hp_fill;
    public Text[] hp_labels;
    //public Image hp_hit_glow;
    public Image low_hp_warn;

    public AnimationCurve warn_anim;
    float warnTime;

    [Header("Shield")]
    public Color sh_Color;
    public GameObject sh_box;
    public Image sh_fill;
    public Text[] sh_labels;
    //public Image sh_hit_glow;
    public Image shieldBreak_warn;

    private void Start()
    {
        GlobalEvents.onShipGetHit += OnShipGetHit;
    }

    public void AssigneTarget(Ship ship)
    {
        target = ship;
    }

    private void OnShipGetHit(Damage dmg)
    {
        if (dmg.victim != target) return;

        switch (dmg.reaction)
        {
            case DamageReactionType.health:
                {
                    hit_glow.color = hp_Color;
                    break;
                }
            case DamageReactionType.shield:
                {
                    hit_glow.color = sh_Color;
                    break;
                }
            case DamageReactionType.both:
                {
                    hit_glow.color = hp_Color;
                    shieldBreak_warn.color = sh_Color;
                    break;
                }
        }
    }

    private void Update()
    {
        if (target == null) return;

        //--- health ---
        float pHP = target.health;
        float pmaxHP = target.maxHealth;

        hp_labels[0].text = pHP.ToString("0");
        hp_labels[1].text = pmaxHP.ToString("0");

        hp_fill.fillAmount = pHP / pmaxHP;

        var tempColor = hit_glow.color;
        tempColor.a = Mathf.MoveTowards(tempColor.a, 0, Time.deltaTime);
        hit_glow.color = tempColor;

        if (pHP < 20)
        {
            if (!low_hp_warn.gameObject.activeSelf) { low_hp_warn.gameObject.SetActive(true); warnTime = 0; }

            low_hp_warn.color = new Color(255, 0, 0, warn_anim.Evaluate(Mathf.Repeat(warnTime, 1f)));

            warnTime += Time.deltaTime;
        }
        else
        {
            if (low_hp_warn.gameObject.activeSelf) low_hp_warn.gameObject.SetActive(false);
        }
        //--------------

        //--- shield ---
        if (target.maxShield > 0)
        {
            if (!sh_box.activeSelf) sh_box.SetActive(true);

            float pSH = target.shield;
            float pmaxSH = target.maxShield;

            sh_labels[0].text = pSH.ToString("0");
            sh_labels[1].text = pmaxSH.ToString("0");

            sh_fill.fillAmount = pSH / pmaxSH;

            tempColor = shieldBreak_warn.color;
            tempColor.a = Mathf.MoveTowards(tempColor.a, 0, Time.deltaTime);
            shieldBreak_warn.color = tempColor;
        }
        else
        {
            if (sh_box.activeSelf) sh_box.SetActive(false);
        }
        //--------------
    }


}
