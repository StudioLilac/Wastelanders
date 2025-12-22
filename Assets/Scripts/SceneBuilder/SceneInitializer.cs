// SceneInitializer.cs (Upgraded)
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using DialogueScripts;
using Managers;
using UI_Toolkit;

#nullable enable
public class SceneInitializer : MonoBehaviour
{
    public static SceneInitializer Instance { get; private set; } = null!;
    [SerializeField] private SceneInitializerPrefabs initializablePrefabs = null!;
    public SceneInitializerPrefabs InitializablePrefabs => initializablePrefabs;

    private GameObject managersParent = null!;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        InitializeManagersForScene();
    }

    private void InitializeManagersForScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        var requiredManagers = SceneData.FromSceneName(currentSceneName).RequiredPrefabs(initializablePrefabs);
        managersParent = new GameObject("[SceneInitialized]");
        
        foreach (var managerPrefab in requiredManagers)
        {
            // Check if a persistent instance of this manager already exists.
            if (FindFirstObjectByType(managerPrefab.GetType()) != null)
            {
                Debug.LogWarning($"{managerPrefab.GetType()} already exists!");
                continue;
            }
            InstantiatePrefab(managerPrefab);
        }
    }

    public T InstantiatePrefab<T>(T prefab) where T : MonoBehaviour
    {
        var newManagerInstance = Instantiate(prefab, managersParent.transform);
        newManagerInstance.name = prefab.name;
        return newManagerInstance;
    }
}

[Serializable]
public class SceneInitializerPrefabs
{
    public AudioManager audioManager = null!;
    public BattleIntro battleIntro = null!;
    public PauseMenuV2 pauseMenuV2 = null!;
    public HUDV2 hudV2 = null!;
    public DialogueManager dialogueManager = null!;
    public DeckSelectV2 deckSelectV2 = null!;
    public UIFadeScreenManager uiFadeScreenManager = null!;
    public CombatFadeScreenHandler combatFadeScreenManager = null!;
    public PopUpNotificationManager popupManager = null!;
    public GameOver gameOver = null!;
    public DialogueBoxV2 dialogueBoxV2 = null!;
    public ArrowIndicatorManager arrowIndicatorManager = null!;
}