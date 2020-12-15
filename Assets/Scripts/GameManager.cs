using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    public Team[] team;

    public static GameState gameState = GameState.MainMenu;

    // Layer masks //

    public LayerMask ai_enemySearchMask;

    // combo ///

    public List<Weapon> weapons;

    // audios //
    public AudioClip sfx_gameOver;

    public Vector2 WorldCenter;
    public float WorldRadius;

    private void Awake()
    {
        //prefabs = _prefabs;

        NameGenerator.Setup(4, 8, 2, 1);

        if (inst==null)
        {
            inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        PlayerUI.current.AllowLoadGame(GameDataManager.CheckForSavedGame());

        GlobalEvents.OnShipKilled += OnShipKilled;
    }

    private void OnShipKilled(Damage dmg)
    {
        Pilot victim = dmg.victim.pilot;
        if (dmg.deathReason == DeathReason.killed && !victim.data.important)
        {
            GameDataManager.RemovePilot(victim);
        }
    }

    private void Start()
    {
        PlayerUI.current.ToggleStartScreen(true);
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

            // asteroids //

            Vector3 circle = Random.insideUnitCircle.normalized;

            AsteroidSize size = AsteroidSize.big;
            float rng = Random.value;
            if (rng > 0.25f) size = AsteroidSize.tiny;
            if (rng > 0.50f) size = AsteroidSize.small;
            if (rng > 0.75f) size = AsteroidSize.medium;

            if (Ship.PLAYER != null) AsteroidSystem.current.SpawnAsteroid(Ship.PLAYER.transform.position + circle * Camera.main.orthographicSize * 2.5f, size, true);

            yield return new WaitForSeconds(1f);
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

    public void StartGame(bool newGame)
    {
        StartCoroutine(StartingSequence(newGame));
    }

    public void TransferToNewLocation(Station station)
    {
        Station currStation = GameDataManager.GetCurrentStation();
        if (currStation.data.ID == station.data.ID) return;

        GameDataManager.data.currentActiveStation = station.data.ID;

        GlobalEvents.ActiveStationChanged(currStation, station);
    }

    public void test_SpawnMinions()
    {
        for (int i = 0; i < GameDataManager.stations.Count; i++)
        {
            var station = GameDataManager.GetStation(i);
            for (int p = 0; p < 20; p++)
            {
                Pilot minion = new Pilot(false, false, "Generic Pilot", station.data.teamID, i);
                GameDataManager.AddPilot(minion);
                minion.data.Name = $"pilot_{minion.data.baseStationID}_{minion.data.ID}";
                minion.data.currStationID = i;

                if (i == GameDataManager.data.currentActiveStation)
                {
                    ShipPool.current.SpawnShip(minion);
                }
            }
        }
    }

    IEnumerator StartingSequence(bool newGame)
    {
        ScreenFade.curr.FadeINOUT(0, 1f, 1f, 1f);
        yield return new WaitForSecondsRealtime(1f);

        PlayerUI.current.ToggleControls(true);
        PlayerUI.current.ToggleGameplayUI(true);
        PlayerUI.current.ToggleStartScreen(false);

        if (newGame)
        {
            GameDataManager.GenerateNewGameData();
        }
        else
        {
            GameDataManager.LoadGameData();
        }

        WorldCenter = GameDataManager.data.GetWorldCenter();
        WorldRadius = GameDataManager.data.GetWorldRadius();

        ChangeGameState(GameState.Game);

        yield return new WaitForEndOfFrame();
        GameDataManager.SpawnActiveStation();
        yield return new WaitForEndOfFrame();
        GameDataManager.SpawnShips();
        yield return new WaitForEndOfFrame();

        if (cor_Logic != null) StopCoroutine(cor_Logic);
        cor_Logic = StartCoroutine(LogicUpdate());
    }

    public void GoToMainMenu()
    {
        ScreenFade.curr.FadeINOUT(0, 0f, 0.1f, 1f);

        PlayerUI.current.ToggleControls(false);
        PlayerUI.current.ToggleGameplayUI(false);
        PlayerUI.current.ToggleStartScreen(true);
        PlayerUI.current.TogglePauseScreen(false);

        GameDataManager.SaveGame();

        ShipPool.current.HideShips();
        AsteroidSystem.current.HideAsteroids();

        ChangeGameState(GameState.MainMenu);

        Time.timeScale = 1;

        if (cor_Logic != null) StopCoroutine(cor_Logic);

        PlayerUI.current.AllowLoadGame(GameDataManager.CheckForSavedGame());
    }

    void ChangeGameState(GameState newState)
    {
        GlobalEvents.GameStateChanged(gameState, newState);
        gameState = newState;
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