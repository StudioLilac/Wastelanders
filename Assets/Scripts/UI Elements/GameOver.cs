using DialogueScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Canvas fadeCanvas;
    [SerializeField] Canvas textCanvas;
    [SerializeField] ButtonFadable restartButton;
    [SerializeField] bool shouldJumpToCombatWhenRestart = true;
    [SerializeField] Button levelSelectButton;
    [SerializeField] Button deckSelectButton;
    [SerializeField] GameObject gameOverText;
    [SerializeField] UIFadeHandler uiFadeScreen;
# nullable enable
    public const float FADE_IN_TIME = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
    }


    void OnEnable()
    {
        restartButton.OnClick += Restart;
        levelSelectButton.onClick.AddListener(() => StartCoroutine(OnLevelSelectClick()));
        deckSelectButton.onClick.AddListener(() => StartCoroutine(OnDeckSelectClick()));
        canvasGroup.alpha = 0f;
        uiFadeScreen.SetLightScreen();
        fadeCanvas.sortingOrder = UISortOrder.GameOverScrim.GetOrder();
        textCanvas.sortingOrder = UISortOrder.GameOverText.GetOrder();
        canvasGroup.blocksRaycasts = false;
    }

    void OnDisable()
    {
        restartButton.OnClick -= Restart;
        levelSelectButton.onClick.RemoveListener(() => StartCoroutine(OnLevelSelectClick()));
        deckSelectButton.onClick.RemoveListener(() => StartCoroutine(OnDeckSelectClick()));
    }

    void Restart() => StartCoroutine(OnRestartClick());

    public void FadeInWithDialogue(DialogueWrapper dialogue)
    {
        StartCoroutine(BeginFadeIn(dialogue));
    }

    private IEnumerator BeginFadeIn(DialogueWrapper dialogue)
    {
        canvasGroup.blocksRaycasts = true;
        AudioManager.Instance.PlayDeath();
        yield return StartCoroutine(uiFadeScreen.FadeInDarkScreen(1.5f));
        StartCoroutine(FadeCoroutine(true, FADE_IN_TIME));
        DialogueBoxV2.Instance.ChangeDialogueBoxOrder(UISortOrder.GameOverDialogue.GetOrder());
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(dialogue));
        DialogueBoxV2.Instance.ChangeDialogueBoxOrder(UISortOrder.DialogueBox.GetOrder());
    }


    private float cycleScaling = 2f; // Higher the number, the faster one phase is 
    private float bobbingAmount = 0.1f; //Amplitude
    private float timer = 0;
    private float verticalOffset = 0;

    //Makes the game over text bob up and down!
    void Update()
    {
        float previousOffset = verticalOffset;
        float waveslice = Mathf.Sin(cycleScaling * timer);
        timer += Time.deltaTime;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }

        verticalOffset = waveslice * bobbingAmount;
        float translateChange = verticalOffset - previousOffset;
        gameOverText.transform.position = new Vector3(gameOverText.transform.position.x, gameOverText.transform.position.y + translateChange, gameOverText.transform.position.z);
    }

    IEnumerator FadeCoroutine(bool isFadingIn, float duration)
    {
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            canvasGroup.alpha = isFadingIn ? t : 1f - t;
            yield return null;
        }

        canvasGroup.alpha = isFadingIn ? 1f : 0f;
    }


    //If your game over isnt registering clicks, check the evenSystem in the hiearchy to see where your clicks are actually landing on 
    private IEnumerator OnRestartClick()
    {
        yield return StartCoroutine(FadeCoroutine(false, 0.7f));
        GameStateManager.Instance.JumpToCombat = shouldJumpToCombatWhenRestart;
        GameStateManager.Instance.Restart();
    }

    private IEnumerator OnLevelSelectClick()
    {
        yield return StartCoroutine(FadeCoroutine(false, 1f));
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.LevelSelect>().SceneName);
    }

    private IEnumerator OnDeckSelectClick()
    {
        yield return StartCoroutine(FadeCoroutine(false, 1f));
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.SelectionScreen>().SceneName);
    }
}
