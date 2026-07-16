using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Commands
{
    // plr link
    public static GameObject player;

    public static void Execute(string line, List<string> log)
    {
        string[] parts = line.Trim().Split(' ');
        string cmd = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        switch (cmd)
        {
            // ===== COMMANDS =====

            case "help":
                log.Add("Available commands:");
                log.Add("  help         - Show this message");
                log.Add("  clear        - Clear console");
                log.Add("  version      - Engine version");
                log.Add("  quit         - Quit the game");
                log.Add("  echo         - Print a message");
                log.Add("  noclip       - Toggle noclip mode");
                log.Add("  god          - Toggle god mode (stub)");
                log.Add("  kill           - Kill yourself");
                log.Add("  map <name>   - Load scene by name");
                log.Add("  restart      - Restart current scene");
                log.Add("  sv_gravity <n> - Set gravity");
                log.Add("  save         - Save game (stub)");
                log.Add("  load         - Load game (stub)");
                log.Add("  impulse <n>  - Weapon impulse (stub)");
                log.Add("  qu_showtel   - Toggle telemetry");
                log.Add("  cl_bob <n>       - Set bob amount");
                log.Add("  cl_rollangle <n>  - Set roll angle");
                log.Add("  fov <n>          - Set field of view");
                break;

            case "clear":
                log.Clear();
                break;

            case "version":
                log.Add("QUnity Engine pre-alpha");
                log.Add("Built on Unity");
                break;

            case "quit":
                log.Add("Quit requested.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;

            case "echo":
                if (args.Length > 0)
                    log.Add(string.Join(" ", args));
                else
                    log.Add("echo: no arguments");
                break;

            case "noclip":
                ToggleNoclip(log);
                break;

            case "god":
                ToggleGod(log);
                break;

            case "kill":
                KillPlayer(log);
                break;

            case "hud_style":
                if (args.Length > 0)
                {
                    var hud = player?.GetComponent<QuakeHUD>();
                    hud?.SetStyle(args[0]);
                    log.Add("hud_style set to " + args[0]);
                }
                else
                {
                    log.Add("hud_style: quake, left, center, right, top");
                }
                break;

            case "map":
                if (args.Length > 0)
                    LoadMap(args[0], log);
                else
                    log.Add("map: specify map name");
                break;

            case "restart":
                RestartScene(log);
                break;

            case "sv_gravity":
                if (args.Length > 0 && float.TryParse(args[0], out float grav))
                    SetGravity(grav, log);
                else
                    log.Add("sv_gravity: specify value (e.g. sv_gravity -800)");
                break;

            case "save":
                SaveLoadSys.Save();
                log.Add("Saved current level.");
                break;

            case "load":
                SaveLoadSys.Load();
                log.Add("Restarting saved level...");
                break;

            case "impulse":
                if (args.Length > 0 && int.TryParse(args[0], out int imp))
                    DoImpulse(imp, log);
                else
                    log.Add("impulse: need a number, genius");
                break;

            case "qu_showtel":
                ToggleTelemetry(log);
                break;

            case "cl_bob":
                if (args.Length > 0 && float.TryParse(args[0], out float bobVal))
                    SetBob(bobVal, log);
                else
                    log.Add("cl_bob: specify value (e.g. cl_bob 0.02)");
                break;

            case "cl_rollangle":
                if (args.Length > 0 && float.TryParse(args[0], out float rollVal))
                    SetRollAngle(rollVal, log);
                else
                    log.Add("cl_rollangle: specify value (e.g. cl_rollangle 2.0)");
                break;

            case "fov":
                if (args.Length > 0 && float.TryParse(args[0], out float fovVal))
                    SetFOV(fovVal, log);
                else
                    log.Add("fov: specify value (e.g. fov 110)");
                break;

            case "pear":
            case "груша":
                log.Add("You found the pear! International9 sends his regards.");
                break;

            default:
                log.Add("Unknown command \"" + cmd + "\"");
                break;
        }
    }

    // ===== CMD METHODS =====

    private static void ToggleNoclip(List<string> log)
    {
        if (player == null)
        {
            log.Add("noclip: player not found");
            return;
        }

        var gm = player.GetComponent<gameMovement>();
        if (gm == null)
        {
            log.Add("noclip: gameMovement not found on player");
            return;
        }

        gm.noclip = !gm.noclip;
        log.Add("noclip " + (gm.noclip ? "ON" : "OFF"));
    }

    private static void ToggleGod(List<string> log)
    {
        if (player == null)
        {
            log.Add("god: player not found");
            return;
        }

        var health = player.GetComponent<HealthComponent>();
        if (health == null)
        {
            log.Add("god: HealthComponent not found");
            return;
        }

        health.IsGodMode = !health.IsGodMode;
        log.Add("god mode " + (health.IsGodMode ? "ON" : "OFF"));
    }

    private static void KillPlayer(List<string> log)
    {
        if (player == null)
        {
            log.Add("kill: player not found");
            return;
        }

        var health = player.GetComponent<HealthComponent>();
        if (health == null)
        {
            log.Add("kill: HealthComponent not found");
            return;
        }

        health.TakeDamage(99999);
        log.Add("Player killed.");
    }

    private static void LoadMap(string mapName, List<string> log)
    {
        if (Application.CanStreamedLevelBeLoaded(mapName))
        {
            SceneManager.LoadScene(mapName);
        }
        else
        {
            log.Add("Unknown map \"" + mapName + "\"");
        }
    }

    private static void RestartScene(List<string> log)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static void SetGravity(float value, List<string> log)
    {
        if (player == null)
        {
            log.Add("sv_gravity: player not found");
            return;
        }

        var gm = player.GetComponent<gameMovement>();
        if (gm == null)
        {
            log.Add("sv_gravity: gameMovement not found");
            return;
        }

        gm.movevars.gravity = value;
        log.Add("sv_gravity set to " + value);
    }

    // impulse handler — weapon switch + other shit later
    private static void DoImpulse(int imp, List<string> log)
    {
        if (player == null)
        {
            log.Add("impulse: plr not found");
            return;
        }

        var holder = player.GetComponentInChildren<WeaponHolder>();
        if (holder == null)
        {
            log.Add("impulse: WeaponHolder not found");
            return;
        }

        switch (imp)
        {
            case 1: holder.SwitchWeapon(0); break;  // axe
            case 2: holder.SwitchWeapon(1); break;  // shotgun
            case 3: holder.SwitchWeapon(2); break;  // supershotgun
            case 4: holder.SwitchWeapon(3); break;  // nailgun
            case 5: holder.SwitchWeapon(4); break;  // supernailgun
            case 6: holder.SwitchWeapon(5); break;  // grenadelauncher
            case 7: holder.SwitchWeapon(6); break;  // rocketlauncher
            case 8: holder.SwitchWeapon(7); break;  // thunderbolt
            case 9: log.Add("impulse 9: all weapons (stub)"); break;
            case 10: holder.CycleWeapon(1); log.Add("next wep"); break;
            case 11: holder.CycleWeapon(-1); log.Add("prev wep"); break;
            case 12: // impulse 12: drop cur wep (stub)
                log.Add("impulse 12: drop wep (stub)");
                break;
            default:
                log.Add("impulse " + imp + " — unknown, whatever");
                break;
        }
    }

    private static void ToggleTelemetry(List<string> log)
    {
        var overlay = player.GetComponent<DeveloperOverlay>();
        if (overlay == null)
        {
            log.Add("qu_showtel: DeveloperOverlay not found in scene");
            return;
        }

        overlay.Toggle();
        log.Add("telemetry toggled");
    }

    private static void SetBob(float value, List<string> log)
    {
        if (player == null)
        {
            log.Add("cl_bob: player not found");
            return;
        }

        var camBob = player.GetComponentInChildren<CameraBob>();
        if (camBob == null)
        {
            log.Add("cl_bob: CameraBob not found");
            return;
        }

        camBob.SetBobValue(value);
        log.Add("cl_bob set to " + value);
    }

    private static void SetRollAngle(float value, List<string> log)
    {
        if (player == null)
        {
            log.Add("cl_rollangle: player not found");
            return;
        }

        var camBob = player.GetComponentInChildren<CameraBob>();
        if (camBob == null)
        {
            log.Add("cl_rollangle: CameraBob not found");
            return;
        }

        camBob.SetRollAngle(value);
        log.Add("cl_rollangle set to " + value);
    }

    private static void SetFOV(float value, List<string> log)
    {
        if (player == null)
        {
            log.Add("fov: player not found");
            return;
        }

        var aiming = player.GetComponentInChildren<PlayerAiming>();
        if (aiming == null)
        {
            log.Add("fov: PlayerAiming not found");
            return;
        }

        if (value <= 0) 
        {
            aiming.ResetFOV();
            log.Add("fov reset to default");
        }
        else
        {
            aiming.SetFOV(value);
            log.Add("fov set to " + value);
        }
    }
}