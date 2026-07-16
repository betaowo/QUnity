using UnityEngine;

public enum HUDStyle
{
    BottomLeft,
    BottomCenter,
    BottomRight,
    TopLeft,
    SideSplit
}

public class QuakeHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HealthComponent plrHealth;
    [SerializeField] private WeaponHolder weaponHolder;

    [Header("HUD Settings")]
    [SerializeField] private HUDStyle hudStyle = HUDStyle.BottomCenter;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private Color hudColor = new Color(0.9f, 0.85f, 0.7f, 1f);
    [SerializeField] private float margin = 20f;
    [SerializeField] private float lineHeight = 22f;

    [Header("Crosshair")]
    [SerializeField] private bool showCrosshair = true;
    [SerializeField] private float crosshairSize = 6f;
    [SerializeField] private Color crosshairColor = Color.white;

    private void Start()
    {
        if (plrHealth == null) plrHealth = GetComponent<HealthComponent>();
        if (weaponHolder == null) weaponHolder = GetComponentInChildren<WeaponHolder>();
    }

    private void OnGUI()
    {
        if (plrHealth == null || plrHealth.IsDead) return;

        GUIStyle big = new GUIStyle(GUI.skin.label);
        big.fontSize = fontSize + 6;
        big.normal.textColor = hudColor;
        big.fontStyle = FontStyle.Bold;
        big.alignment = TextAnchor.UpperLeft;

        GUIStyle small = new GUIStyle(GUI.skin.label);
        small.fontSize = fontSize;
        small.normal.textColor = hudColor;
        small.alignment = TextAnchor.UpperLeft;

        // data
        float hp = plrHealth.CurrentHealth;
        float armor = plrHealth.CurrentArmor;
        string armorText = $"AR: {armor:F0}";

        string ammoLine = "";
        string wepName = "";
        bool isMelee = false;

        if (weaponHolder != null && weaponHolder.CurrentWeapon != null)
        {
            var w = weaponHolder.CurrentWeapon;
            wepName = w.WeaponName;
            isMelee = w.Type == WeaponType.Melee;

            if (!isMelee)
            {
                if (w.HasMagazine)
                    ammoLine = $"{w.CurrentMag}/{w.MagSize}  ({w.ReserveAmmo})";
                else
                    ammoLine = $"{w.CurrentAmmo}/{w.MaxAmmo}";
            }
        }

        switch (hudStyle)
        {
            case HUDStyle.BottomCenter:
                DrawBottomCenter(hp, armorText, ammoLine, wepName, isMelee, big, small);
                break;
            case HUDStyle.SideSplit:
                DrawSideSplit(hp, armorText, ammoLine, wepName, isMelee, big, small);
                break;
            case HUDStyle.BottomLeft:
                DrawStack(new Vector2(margin, Screen.height - margin - lineHeight * (isMelee ? 2 : 3)),
                          hp, armorText, ammoLine, wepName, isMelee, big, small, false);
                break;
            case HUDStyle.BottomRight:
                DrawStack(new Vector2(Screen.width - margin - 200, Screen.height - margin - lineHeight * (isMelee ? 2 : 3)),
                          hp, armorText, ammoLine, wepName, isMelee, big, small, true);
                break;
            case HUDStyle.TopLeft:
                DrawStack(new Vector2(margin, margin),
                          hp, armorText, ammoLine, wepName, isMelee, big, small, false);
                break;
        }

        if (showCrosshair)
            DrawCrosshair();
    }

    private void DrawBottomCenter(float hp, string armorText, string ammoLine, string wepName, bool isMelee,
                                  GUIStyle big, GUIStyle small)
    {
        float w = 320;
        float x = Screen.width / 2f - w / 2f;
        float y = Screen.height - margin - lineHeight * 3;

        GUI.Box(new Rect(x, y, w, lineHeight * 3 + 8), "");
        GUI.Label(new Rect(x + 10, y + 6, w - 20, lineHeight), $"HP: {hp:F0}  |  {armorText}", big);

        if (!isMelee)
            GUI.Label(new Rect(x + 10, y + lineHeight + 6, w - 20, lineHeight), ammoLine, big);

        GUI.Label(new Rect(x + 10, y + lineHeight * 2 + 6, w - 20, lineHeight), wepName, small);
    }

    private void DrawSideSplit(float hp, string armorText, string ammoLine, string wepName, bool isMelee,
                               GUIStyle big, GUIStyle small)
    {
        float boxW = 180;
        float y = Screen.height - margin - lineHeight * 3;

        GUI.Box(new Rect(margin, y, boxW, lineHeight * 3 + 8), "");
        GUI.Label(new Rect(margin + 8, y + 6, boxW - 16, lineHeight), $"HP: {hp:F0}", big);
        GUI.Label(new Rect(margin + 8, y + lineHeight + 6, boxW - 16, lineHeight), armorText, small);

        if (!isMelee)
        {
            GUI.Box(new Rect(Screen.width - margin - boxW, y, boxW, lineHeight * 3 + 8), "");
            GUI.Label(new Rect(Screen.width - margin - boxW + 8, y + 6, boxW - 16, lineHeight), ammoLine, big);
            GUI.Label(new Rect(Screen.width - margin - boxW + 8, y + lineHeight + 6, boxW - 16, lineHeight), wepName, small);
        }
    }

    private void DrawStack(Vector2 pos, float hp, string armorText, string ammoLine, string wepName, bool isMelee,
                           GUIStyle big, GUIStyle small, bool rightAlign)
    {
        float w = 220;
        int lines = isMelee ? 2 : 3;

        if (rightAlign)
        {
            big.alignment = TextAnchor.UpperRight;
            small.alignment = TextAnchor.UpperRight;
        }

        GUI.Box(new Rect(pos.x, pos.y, w, lineHeight * lines + 8), "");
        GUI.Label(new Rect(pos.x + 8, pos.y + 6, w - 16, lineHeight), $"HP: {hp:F0}  {armorText}", big);

        if (!isMelee)
            GUI.Label(new Rect(pos.x + 8, pos.y + lineHeight + 6, w - 16, lineHeight), ammoLine, big);

        GUI.Label(new Rect(pos.x + 8, pos.y + lineHeight * (isMelee ? 1 : 2) + 6, w - 16, lineHeight), wepName, small);

        big.alignment = TextAnchor.UpperLeft;
        small.alignment = TextAnchor.UpperLeft;
    }

    private void DrawCrosshair()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        float s = crosshairSize;

        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(cx - 1, cy - s, 2, s * 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - s, cy - 1, s * 2, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    public void SetStyle(string styleName)
    {
        switch (styleName.ToLower())
        {
            case "center": case "quake": hudStyle = HUDStyle.BottomCenter; break;
            case "split": case "side": hudStyle = HUDStyle.SideSplit; break;
            case "left": hudStyle = HUDStyle.BottomLeft; break;
            case "right": hudStyle = HUDStyle.BottomRight; break;
            case "top": hudStyle = HUDStyle.TopLeft; break;
        }
    }
}