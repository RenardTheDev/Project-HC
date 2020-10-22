using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager current;

    public Team[] team;

    public static GameState gameState = GameState.MainMenu;

    // combo ///
    public float comboTimer;
    public int combo = 1;

    public List<Weapon> weapons;

    // audios //
    public AudioClip sfx_gameOver;

    private void Awake()
    {
        NameGenerator.Setup(4, 8, 2, 1);

        if (current==null)
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        GlobalEvents.onShipKilled += OnShipKilled;
    }

    private void Start()
    {
        /*weapons = new List<Weapon>();

        string[] files = AssetDatabase.FindAssets("t:Weapon");
        for (int i = 0; i < files.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(files[i]);
            var w = AssetDatabase.LoadAssetAtPath<Weapon>(path);

            if (w is Weapon_Homing) continue;

            weapons.Add(w);
        }*/

        //ScreenFade.curr.FadeOUT(0.25f, 1f);
        PlayerUI.current.ToggleStartScreen(true);
    }

    private void OnShipKilled(Damage dmg)
    {
        if (dmg.attacker != null && dmg.attacker.isPlayer)
        {
            if (comboTimer > 0)
            {
                combo++;
            }
            comboTimer = 1;
        }

        if (dmg.victim.isPlayer)
        {
            StartCoroutine(GameOverProcess());
        }
        else
        {
            for (int i = 0; i < team.Length; i++)
            {
                if (AIs[i].Contains(dmg.victim))
                {
                    AIs[i].Remove(dmg.victim);
                    if (TeamPlayerCount(i) < maxPlayers / team.Length)
                        SpawnAI(i);
                }
            }
        }
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
    }

    public int maxPlayers;

    Dictionary<int, List<Ship>> AIs;

    //public List<Ship> AIs = new List<Ship>();
    Coroutine cor_Logic;
    IEnumerator LogicUpdate()
    {
        while (true)
        {
            if (maxPlayers <= 0) maxPlayers = 1;

            Vector3 circle = Random.insideUnitCircle.normalized;

            AsteroidSize size = AsteroidSize.big;
            float rng = Random.value;
            if (rng > 0.25f) size = AsteroidSize.tiny;
            if (rng > 0.50f) size = AsteroidSize.small;
            if (rng > 0.75f) size = AsteroidSize.medium;

            if (Ship.PLAYER != null) AsteroidSystem.current.SpawnAsteroid(Ship.PLAYER.transform.position + circle * 40f, size, true);

            // skirmish //

            if (PlayerCount() < maxPlayers - 1)
            {
                for (int i = 0; i < team.Length; i++)
                {
                    while (TeamPlayerCount(i) < maxPlayers / team.Length)
                    {
                        SpawnAI(i);
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            if (PlayerCount() > maxPlayers - 1)
            {
                for (int i = 0; i < team.Length; i++)
                {
                    if (TeamPlayerCount(i) > maxPlayers / team.Length)
                    {
                        AIs[i][0].Obliterate();
                        AIs[i].RemoveAt(0);
                    }
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnAI(int teamID)
    {
        if (Ship.PLAYER == null || !Ship.PLAYER.isAlive) return;

        var ship = ShipPool.current.SpawnTeamShip(Random.insideUnitCircle * 100, Random.value * 360f, teamID);
        AIs[teamID].Add(ship);
        ship.weap.AssignWeapon(weapons[Random.Range(0, weapons.Count)]);
    }

    int PlayerCount()
    {
        int value = 0;

        for (int i = 0; i < team.Length; i++)
        {
            if (AIs.ContainsKey(i)) value += AIs[i].Count;
        }

        return value;
    }

    int TeamPlayerCount(int team)
    {
        if (AIs.ContainsKey(team))
        {
            return AIs[team].Count;
        }
        else
        {
            return 0;
        }
    }

    IEnumerator GameOverProcess()
    {
        if (cor_Logic != null) StopCoroutine(cor_Logic);

        PlayerUI.current.ToggleControls(false);
        PlayerUI.current.ToggleGameplayUI(false);

        SFXShotSystem.current.SpawnSFX(Camera.main.transform.position, 0.4f, sfx_gameOver, Camera.main.transform);

        yield return new WaitForSeconds(3f);

        ScreenFade.curr.FadeINOUT(0, 1f, 0.25f, 1f);

        yield return new WaitForSeconds(1f);

        ChangeGameState(GameState.MainMenu);

        PlayerUI.current.ToggleStartScreen(true);

        ShipPool.current.HideShips();
        AsteroidSystem.current.HideAsteroids();
    }

    public void TogglePause(bool enable)
    {
        if (gameState != GameState.MainMenu)
        {
            if (enable)
            {
                ChangeGameState(GameState.Pause);
                Time.timeScale = 0;

                PlayerUI.current.ToggleControls(false, true);
                PlayerUI.current.ToggleGameplayUI(false, true);
                PlayerUI.current.TogglePauseScreen(true);
            }
            else
            {
                ChangeGameState(GameState.Game);
                Time.timeScale = 1;

                PlayerUI.current.ToggleControls(true, true);
                PlayerUI.current.ToggleGameplayUI(true, true);
                PlayerUI.current.TogglePauseScreen(false);
            }
        }
    }

    public void StartGame()
    {
        ShipPool.current.SpawnPlayeShip(Vector2.zero, 0f);

        Ship.PLAYER.weap.AssignWeapon(weapons[Random.Range(0, weapons.Count)]);

        PlayerUI.current.ToggleControls(true);
        PlayerUI.current.ToggleGameplayUI(true);
        PlayerUI.current.ToggleStartScreen(false);

        ChangeGameState(GameState.Game);

        AIs = new Dictionary<int, List<Ship>>();
        for (int i = 0; i < team.Length; i++)
        {
            AIs.Add(i, new List<Ship>());
        }

        if (cor_Logic != null) StopCoroutine(cor_Logic);
        cor_Logic = StartCoroutine(LogicUpdate());
    }

    public void GoToMainMenu()
    {
        PlayerUI.current.ToggleControls(false);
        PlayerUI.current.ToggleGameplayUI(false);
        PlayerUI.current.ToggleStartScreen(true);
        PlayerUI.current.TogglePauseScreen(false);

        ChangeGameState(GameState.MainMenu);

        ShipPool.current.HideShips();
        AsteroidSystem.current.HideAsteroids();

        if (cor_Logic != null) StopCoroutine(cor_Logic);
    }

    void ChangeGameState(GameState newState)
    {
        gameState = newState;

        GlobalEvents.GameStateChanged(newState);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

public enum GameState
{
    MainMenu,
    Game,
    Pause
}