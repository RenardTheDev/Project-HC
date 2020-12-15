using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public bool showConsole;

    public string input;

    public static DebugCommand HELP;

    public static List<string> lines;
    public static List<string> lastInputs;
    int lastSelectedInput = -1;

    // game //

    public static DebugCommand KILL_ALL;
    public static DebugCommand<float> TIME_SCALE;
    //public static DebugCommand<int> SET_MAX_PLAYERS;

    public static DebugCommand<int> PILOTS;

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
            for (int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase command = commandList[i] as DebugCommandBase;
                string label = $"{command.commandFormat} - {command.commandDescription}";
                WriteLine(label);
            }
        });

        lines = new List<string>(); 
        lastInputs = new List<string>();

        // game //

        KILL_ALL = new DebugCommand("kill_all", "Destroy all alive ships.", "kill_all", () =>
        {
            ShipPool.current.ObliterateShips();
        });
        TIME_SCALE = new DebugCommand<float>("time_scale", "Sets Time.timeScale value.", "time_scale [float]", (x) =>
        {
            Time.timeScale = x;
        }, 1f);
        /*SET_MAX_PLAYERS = new DebugCommand<int>("max_players", "Sets the maximum player setting.", "max_players [int]", (x) =>
        {
            GameManager.inst.maxPlayers = x;
        }, 10);*/

        PILOTS = new DebugCommand<int>("pilots", "Shows list of pilots in current game. [0 - In game manager data, 1 - search by class]", "pilots [int]", (x) =>
            {
                switch (x)
                {
                    case 0:
                        {
                            foreach (var p in GameDataManager.pilots)
                            {
                                WriteLine($"[{p.Key}] {p.Value.data.Name}, " +
                                    $"baseStation = [{p.Value.data.baseStationID}] {GameDataManager.GetStation(p.Value.data.baseStationID).data.Name}, " +
                                    $"currStationID = [{p.Value.data.currStationID}] {GameDataManager.GetStation(p.Value.data.currStationID).data.Name}");
                            }
                            break;
                        }
                    case 1:
                        {
                            WriteLine("<color=grey>Not implemented</color>");
                            /*Pilot[] pilots = FindObjectsOfType(typeof(Pilot));

                            foreach (var p in GameDataManager.pilots)
                            {
                                WriteLine($"[{p.Key}] {p.Value.data.Name}, " +
                                    $"baseStation = [{p.Value.data.baseStationID}] {GameDataManager.GetStation(p.Value.data.baseStationID).data.Name}, " +
                                    $"currStationID = [{p.Value.data.currStationID}] {GameDataManager.GetStation(p.Value.data.currStationID).data.Name}");
                            }*/
                            break;
                        }
                    default:    WriteLine("<color=red>Error:</color> pilots [0-1]");
                        break;
                }
                
            }, 0);


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
            TIME_SCALE,
            //SET_MAX_PLAYERS,
            PILOTS,

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

        if (showConsole && lastInputs.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                lastSelectedInput++;
                if (lastSelectedInput > lastInputs.Count-1) lastSelectedInput = lastInputs.Count - 1;

                input = lastInputs[lastSelectedInput];
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                lastSelectedInput--;
                if (lastSelectedInput < 0) lastSelectedInput = 0;

                input = lastInputs[lastSelectedInput];
            }
        }
    }

    void OnConsoleToggle(bool toggle)
    {
        showConsole = toggle;
        if (GameManager.gameState == GameState.Game) Time.timeScale = toggle ? 0 : 1;
    }

    public void WriteLine(string line)
    {
        lines.Add(line);
        viewport = new Rect(0, 0, Screen.width - 30, 20 * lines.Count);
        scroll = new Vector2(0, viewport.height - Screen.height * 0.75f);
    }

    Vector2 scroll;
    Rect viewport;
    private void OnGUI()
    {
        if (!showConsole) return;

        float y = 0;

        // console log //

        GUI.Box(new Rect(0, y, Screen.width, Screen.height * 0.75f + 28), "");
        viewport = new Rect(0, 0, Screen.width - 30, 20 * lines.Count);
        scroll = GUI.BeginScrollView(new Rect(0, y + 5, Screen.width, Screen.height * 0.75f), scroll, viewport);

        for (int i = 0; i < lines.Count; i++)
        {
            string label = lines[i];
            Rect labelrect = new Rect(5, 20 * i, viewport.width - 100, 20);
            GUI.Label(labelrect, label);
        }

        GUI.EndScrollView();

        y += Screen.height * 0.75f + 10;

        // input field //

        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);

        Event e = Event.current;
        if (e.keyCode == KeyCode.Return) HandleInput();
    }

    private void HandleInput()
    {
        if (input.Length < 1) return;

        WriteLine($"<color=red>></color> {input}");
        lastInputs.Insert(0, input);
        lastSelectedInput = -1;

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