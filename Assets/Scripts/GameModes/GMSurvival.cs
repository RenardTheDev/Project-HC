using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEditor;
using UnityEngine;

public class GMSurvival : MonoBehaviour
{
    public static GMSurvival current;

    public int maxAttackers = 5;
    public int currAttackers = 0;

    public List<ShipAI> shipAI = new List<ShipAI>();

    public Color NewWaveMSGColor;
    public Color MilestoneMSGColor;

    public Gradient WarningColor;

    public float playerAliveTime = 0;

    public int saveEveryWave = 5;

    public static WaveType CurrentWaveType = WaveType.normal;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        GlobalEvents.onShipGetHit += OnShipGetHit;
        GlobalEvents.onShipKilled += OnShipKilled;

        GlobalEvents.onShipPoolCreatedNewInstance += OnCreatedNewShipInstance;
    }

    private void OnCreatedNewShipInstance(Ship ship)
    {
        shipAI.Add(ship.GetComponent<ShipAI>());
    }

    public void StartSurvivalMode()
    {
        if (playerSpawn != null) StopCoroutine(playerSpawn);
        playerSpawn = StartCoroutine(SpawnPlayer(1f, false));

        playerAliveTime = 0;
        currEnemiesAlive = 0;

        if (cor_newWave != null) StopCoroutine(cor_newWave);
        cor_newWave = StartCoroutine(StartNewWave());

        if (AttackersAssigner_cor != null) StopCoroutine(AttackersAssigner_cor);
        AttackersAssigner_cor = StartCoroutine(AttackersAssigner());
    }

    public void ContinueSurvivalMode()
    {
        if (playerSpawn != null) StopCoroutine(playerSpawn);
        playerSpawn = StartCoroutine(SpawnPlayer(1f, true));

        if (cor_newWave != null) StopCoroutine(cor_newWave);
        cor_newWave = StartCoroutine(StartNewWave());

        if (AttackersAssigner_cor != null) StopCoroutine(AttackersAssigner_cor);
        AttackersAssigner_cor = StartCoroutine(AttackersAssigner());
    }

    private void OnShipKilled(Damage data)
    {
        if (!data.isObliteration)
        {
            data.victim.isHunter = false;
            //--- NPC KILLED ---
            if (data.victim != null && !data.victim.isPlayer)
            {
                enemiesLeft--;
                currEnemiesAlive--;

                if (enemiesLeft > 0)
                {
                    if (currEnemiesAlive < enemiesLeft && currEnemiesAlive < maxEnemiesAlive)
                    {
                        SpawnRegularEnemy();
                    }
                }
                else
                {
                    //--- wave complete ---
                    if (cor_waveComplete != null) StopCoroutine(cor_waveComplete);
                    cor_waveComplete = StartCoroutine(WaveComplete());
                }
            }

            //--- PLAYER KILLED ---
            if (data.victim != null && data.victim.isPlayer)
            {
                if (cor_waveComplete != null) StopCoroutine(cor_waveComplete);
                if (playerSpawn != null) StopCoroutine(playerSpawn);
                //playerSpawn = StartCoroutine(SpawnPlayer(5f));

                PlayerShipUI.current.ToggleControls(false);
                PlayerShipUI.current.ToggleGameplayUI(false);
            }
        }
    }

    public void RespawnPlayer()
    {
        if (AttackersAssigner_cor != null) StopCoroutine(AttackersAssigner_cor);
        AttackersAssigner_cor = StartCoroutine(AttackersAssigner());

        if (playerSpawn != null) StopCoroutine(playerSpawn);
        playerSpawn = StartCoroutine(SpawnPlayer(1f, true, true));
    }

    private void OnShipGetHit(Damage data)
    {

    }

    private void Update()
    {
        if (GameManager.current.gameState == GameState.game)
        {
            playerAliveTime += Time.deltaTime;
        }
    }

    Coroutine AttackersAssigner_cor;
    IEnumerator AttackersAssigner()
    {
        while (true)
        {
            if (Random.value > 0.75f) shipAI.Sort((x, y) => (x.transform.position - Ship.PLAYER.transform.position).magnitude.CompareTo((y.transform.position - Ship.PLAYER.transform.position).magnitude));

            currAttackers = 0;
            for (int i = 0; i < shipAI.Count; i++)
            {
                var sh = shipAI[i];
                if (sh.isPlayer || sh.health <= 0)
                {
                    sh.isAttacker = false;
                    continue;
                }

                if (currAttackers < maxAttackers)
                {
                    currAttackers++;
                    sh.isAttacker = true;
                }
                else
                {
                    sh.isAttacker = false;
                }

                yield return 0;
            }

            Vector3 circle = Random.insideUnitCircle.normalized;

            AsteroidSize size = AsteroidSize.big;
            float rng = Random.value;
            if (rng > 0.25f) size = AsteroidSize.tiny;
            if (rng > 0.50f) size = AsteroidSize.small;
            if (rng > 0.75f) size = AsteroidSize.medium;

            if (Ship.PLAYER != null) AsteroidSystem.current.SpawnAsteroid(Ship.PLAYER.transform.position + circle * 40f, size, true);

            yield return new WaitForSeconds(1f);
        }
    }

    Coroutine playerSpawn;
    IEnumerator SpawnPlayer(float timer, bool loadedGame, bool respGame = false)
    {
        yield return new WaitForSeconds(timer);

        var ship = ShipPool.current.SpawnShip(Vector2.zero, 0, true, "Player");

        CameraController.current.AssignTarget(ship.transform);
        PlayerShipUI.current.AssignTarget(ship);

        ship.GetComponent<ShipAI>().ToggleAI(false);

        PlayerShipUI.current.ToggleControls(true);
        PlayerShipUI.current.ToggleGameplayUI(true);

        if (loadedGame)
        {
            ship.shipUpgrades = respGame ? GameData.respSave.shipUpgrades : GameData.saved.shipUpgrades;
            ship.weapUpgrades = respGame ? GameData.respSave.weapUpgrades : GameData.saved.weapUpgrades;
            ship.ApplyUpgrades();

            ship.health = ship.maxHealth;
            ship.shield = ship.maxShield;

            ship.weap.AssignWeapon(GameManager.current.weapons[respGame ? GameData.respSave.equippedWeapID : GameData.saved.equippedWeapID]);
        }
        else
        {
            ship.shipUpgrades = new Dictionary<int, int>();
            foreach (int upg in System.Enum.GetValues(typeof(UpgradeType)))
            {
                ship.shipUpgrades.Add(upg, 1);
            }

            ship.weapUpgrades = new Dictionary<int, int>();
            foreach (var weap in GameManager.current.weapons)
            {
                ship.weapUpgrades.Add(weap.WEAPON_ID, 0);
            }
            ship.weapUpgrades[GameManager.current.startingWeapon.WEAPON_ID] = 1;

            ship.ApplyUpgrades();

            ship.weap.AssignWeapon(GameManager.current.startingWeapon);

            GameData.respSave.shipUpgrades = ship.shipUpgrades;
            GameData.respSave.weapUpgrades = ship.weapUpgrades;
            GameData.respSave.equippedWeapID = ship.equippedWeapID;
        }
    }

    //Coroutine enemiesRespawn;
    public int enemiesLeft = 0;
    public int enemiesForWave { get => 8 + GameData.wave * 2; }
    public int currEnemiesAlive = 0;
    public int maxEnemiesAlive = 5;

    //public int wave = 1;

    Ship SpawnEnemyShip()
    {
        currEnemiesAlive++;

        Vector3 rand = Random.insideUnitCircle.normalized * 50;
        Vector3 spawn = Ship.PLAYER.transform.position + rand;

        var ship = ShipPool.current.SpawnShip(spawn, Random.value * 360f, false, $"Pilot#{Random.value * 1000:0000}", 1);

        var ai = ship.GetComponent<ShipAI>();
        ai.ToggleAI(true);

        if (!shipAI.Contains(ai))
        {
            shipAI.Add(ai);
        }

        return ship;
    }

    void SpawnRegularEnemy()
    {
        var ship = SpawnEnemyShip();

        ship.shipUpgrades = new Dictionary<int, int>();
        foreach (int upg in System.Enum.GetValues(typeof(UpgradeType)))
        {
            ship.shipUpgrades.Add(upg, 1);
        }

        ship.weapUpgrades = new Dictionary<int, int>();
        foreach (var weap in GameManager.current.weapons)
        {
            ship.weapUpgrades.Add(weap.WEAPON_ID, 0);
        }

        ship.ApplyUpgrades();

        int weapID = Random.Range(0, GameManager.current.weapons.Count);
        ship.weapUpgrades[GameManager.current.weapons[weapID].WEAPON_ID] = 1;

        ship.weap.AssignWeapon(GameManager.current.weapons[weapID]);
    }

    void SpawnHunter()
    {
        var ship = SpawnEnemyShip();

        ship.shipUpgrades = Ship.PLAYER.shipUpgrades;
        ship.weapUpgrades = Ship.PLAYER.weapUpgrades;
        ship.ApplyUpgrades();

        ship.health = ship.maxHealth;
        ship.shield = ship.maxShield;

        ship.weap.AssignWeapon(GameManager.current.weapons[Ship.PLAYER.equippedWeapID]);

        ship.isHunter = true;
    }

    Coroutine cor_waveComplete;
    IEnumerator WaveComplete()
    {
        GlobalEvents.RoundEnded();
        if (GameData.wave % saveEveryWave == 0)
        {
            PlayerShipUI.current.ShowCenterMSG($"Wave <color=white>#{GameData.wave}</color> complete!", 0.5f, 3f, NewWaveMSGColor);
            yield return new WaitForSeconds(4f);
            PlayerShipUI.current.ShowCenterMSG($"Milestone achieved\nProgress saved", 0.5f, 3f, MilestoneMSGColor);
            yield return new WaitForSeconds(4f);
            PlayerShipUI.current.shipUpgrades.ToggleUpgradeScreen(true);
        }
        else
        {
            PlayerShipUI.current.ShowCenterMSG($"Wave <color=white>#{GameData.wave}</color> complete!", 0.5f, 3f, NewWaveMSGColor);
            yield return new WaitForSeconds(4f);
            PlayerShipUI.current.shipUpgrades.ToggleUpgradeScreen(true);
        }
    }

    public void StartWaveAfterUpgrades()
    {
        PlayerUpgradeScreen.current.ToggleUpgradeScreen(false);

        if (GameData.wave % saveEveryWave == 0) GameData.SaveGame();

        StartCoroutine(StartNewWave());
    }

    Coroutine cor_newWave;
    IEnumerator StartNewWave()
    {
        yield return new WaitForSeconds(1.5f);
        GameData.wave++;
        bool spawnHunter = (Random.value < 0.5f && GameData.wave >= 2) || GameData.wave % saveEveryWave == 0;
        CurrentWaveType = spawnHunter ? WaveType.hunter : WaveType.normal;

        if (spawnHunter)
        {
            PlayerShipUI.current.ShowCenterMSG(
                $"Warning, HUNTER incoming!\n" +
                $"<color=#{ColorUtility.ToHtmlStringRGB(NewWaveMSGColor)}>Wave <color=white>#{GameData.wave}</color></color>",
                0.5f, 3f, WarningColor);
        }
        else
        {
            PlayerShipUI.current.ShowCenterMSG(
                $"<color=#{ColorUtility.ToHtmlStringRGB(NewWaveMSGColor)}>Wave <color=white>#{GameData.wave}</color> incoming!</color>",
                0.5f, 3f, NewWaveMSGColor);
        }
        enemiesLeft = enemiesForWave;

        GlobalEvents.RoundStarted(CurrentWaveType);

        yield return new WaitForSeconds(4f);

        for (int i = 0; i < Mathf.Min(enemiesLeft, maxEnemiesAlive); i++)
        {
            //StartCoroutine(SpawnEnemy(1f));
            if (i == 0 && spawnHunter)
            {
                SpawnHunter();
            }
            else
            {
                SpawnRegularEnemy();
            }
            yield return 0;
        }
    }
}

public enum WaveType
{
    normal,
    hunter
}