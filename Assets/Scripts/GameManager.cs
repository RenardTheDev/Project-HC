using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public bool friendlyFire = false;

    public AudioMixer audioMixer;
    public AudioMixerSnapshot[] pauseSnapShots;

    public static GameManager current;

    public float comboTimer;
    public int combo = 1;

    public GameState gameState = GameState.start;

    public Weapon startingWeapon;
    public List<Weapon> weapons;
    public List<SkillBase> skills;

    [Header("Difficulty")]
    public static float _enemyDamageMult = 1.0f;
    public static float _enemyHitboxScale = 1.5f;
    public static float _playerDamageMult = 1.0f;
    public float enemyDamageMult = 1.0f;
    public float enemyHitboxScale = 1.5f;
    public float playerDamageMult = 1.0f;

    //--- records ---
    public static PlayerRecords records;

    public AudioClip sfx_player_hit;
    public AudioClip sfx_player_hit_shield;
    public AudioClip sfx_player_hit_shield_break;

    private void Awake()
    {
        weapons.Sort((x, y) => x.WEAPON_ID.CompareTo(y.WEAPON_ID));

        GameData.GameStartUp();
        ShipUpgrade.GenerateUpgrades();

        gameState = GameState.start;
        current = this;
        UpdateGameDifficulty();

        Time.timeScale = 1;
    }

    private void Start()
    {
        GlobalEvents.onShipGetHit += OnShipGetHit;
        GlobalEvents.onShipKilled += OnShipKilled;

        PlayerShipUI.current.ToggleStartScreen(true);

        records = new PlayerRecords(1, 0, 0);

        LoadGameData();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateGameDifficulty();
    }
