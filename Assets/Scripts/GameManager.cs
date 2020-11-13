using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager current;

    public Team[] team;

    public static GameState gameState = GameState.MainMenu;

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

        if (current==null)
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (GameDataManager.CheckForSavedGame())
        {
            PlayerUI.current.AllowLoadGame(true);
        }
        else
        {

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

    public void test_SpawnMinions()
    {
        for (int i = 0; i < GameDataManager.data.stations.Count; i++)
        {
            var station = GameDataManager.data.GetStation(i);
            for (int p = 0; p < 20; p++)
            {
                Pilot minion = new Pilot(false, false, $"Generic Pilot#{Random.Range(0, 1000):000}", station.teamID, i);
                GameDataManager.data.pilots.Add(minion);
                ShipPool.current.SpawnShip(minion);
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
            GameDataManager.LoadGame();
        }

        WorldCenter = GameDataManager.data.GetWorldCenter();
        WorldRadius = GameDataManager.data.GetWorldRadius();

        ChangeGameState(GameState.Game);

        yield return new WaitForEndOfFrame();
        GameDataManager.SpawnStations();
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

        ChangeGameState(GameState.MainMenu);

        ShipPool.current.HideShips();
        AsteroidSystem.current.HideAsteroids();

        Time.timeScale = 1;

        if (cor_Logic != null) StopCoroutine(cor_Logic);
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