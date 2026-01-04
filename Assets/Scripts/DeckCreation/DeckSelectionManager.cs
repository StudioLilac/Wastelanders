using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CardDatabase;
using UnityEngine.SceneManagement;
using System.Linq;
using Systems.Persistence;
using WeaponDeckSerialization;
using UnityEditor;
using System;
using TMPro;

public class DeckSelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject characterSelectionUi;
    [SerializeField] private GameObject weaponSelectionUi;
    [SerializeField] private GameObject deckSelectionUi;
    [SerializeField] private GameObject cardArrayParent;
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private PlayerDatabase playerDatabase;
    [SerializeField] private TMP_Text cardTitleTextField;
    [SerializeField] private TMP_Text cardDescriptorTextField;
    [SerializeField] private Transform enemyEditParent;
    [SerializeField] private GameObject enemyEditButtonPrefab;

    [Serializable]
    private struct CharacterSpritePair
    {
        public PlayerDatabase.PlayerName playerName;
        public Sprite sprite;
    }

    [SerializeField] private SpriteRenderer selectedCharacterIndicator;
    [SerializeField] private CharacterSpritePair[] characterSpriteIndicators; // Used for the top right corner selected character indicater

    private PlayerDatabase.PlayerData playerData;
    private WeaponType weaponType;
    public WeaponAmount weaponText;
    public PointsAmount pointsText;
    public BuffExplainer buffExplainer;
    public static DeckSelectionManager Instance { get; private set; }