#endif

    public void UpdateGameDifficulty()
    {
        _enemyDamageMult = enemyDamageMult;
        _playerDamageMult = playerDamageMult;
        _enemyHitboxScale = enemyHitboxScale;
    }

    private void OnShipKilled(Damage data)
    {
        if (gameState == GameState.game && !data.isObliteration)
        {
            if (data.attacker != null && data.attacker.isPlayer)
            {
                if (comboTimer > 0)
                {
                    combo++;
                }
                comboTimer = 1;
                if (data.victim.isHunter)
                {
                    PlayerShipUI.current.ShowCenterMSG($"Hunter killed!", 0.5f, 2f, new Color(1, 0.75f, 0.2f));
                    AddScore((data.victim.maxHealth + data.victim.maxShield) * combo * 2);
                }
                else
                {
                    AddScore((data.victim.maxHealth + data.victim.maxShield) * combo);
                }
                GameData.kills++;
            }
            if (data.victim != null && data.victim.isPlayer)
            {
                //--- gameover screen ---
                if (AdsManager.current.respawnRewardReady)
                {
                    PlayerShipUI.current.ToggleRespawnScreen(true);
                }
                else
                {
                    GameOver();
                    GlobalEvents.GameOver();
                }
            }
        }
    }

    public void RespawnButtons(bool resp)
    {
        if (resp)
        {
            AdsManager.current.ShowAdForRespawn();
        }
        else
        {
            GameOver();
            GlobalEvents.GameOver();
        }

        PlayerShipUI.current.ToggleRespawnScreen(false);
    }

    public void RespawnRewardApply()
    {
        GMSurvival.current.RespawnPlayer();
    }

    private void OnShipGetHit(Damage data)
    {
        if (data.attacker.isPlayer)
        {
            //AddScore(data.hp);
        }

        if (data.victim.isPlayer)
        {
            switch (data.reaction)
            {
                case DamageReactionType.health:
                    {
                        SFXShotSystem.current.SpawnSFX(data.victim.transform.position, 0.05f, sfx_player_hit, data.victim.transform);
                        break;
                    }
                case DamageReactionType.both:
                    {
                        SFXShotSystem.current.SpawnSFX(data.victim.transform.position, 0.05f, sfx_player_hit, data.victim.transform);
                        SFXShotSystem.current.SpawnSFX(data.victim.transform.position, 0.15f, sfx_player_hit_shield_break, data.victim.transform);
                        break;
                    }
                case DamageReactionType.shield:
                    {
                        SFXShotSystem.current.SpawnSFX(data.victim.transform.position, 0.05f, sfx_player_hit_shield, data.victim.transform);
                        break;
                    }
            }
        }
    }

    void AddScore(float score)
    {
        int value = Mathf.CeilToInt(Mathf.Clamp(score, 0, int.MaxValue));
        GameData.score += value;
        GameData.cash += value;
    }

    private void Update()
    {
        if (combo > 1)
        {
            comboTimer = Mathf.MoveTowards(comboTimer, 0f, Time.deltaTime * Mathf.Pow(combo, 0.5f) * 0.2f);

            if (comboTimer <= 0)
            {
                combo = 1;
            }
        }
        else
        {
            comboTimer = Mathf.MoveTowards(comboTimer, 0f, Time.deltaTime * 0.25f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackButtonAction();
        }

        switch (gameState)
        {
            case GameState.start:
                {
                    break;
                }
            case GameState.game:
                {
                    break;
                }
            case GameState.pause:
                {
                    break;
                }
            case GameState.gameover:
                {
                    break;
                }
        }
    }

    public void BackButtonAction()
    {
        switch (gameState)
        {
            case GameState.start:
                {
                    if (SettingsUI.current.canvas.enabled)
                    {
                        SettingsUI.current.ToggleSettingsWindow(false);
                    }
                    break;
                }
            case GameState.game:
                {
                    if (!PlayerUpgradeScreen.current.canvas.enabled)
                    {
                        TogglePause(true);
                    }
                    break;
                }
            case GameState.pause:
                {
                    if (SettingsUI.current.canvas.enabled)
                    {
                        SettingsUI.current.ToggleSettingsWindow(false);
                    }
                    else
                    {
                        TogglePause(false);
                    }
                    break;
                }
            case GameState.gameover:
                {
                    break;
                }
        }
    }

    public void TogglePause(bool enable)
    {
        if (PlayerUpgradeScreen.current.canvas.enabled) return;

        if (enable)
        {
            gameState = GameState.pause;
            Time.timeScale = 0;
            PlayerShipUI.current.ToggleControls(false);
            PlayerShipUI.current.ToggleGameplayUI(false);
            PlayerShipUI.current.TogglePauseScreen(true);

            pauseSnapShots[1].TransitionTo(0.1f);
        }
        else
        {
            gameState = GameState.game;
            Time.timeScale = 1;
            PlayerShipUI.current.ToggleControls(true);
            PlayerShipUI.current.ToggleGameplayUI(true);
            PlayerShipUI.current.TogglePauseScreen(false);

            pauseSnapShots[0].TransitionTo(1f);
        }

    }

    public void GoToStartScreen()
    {
        TogglePause(false);

        PlayerShipUI.current.ToggleControls(false);
        PlayerShipUI.current.ToggleGameplayUI(false);
        PlayerShipUI.current.ToggleGameoverScreen(false);

        PlayerShipUI.current.ToggleStartScreen(true);

        GMSurvival.current.StopAllCoroutines();

        ScreenFade.curr.FadeINOUT(0f, 0f, 1f, 1f);

        MusicManager.current.StopAction();

        StartCoroutine(Obliteration());

        gameState = GameState.start;
    }

    IEnumerator Obliteration()
    {
        var ships = FindObjectsOfType<Ship>();
        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].Obliterate();
            yield return new WaitForSecondsRealtime(UnityEngine.Random.value * 0.25f);
        }
    }

    public void StartGame()
    {
        StartCoroutine(Obliteration());

        PlayerShipUI.current.ToggleGameoverScreen(false);

        GameData.NewGameReset();

        gameState = GameState.game;

        GMSurvival.current.StartSurvivalMode();
        PlayerShipUI.current.ToggleStartScreen(false);

        PlayerShipUI.current.btn_continue.interactable = false;
        PlayerShipUI.current.btn_continue_label.text = "continue";
    }

    public void ContinueGame()
    {
        StartCoroutine(Obliteration());
        PlayerShipUI.current.ToggleGameoverScreen(false);

        GameData.ContinueGame();

        gameState = GameState.game;

        GMSurvival.current.ContinueSurvivalMode();
        PlayerShipUI.current.ToggleStartScreen(false);
    }

    public void RestartGame()
    {
        if (GameData.saved != null)
        {
            ContinueGame();
        }
        else
        {
            StartGame();
        }
    }

    public void GameOver()
    {
        PlayerShipUI.current.ToggleGameoverScreen(true);

        gameState = GameState.gameover;

        if (gameOverCor != null) StopCoroutine(gameOverCor);
        gameOverCor = StartCoroutine(GameOverProcess());
    }

    public void SaveGameState()
    {
        GameData.SaveGame();
    }

    Coroutine gameOverCor;
    IEnumerator GameOverProcess()
    {
        yield return new WaitForSeconds(1f);
        PlayerShipUI.current.ShowCenterMSG("GAME OVER", 0.5f, 4f, Color.red);
        yield return new WaitForSeconds(2f);

        PlayerShipUI.current.StartResultBoxAnimation();

        if (GameData.wave > records.wave) records.wave = GameData.wave;
        if (GameData.kills > records.kills) records.kills = GameData.kills;
        if (GameData.score > records.score) records.score = GameData.score;

        PlayerShipUI.current.btn_continue.interactable = true;
        if (GameData.saved != null) PlayerShipUI.current.btn_gameoverRestart_label.text = $"> wave {GameData.saved.wave + 1}";
        PlayerShipUI.current.btn_gameoverRestart_label.text = GameData.wave >= 10 ? $"> wave {GameData.saved.wave + 1}" : "restart";
        SaveRecords();

        yield return new WaitForSeconds(2f);
        StartCoroutine(Obliteration());
    }

    void LoadGameData()
    {
        //--- records ---
        if (File.Exists(Application.persistentDataPath + "/records.dat"))
        {
            Debug.Log($"Records exists at \n\'{Application.persistentDataPath}/records.dat\'");

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/records.dat");

            records = (PlayerRecords)bf.Deserialize(file);

            file.Close();
        }
    }

    void SaveRecords()
    {
        //--- records ---
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/records.dat");

        bf.Serialize(file, records);
        file.Close();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

public enum GameState
{
    start,
    game,
    pause,
    gameover
}

[System.Serializable]
public class PlayerRecords
{
    public int wave;
    public int score;
    public int kills;

    public PlayerRecords(int wave, int score, int kills)
    {
        this.wave = wave;
        this.score = score;
        this.kills = kills;
    }
}

[System.Serializable]
public class WeaponUpgrade
{
    public int level;
    public bool isEquipped;
}