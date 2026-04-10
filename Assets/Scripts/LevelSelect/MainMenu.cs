using LevelSelectInformation;
using System.Collections;
using System.Linq;
using Systems.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static LevelSelectInformation.BountyInformation;

public class MainMenu : MonoBehaviour {
    [SerializeField] private GameObject wastelandersText;
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject bountyButton;
    [SerializeField] private MainMenuConfigHolder configHolder;
    [SerializeField] private Image background;
    [SerializeField] private RectTransform backgroundTransform;
    [SerializeField] private UIFadeHandler backgroundTransition;
    [SerializeField] private Image backgroundTransitionImage;
    [SerializeField] private RectTransform backgroundTransitionTransform;
    [SerializeField] private AttentionSeeker startAttention;


#nullable enable
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        if (SaveLoadSystem.Instance != null)
        {
            SaveLoadSystem.Instance.SaveGame();
        }

        Application.Quit();
    }

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartClick);
    }

    void OnStartClick()
    {
        StageInformation stage = StageInformation.Stages.LastOrDefault(s => s.UnlockCriteriaMet())
                              ?? StageInformation.Get<StageInformation.Tutorial>();

        GameStateManager.Instance.LoadScene(stage.SceneName);
    }

    private void Start()
    {
#if UNITY_WEBGL
        quitButton.gameObject.SetActive(false);
#endif
        bountyButton.SetActive(GameStateManager.Instance.CurrentLevelProgress >= Get<PrincessFrogBounty>().LevelID && GameStateManager.SEASON_1_ACTIVE);
        //bountyButton.SetActive(false); // Locks this button until part 2 is ready.
        versionText.text = $"v{Application.version}";
        UpdateStatusUI();
        ApplyBGConfig();
        StartCoroutine(TransitionBackground());
        startAttention.ConfigureAttention(GameStateManager.SEASON_1_ACTIVE && Get<PrincessFrogBounty>().UnlockCriteriaMet());
    }

    private IEnumerator TransitionBackground()
    {
        if (GameStateManager.Instance.PreviousScene == SceneData.Get<SceneData.PreBounty0>())
        {
            MainMenuConfig config = configHolder.season1Background;
            background.sprite = config.backgroundImage;
            background.color = new Color(1, 1, 1, config.overlayOpacity / 255f);
            backgroundTransform.sizeDelta = new Vector2(config.width, config.height);

            backgroundTransitionImage.sprite = config.backgroundImage;
            backgroundTransitionTransform.sizeDelta = new Vector2(config.width, config.height);
            backgroundTransition.SetDarkScreen();
            yield return new WaitForSeconds(1f);

            StartCoroutine(backgroundTransition.FadeInLightScreen(2f));
        }
        yield break;
    }


    private void ApplyBGConfig()
    {
        MainMenuConfig config = configHolder.GetConfig();
        background.sprite = config.backgroundImage;
        background.color = new Color(1, 1, 1, config.overlayOpacity / 255f);
        backgroundTransform.sizeDelta = new Vector2(config.width, config.height);
    }

    private void UpdateStatusUI()
    {
        var currentSaveStatus = new GetSaveSystemStatus().Query();
        switch (currentSaveStatus)
        {
            case SaveStatus.Ok:
                statusText.text = "";
                break;
            case SaveStatus.Error error:
                statusText.text = $"Current Status: {error.Message}. Restart Required.";
                statusText.color = Color.red;
                break;
        }
    }


    [SerializeField]
    private float cycleScaling = 2f; // Higher the number, the faster one phase is 
    [SerializeField]
    private float bobbingAmount = 500f; //Amplitude
    private float timer = 0;
    private float verticalOffset = 0;

    //Makes the game over text bob up and down!
    void Update() {
        float previousOffset = verticalOffset;
        float waveslice = Mathf.Sin(cycleScaling * timer);
        timer += Time.deltaTime;
        if (timer > Mathf.PI * 2) {
            timer = timer - (Mathf.PI * 2);
        }

        verticalOffset = waveslice * bobbingAmount;
        float translateChange = verticalOffset - previousOffset;
        wastelandersText.transform.position = new Vector3(wastelandersText.transform.position.x,
            wastelandersText.transform.position.y + translateChange, wastelandersText.transform.position.z);
    }
}