#nullable enable
    public delegate void PlayerActionDeckDelegate(int points);
    public event PlayerActionDeckDelegate? PlayerActionDeckModifiedEvent;

    private string nextScene = SceneData.Get<SceneData.LevelSelect>().SceneName;

    private DeckSelectionState deckSelectionState;
    private DeckSelectionState DeckSelectionState //Might want to swap out this state machine for an event driven changing phases.
    {
        get
        {
            return deckSelectionState;
        }
        set
        {
            deckSelectionState = value;
            switch (value)
            {
                case DeckSelectionState.CharacterSelection:
                    PerformCharacterSelection();
                    break;

                case DeckSelectionState.WeaponSelection:
                    PerformWeaponSelection();
                    break;
                case DeckSelectionState.DeckSelection:
                    PerformDeckSelection();
                    break;
            }

            OnDeckSelectStateChanged?.Invoke(deckSelectionState);
        }
    }

    public static event Action<DeckSelectionState>? OnDeckSelectStateChanged;
    public static event Action<int, List<ActionClass>>? OnRenderDecks;

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

    void Start()
    {
        ActionClass.CardClickedEvent += ActionSelected;
        ActionClass.CardRightClickedEvent += CardRightClicked;
        ActionClass.CardHighlightedEvent += RenderCardInformation;
        ActionClass.CardUnhighlightedEvent += RemoveCardInformation;
        CharacterSelect.CharacterSelectedEvent += CharacterChosen;
        WeaponSelect.WeaponSelectEvent += WeaponSelected;
        WeaponEdit.WeaponEditEvent += WeaponDeckEdit;
        EnterDeckSelection();
    }

    private void EnterDeckSelection()
    {
        UIFadeScreenManager.Instance.SetDarkScreen();
        StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(2f));
    }

    void OnDestroy()
    {
        ActionClass.CardClickedEvent -= ActionSelected;
        ActionClass.CardRightClickedEvent -= CardRightClicked;
        ActionClass.CardHighlightedEvent -= RenderCardInformation;
        ActionClass.CardUnhighlightedEvent -= RemoveCardInformation;
        CharacterSelect.CharacterSelectedEvent -= CharacterChosen;
        WeaponSelect.WeaponSelectEvent -= WeaponSelected;
        WeaponEdit.WeaponEditEvent -= WeaponDeckEdit;
    }

    public void PrevState()
    {
        if (DeckSelectionState == DeckSelectionState.WeaponSelection)
        {
            DeckSelectionState = DeckSelectionState.CharacterSelection;
        }
        else if (DeckSelectionState == DeckSelectionState.DeckSelection)
        {
            DeckSelectionState = DeckSelectionState.WeaponSelection;
        }
        else if (DeckSelectionState == DeckSelectionState.CharacterSelection)
        {
            ExitDeckSelection();
        }
    }

    public void OnHomeButtonClicked()
    {
        ExitDeckSelection();
    }

    private void ExitDeckSelection()
    {
        SaveLoadSystem.Instance.SaveGame();
        //EditorUtility.SetDirty(playerDatabase); // For easily resetting the default weaponDeck of playerDatabase
        GameStateManager.Instance.LoadScene(nextScene);
    }
    public void SetNextScene(string newScene)
    {
        nextScene = newScene;
    }

    private void CharacterChosen(PlayerDatabase.PlayerName playerName)
    {
        playerData = playerDatabase.GetDataByPlayerName(playerName);
        DeckSelectionState = DeckSelectionState.WeaponSelection;
        selectedCharacterIndicator.sprite = characterSpriteIndicators.First((s) => s.playerName == playerName).sprite;
        selectedCharacterIndicator.gameObject.SetActive(true);
    }

    private void WeaponSelected(WeaponSelect c, CardDatabase.WeaponType weaponType)
    {
        if (playerData.selectedWeapons.Contains(weaponType))
        {
            playerData.selectedWeapons.Remove(weaponType);
            c.SetSelected(false);
            weaponText.TextUpdate(playerData.selectedWeapons.Count.ToString() + "/2 Selected");
        }
        else if (playerData.selectedWeapons.Count < 2)
        {
            playerData.selectedWeapons.Add(weaponType);
            c.SetSelected(true);
            weaponText.TextUpdate(playerData.selectedWeapons.Count.ToString() + "/2 Selected");
        }
        else
        {
            Debug.LogWarning("Can only select 2 weapons");
        }
    }

    private void WeaponDeckEdit(WeaponEditInformation weaponEditInformation)
    {
        this.weaponType = weaponEditInformation.WeaponType;
        buffExplainer.RenderExplanationForBuff(weaponType);
        DeckSelectionState = DeckSelectionState.DeckSelection;
        RenderDecks(weaponEditInformation);
        OnUpdateDeck(playerData.GetProficiencyPointsTuple(weaponType));
    }

    private bool DeckContainsCard(ActionClass ac)
    {
        return playerData.GetPlayerWeaponDeck(weaponType).weaponDeck.FirstOrDefault(action => action.ActionClassName == ac.GetType().Name) != null;
    }

    private void OnUpdateDeck(WeaponProficiency weaponPointTuple)
    {
        int availablePoints = weaponPointTuple.MaxPoints - weaponPointTuple.CurrentPoints;
        pointsText.TextUpdate("Select Your Cards\nAvailable Points: <color=#FFD700>" + availablePoints.ToString() + "</color>");

        PlayerActionDeckModifiedEvent?.Invoke(availablePoints);
    }

    // PERF: DeckContainsCard finds an actionFound but in doesn't return it, which is searched for again here.
    private void DeselectFromDeck(ActionClass ac, bool performChecks = true)
    {
        SerializableWeaponListEntry playerWeaponDeck = playerData.GetPlayerWeaponDeck(weaponType);
        WeaponProficiency weaponPointTuple = playerData.GetProficiencyPointsTuple(weaponType);

        if (!performChecks || DeckContainsCard(ac))
        {
            weaponPointTuple.CurrentPoints -= ac.CostToAddToDeck;
            ac.SetSelectedForDeck(false);
            var actionFound = playerWeaponDeck.weaponDeck.FirstOrDefault(action => action.ActionClassName == ac.GetType().Name);
            playerWeaponDeck.weaponDeck.Remove(actionFound);
            OnUpdateDeck(weaponPointTuple);
        }
    }

    private void AddToDeck(ActionClass ac, bool performChecks = true)
    {
        SerializableWeaponListEntry playerWeaponDeck = playerData.GetPlayerWeaponDeck(weaponType);
        WeaponProficiency weaponPointTuple = playerData.GetProficiencyPointsTuple(weaponType);

        // Do we have sufficient points? If so, are we trying to add the evolved form? If so, is the evolution progress sufficient?
        if ((!performChecks || weaponPointTuple.CurrentPoints + ac.CostToAddToDeck <= weaponPointTuple.MaxPoints) && (!ac.IsFlipped || (ac.IsFlipped && ac.CanEvolve())))
        {
            weaponPointTuple.CurrentPoints += ac.CostToAddToDeck;
            ac.SetSelectedForDeck(true);
            playerWeaponDeck.weaponDeck.Add(new(ac.GetType().Name, ac.IsFlipped && ac.CanEvolve()));
            OnUpdateDeck(weaponPointTuple);
        }
        else
        {
            Debug.LogWarning("Insufficient experience points");
        }
    }

    private void FlipCard(ActionClass ac)
    {
        ac.IsFlipped = !ac.IsFlipped; // Invert IsFlipped status
        ac.cardUI.RenderCard(ac); // Re-render the card based on the new flipped state
    }

    private void CardRightClicked(ActionClass ac)
    {
        // Deselect the card as to not allow duplicate selecting
        // DeselectFromDeck(ac);
        // Flip the card itself
        // FlipCard(ac);
        // Re-call OnMouseEnter so that we re-render the card description popup
        // ac.OnMouseEnter();
    }

    // Handles when a card is clicked event
    private void ActionSelected(ActionClass ac)
    {
        if (DeckContainsCard(ac))
        {
            DeselectFromDeck(ac, false);
        }
        else
        {
            AddToDeck(ac);
        }
    }


    private void PerformCharacterSelection()
    {
        characterSelectionUi.SetActive(true);
        weaponSelectionUi.SetActive(false);
        deckSelectionUi.SetActive(false);
        selectedCharacterIndicator.gameObject.SetActive(false);
    }

    private void PerformWeaponSelection()
    {
        characterSelectionUi.SetActive(false);
        weaponSelectionUi.SetActive(true);
        deckSelectionUi.SetActive(false);
        weaponText.TextUpdate(playerData.selectedWeapons.Count.ToString() + "/2 Selected");
        foreach (Transform child in weaponSelectionUi.transform)
        {
            WeaponSelect deckItem = child.GetComponent<WeaponSelect>();
            if (deckItem)
            {
                deckItem.SetSelected(playerData.selectedWeapons.Contains(deckItem.type));
            }
        }
    }

    private void PerformDeckSelection()
    {
        characterSelectionUi.SetActive(false);
        weaponSelectionUi.SetActive(false);
        deckSelectionUi.SetActive(true);
    }

    private void RenderCardInformation(ActionClass card)
    {
        cardTitleTextField.text = card.GetName().ToUpper();
        cardDescriptorTextField.text = card.GenerateCardDescription();
    }

    private void RemoveCardInformation(ActionClass card)
    {
        cardTitleTextField.text = "";
        cardDescriptorTextField.text = "";
    }

    private void GenerateSubFolders(WeaponType weaponType)
    {
        float y = 0f;
        float delta = -1f;
        foreach (ISubWeaponType subWeapon in GetUnlockedSubFoldersFor(weaponType))
        {
            if (!subWeapon.IsUnlocked()) continue;

            GameObject button = Instantiate(enemyEditButtonPrefab);
            button.transform.SetParent(enemyEditParent);
            button.transform.localPosition = new Vector3(0, y, 0);
            y += delta;

            WeaponEdit weaponEdit = button.GetComponentInChildren<WeaponEdit>();
            weaponEdit.editText.SetText(subWeapon.Name);
            weaponEdit.InitializeWeaponEdit(weaponType, true, subWeapon.GetSubWeaponCards);
        }
    }

    //Renders the weaponDeck corresponding to (@param weaponType)
    public void RenderDecks(WeaponEditInformation weaponEditInformation)
    {
        UnrenderDecks();

        WeaponType weaponType = weaponEditInformation.WeaponType;
        List<ActionClass> chosenCardList = cardDatabase.ConvertStringsToCards(weaponType, playerData.GetDeckByWeaponType(weaponType).Select(p => p.ActionClassName).ToList());
        List<ActionClass> cardsToRender = weaponEditInformation.GetCards(cardDatabase);
        List<GameObject> instantiatedCards = new List<GameObject>();
        int cols;

        if (weaponEditInformation.ShowSubFolders)
        {
            cols = 3;
            GenerateSubFolders(weaponType);
        }
        else
        {
            cols = cardsToRender.Count > 8 ? 5 : 4;
        }

        //In order to sort, the cards must be instantiated and initialized first :pensive:
        foreach (ActionClass card in cardsToRender)
        {
            GameObject go = Instantiate(card.gameObject, new Vector3(-100, -100, 1), Quaternion.identity);
            instantiatedCards.Add(go);
            ActionClass ac = go.GetComponent<ActionClass>();
            ActionClass? pref = chosenCardList.FirstOrDefault(action => action.GetType() == card.GetType());
            if (pref != null)
            {
                ac.SetSelectedForDeck(true);
            }
            ac.SetRenderCost(true);
            ac.UpdateDup();
        }

        SaveLoadSystem.Instance.LoadCardEvolutionProgress();
        OnRenderDecks?.Invoke(cols, instantiatedCards.Select(card => card.GetComponent<ActionClass>()).OrderBy(card => card.GetComponent<ActionClass>().Speed).ToList());
    }

    private void UnrenderDecks()
    {
        foreach (Transform child in cardArrayParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyEditParent)
        {
            Destroy(child.gameObject);
        }
    }
}