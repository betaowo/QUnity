using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private List<WeaponBase> weapons = new List<WeaponBase>();
    [SerializeField] private int currentWeaponIndex = 0;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction[] numberKeyActions;

    public WeaponBase CurrentWeapon => (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
        ? weapons[currentWeaponIndex] : null;

    private void Awake()
    {
        // only enable starting wep (axe)
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].IsCurrentWeapon = (i == currentWeaponIndex);
            weapons[i].gameObject.SetActive(weapons[i].IsOwned && weapons[i].IsCurrentWeapon);
        }
    }

    private void OnEnable()
    {
        fireAction = playerInput.actions["Fire"];
        reloadAction = playerInput.actions["Reload"];
        fireAction.Enable();
        reloadAction.Enable();

        numberKeyActions = new InputAction[10];
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            numberKeyActions[i] = new InputAction($"Weapon{index}", binding: $"<Keyboard>/{index}");
            numberKeyActions[i].performed += ctx => SwitchWeapon(index - 1);
            numberKeyActions[i].Enable();
        }
    }

    private void OnDisable()
    {
        fireAction.Disable();
        reloadAction.Disable();
        foreach (var action in numberKeyActions)
            action.Disable();
    }

    private void Update()
    {
        if (CurrentWeapon == null || !CurrentWeapon.IsOwned) return;

        WeaponBase weapon = CurrentWeapon;

        if (weapon.IsAuto)
        {
            if (fireAction.IsPressed())
                weapon.Fire();
        }
        else
        {
            if (fireAction.WasPressedThisFrame())
                weapon.Fire();
        }

        if (reloadAction.WasPressedThisFrame())
            weapon.Reload();
    }

    public void SwitchWeapon(int index)
    {
        // find next OWNED wep
        int target = index;
        if (target < 0 || target >= weapons.Count) return;
        if (!weapons[target].IsOwned) return;
        if (target == currentWeaponIndex) return;

        // kill flash on old wep
        var oldFlash = weapons[currentWeaponIndex].GetComponentInChildren<MuzzleFlash>();
        if (oldFlash != null) oldFlash.StopAllCoroutines();

        // disable old
        var oldAnim = weapons[currentWeaponIndex].GetComponent<WeaponAnimator>();
        oldAnim?.StopCurrentAnimation();
        weapons[currentWeaponIndex].IsCurrentWeapon = false;
        weapons[currentWeaponIndex].gameObject.SetActive(false);

        // enable new
        currentWeaponIndex = target;
        weapons[currentWeaponIndex].IsCurrentWeapon = true;
        weapons[currentWeaponIndex].gameObject.SetActive(true);

        var newAnim = weapons[currentWeaponIndex].GetComponent<WeaponAnimator>();
        newAnim?.ResetAllBlendShapes();
    }

    // switch to next/prev owned wep
    public void CycleWeapon(int dir)
    {
        int count = weapons.Count;
        for (int i = 1; i < count; i++)
        {
            int idx = (currentWeaponIndex + dir * i + count) % count;
            if (weapons[idx].IsOwned)
            {
                SwitchWeapon(idx);
                return;
            }
        }
    }

    // give wep — adds to inventory
    public void GiveWeapon(string name)
    {
        foreach (var w in weapons)
        {
            if (w.WeaponName == name && !w.IsOwned)
            {
                w.IsOwned = true;
                Debug.Log($"Picked up: {name}");
                return;
            }
        }
    }

    // give wep by index
    public void GiveWeapon(int index)
    {
        if (index >= 0 && index < weapons.Count)
        {
            weapons[index].IsOwned = true;
        }
    }

    // check if owned
    public bool HasWeapon(string name)
    {
        foreach (var w in weapons)
        {
            if (w.WeaponName == name && w.IsOwned)
                return true;
        }
        return false;
    }

    public bool HasWeapon(int index)
    {
        return index >= 0 && index < weapons.Count && weapons[index].IsOwned;
    }

    // get wep by name
    public WeaponBase GetWeaponByName(string name)
    {
        foreach (var w in weapons)
        {
            if (w.WeaponName == name)
                return w;
        }
        return null;
    }

    // save/load
    public string[] GetWeaponNames()
    {
        string[] names = new string[weapons.Count];
        for (int i = 0; i < weapons.Count; i++)
            names[i] = weapons[i].WeaponName;
        return names;
    }

    public int GetCurWeaponIndex() => currentWeaponIndex;

    public void LoadWeapons(string[] savedNames, int curIdx)
    {
        foreach (var w in weapons)
        {
            w.IsOwned = false;
            w.IsCurrentWeapon = false;
            w.gameObject.SetActive(false);
        }

        foreach (string name in savedNames)
        {
            foreach (var w in weapons)
            {
                if (w.WeaponName == name)
                {
                    w.IsOwned = true;
                    break;
                }
            }
        }

        if (curIdx >= 0 && curIdx < weapons.Count && weapons[curIdx].IsOwned)
        {
            currentWeaponIndex = curIdx;
            weapons[curIdx].IsCurrentWeapon = true;
            weapons[curIdx].gameObject.SetActive(true);
        }
    }

    public List<WeaponBase> GetAllWeapons()
    {
        return weapons;
    }
}