using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public bool showConsole;
    public bool showHelp;

    public string input;

    public static DebugCommand HELP;

    // game //

    public static DebugCommand KILL_ALL;
    public static DebugCommand<int> SET_MAX_PLAYERS;

    // camera //

    public static DebugCommand<int> SET_ORTHO_SIZE;

    // player //

    public static DebugCommand HEAL;
    public static DebugCommand GOD_MOD;

    // ----- //

    public List<object> commandList;

    private void Awake()
    {
        HELP = new DebugCommand("help", "Shows available console commands.", "help", () =>
        {
            showHelp = true;
        });


        // game //

        KILL_ALL = new DebugCommand("kill_all", "Destroy all alive ships.", "kill_all", () =>
        {
            ShipPool.current.ObliterateShips();
        });
        SET_MAX_PLAYERS = new DebugCommand<int>("max_players", "Sets the maximum player setting.", "max_players [int]", (x) =>
        {
            GameManager.current.maxPlayers = x;
        }, 10);


        // camera //

        SET_ORTHO_SIZE = new DebugCommand<int>("cam_size", "Sets orthographic size of camera.", "cam_size [int=15]", (x) =>
        {
            CameraController.current.SetOrthoSize(x);
        }, 15);


        // player //

        HEAL = new DebugCommand("heal", "Restores player HP.", "heal", () =>
        {
            if (Ship.PLAYER != null) Ship.PLAYER.health = Ship.PLAYER.maxHealth;
        });
        GOD_MOD = new DebugCommand("gm", "Makes player invulnerable.", "gm", () =>
        {
            if (Ship.PLAYER != null) Ship.PLAYER.isInvulnerable = !Ship.PLAYER.isInvulnerable;
        });


        // build command list //

        commandList = new List<object>
        {
            HELP,

            KILL_ALL,
            SET_MAX_PLAYERS,

            SET_ORTHO_SIZE,

            HEAL,
            GOD_MOD,
        };
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
        {
            OnConsoleToggle(!showConsole);
        }
    }

    void OnConsoleToggle(bool toggle)
    {
        showConsole = toggle;
        if (GameManager.gameState == GameState.Game) Time.timeScale = toggle ? 0 : 1;
    }

    Vector2 scroll;
    private void OnGUI()
    {
        if (!showConsole) return;

        float y = 0;

        if (showHelp)
        {
            GUI.Box(new Rect(0, y, Screen.width, 128), "");
            Rect viewport = new Rect(0, y, Screen.width - 30, 20 * commandList.Count);
            scroll = GUI.BeginScrollView(new Rect(0, y + 5, Screen.width, 90), scroll, viewport);

            for (int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase command = commandList[i] as DebugCommandBase;
                string label = $"{command.commandFormat} - {command.commandDescription}";
                Rect labelrect = new Rect(5, 20 * i, viewport.width - 100, 20);
                GUI.Label(labelrect, label);
            }

            GUI.EndScrollView();

            y += 100;
        }

        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);

        Event e = Event.current;
        if (e.keyCode == KeyCode.Return) HandleInput();
    }

    private void HandleInput()
    {
        string[] properties = input.Split(' ');

        for (int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;

            if (input.Contains(commandBase.commandID))
            {
                if (commandList[i] as DebugCommand != null)
                {
                    (commandList[i] as DebugCommand).Invoke();
                }
                else if (commandList[i] as DebugCommand<int> != null)
                {
                    DebugCommand<int> casted = commandList[i] as DebugCommand<int>;
                    if (properties.Length < 2)
                    {
                        casted.Invoke(casted.DefValue);
                    }
                    else
                    {
                        (commandList[i] as DebugCommand<int>).Invoke(int.Parse(properties[1]));
                    }
                }
            }
        }

        //OnConsoleToggle(false);
        input = "";
    }
}