using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Text[] btnTexts;      // 0=Play, 1=Settings, 2=Info, 3=Quit
    [SerializeField] private string[] btnLabels = { "PLAY", "SETTINGS", "INFO", "QUIT" };

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject infoPanel;

    [Header("Settings")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Text fovValueText;
    [SerializeField] private Dropdown qualityDropdown;

    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioSource audioSrc;

    private int selectedIndex = 0;
    private bool inSettings, inInfo;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (audioSrc == null) audioSrc = GetComponent<AudioSource>();

        UpdateSelection();

        // settings defaults
        if (fovSlider != null)
        {
            fovSlider.value = PlayerPrefs.GetFloat("fov", 90f);
            fovSlider.onValueChanged.AddListener(OnFovChanged);
            OnFovChanged(fovSlider.value);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }
    }

    private void Update()
    {
        if (inSettings || inInfo) return;

        // keyboard nav
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = (selectedIndex - 1 + btnTexts.Length) % btnTexts.Length;
            UpdateSelection();
            PlayHover();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = (selectedIndex + 1) % btnTexts.Length;
            UpdateSelection();
            PlayHover();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectButton();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inSettings) CloseSettings();
            else if (inInfo) CloseInfo();
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < btnTexts.Length; i++)
        {
            btnTexts[i].text = (i == selectedIndex ? "> " : "  ") + btnLabels[i];
            btnTexts[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
        }
    }

    private void SelectButton()
    {
        PlayClick();

        switch (selectedIndex)
        {
            case 0: // Play
                SceneManager.LoadScene(1);
                break;
            case 1: // Settings
                OpenSettings();
                break;
            case 2: // Info
                OpenInfo();
                break;
            case 3: // Quit
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    public void OnBtnClick(int index)
    {
        selectedIndex = index;
        UpdateSelection();
        SelectButton();
    }

    public void OnBtnHover(int index)
    {
        selectedIndex = index;
        UpdateSelection();
        PlayHover();
    }

    private void OpenSettings()
    {
        inSettings = true;
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        inSettings = false;
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    private void OpenInfo()
    {
        inInfo = true;
        mainPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        inInfo = false;
        infoPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    private void OnFovChanged(float val)
    {
        if (fovValueText != null)
            fovValueText.text = $"FOV: {val:F0}";
        PlayerPrefs.SetFloat("fov", val);
    }

    private void OnQualityChanged(int level)
    {
        QualitySettings.SetQualityLevel(level);
        PlayerPrefs.SetInt("quality", level);
    }

    private void PlayHover()
    {
        if (hoverSound != null)
            audioSrc.PlayOneShot(hoverSound, 0.4f);
    }

    private void PlayClick()
    {
        if (clickSound != null)
            audioSrc.PlayOneShot(clickSound, 0.6f);
    }
}