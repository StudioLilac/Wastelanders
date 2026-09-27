using DialogueScripts;
using LevelSelectInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tutorials;
using UnityEngine;
using UnityEngine.UI;
using WeaponDeckSerialization;

public class DeckSelectionTutorial : MonoBehaviour
{
    private PlayerDatabase playerDatabase => new GetPlayerDatabase().Query();
    [SerializeField] private CharacterSelect jackieSelect;

    [SerializeField] private DialogueWrapper selectYourCharacter;
    [SerializeField] private DialogueWrapper selectYourWeapon;
    [SerializeField] private DialogueWrapper editYourWeapon;
    [SerializeField] private DialogueWrapper selectYourActions;
    [SerializeField] private DialogueWrapper backButtonTutorial;

    [SerializeField] private List<CharacterSelect> lockedCharacters;
    [SerializeField] private List<WeaponSelect> lockedWeapons;

    [SerializeField] private List<WeaponEdit> weaponEditBoxCollidersToDisable;
    [SerializeField] private List<WeaponSelect> weaponSelectBoxCollidersToDisable;

    [SerializeField] private SpriteRenderer playerSelectIndicator;
    [SerializeField] private SpriteRenderer weaponSelectIndicator;
    [SerializeField] private SpriteRenderer editDeckIndicator;
    [SerializeField] private Image backButtonIndicator;
    [SerializeField] private GameObject backButton;

    [SerializeField] private ScreenCutoutScrim screenCutoutScrim;
    [SerializeField] private MaterialTintFadeHandler materialTintFadeHandler;
    [SerializeField] private SpriteRenderer interactionArea;

    [SerializeField] private bool activateTutorial;
    [SerializeField] private bool activateEnemyDeckTutorial;

    public const String DismissInteractionBlocker = "DismissInteractionBlocker";
#nullable enable
    private void Start()
    {
        StartCoroutine(ExecuteGameStart());
    }

    private IEnumerator ExecuteGameStart()
    {
        bool showTutorial =
            SceneData.Get<SceneData.TutorialFight>() == GameStateManager.Instance.PreviousScene &&
            GameStateManager.Instance.CurrentLevelProgress > StageInformation.Get<StageInformation.DeckSelectionTutorial>().LevelID;
        bool showHowToDeckSelectTutorial = Mathf.Approximately(GameStateManager.Instance.CurrentLevelProgress, StageInformation.Get<StageInformation.DeckSelectionTutorial>().LevelID) || showTutorial || activateTutorial;
        bool showEnemyWeaponTutorial = (GameStateManager.Instance.PreviousScene == SceneData.Get<SceneData.Epilogue_3>() || activateEnemyDeckTutorial) 
                                       && GameStateManager.Instance.CurrentLevelProgress >= StageInformation.Get<StageInformation.IvesFinale>().LevelID;

        if (showHowToDeckSelectTutorial)
        {
            backButton.SetActive(false);
            this.Subscribe<DeckSelectStateChanged>(HandleDeckSelectStateChanged);
            NormalizeTutorialDecks();

            foreach (WeaponEdit boxCollider in weaponEditBoxCollidersToDisable)
            {
                boxCollider.GetComponent<BoxCollider2D>().enabled = false;
            }
            jackieSelect.GetComponent<BoxCollider2D>().enabled = false;
            foreach (CharacterSelect character in lockedCharacters)
            {
                character.SetLockedState(true);
            }
            foreach (WeaponSelect weapon in lockedWeapons)
            {
                weapon.SetLockedState(true);
            }
            // Wait for fade screen to come in
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(StartDialogueWithNextEvent(selectYourCharacter, () =>
            {
                playerSelectIndicator.enabled = true;
                jackieSelect.GetComponent<BoxCollider2D>().enabled = true;

                this.Subscribe<CharacterSelected>(HandleCharacterSelected);
            }));
        }
        else if (showEnemyWeaponTutorial)
        {
            this.Subscribe<CustomEvent>(DismissBlocker);
            void DismissBlocker(CustomEvent customEvent)
            {
                this.UnSubscribe<CustomEvent>(DismissBlocker);
                if (customEvent.EventName == DismissInteractionBlocker)
                {
                    interactionArea.gameObject.SetActive(false);
                }
            }

            this.Subscribe<WeaponEditSelected>(DismissScrim);
            void DismissScrim(WeaponEditSelected ev)
            {
                this.UnSubscribe<WeaponEditSelected>(DismissScrim);
                materialTintFadeHandler.SetLightScreen();
            }

            new CharacterSelected(PlayerDatabase.PlayerName.JACKIE).Invoke();
            interactionArea.gameObject.SetActive(true);
            materialTintFadeHandler.SetDarkScreen();
            screenCutoutScrim.SetTarget(new SpriteTarget(interactionArea));
            VerticalLayoutChange.MoveBoxV2ToTop();

            new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
            DeckSelectionManager.Instance.SetNextScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
            yield return materialTintFadeHandler.FadeToAlpha(200f / 255f, 1f);
            yield return DialogueBoxV2.Instance.Play(EnemyActionTutorial.Explanation);
            yield return materialTintFadeHandler.FadeInLightScreen(1f);
        }
    }


    private void HandleDeckSelectStateChanged(DeckSelectStateChanged ev)
    {
        backButton.SetActive(ev.State != DeckSelectionState.CharacterSelection);
    }

    private void HandleCharacterSelected(CharacterSelected cs)
    {
        this.UnSubscribe<CharacterSelected>(HandleCharacterSelected);

        PlayerDatabase.PlayerName playerName = cs.PlayerName;
        playerSelectIndicator.enabled = false;
        weaponSelectBoxCollidersToDisable.ForEach(ws => ws.GetComponent<PolygonCollider2D>().enabled = false);
        StartCoroutine(StartDialogueWithNextEvent(selectYourWeapon, () =>
        {
            weaponSelectIndicator.enabled = true;
            weaponSelectBoxCollidersToDisable.ForEach(ws => ws.GetComponent<PolygonCollider2D>().enabled = true);
            this.Subscribe<WeaponSelectEvent>(HandleWeaponSelected);
        }));
    }

    private void HandleWeaponSelected(WeaponSelectEvent ev)
    {
        (WeaponSelect weaponSelect, CardDatabase.WeaponType type) = ev;
        if (type != CardDatabase.WeaponType.PISTOL) return;
        this.UnSubscribe<WeaponSelectEvent>(HandleWeaponSelected);
        weaponSelectIndicator.enabled = false;

        StartCoroutine(StartDialogueWithNextEvent(editYourWeapon, () => {
            foreach (WeaponEdit boxCollider in weaponEditBoxCollidersToDisable)
            {
                boxCollider.GetComponent<BoxCollider2D>().enabled = true;
            }
            editDeckIndicator.enabled = true;
            this.UnSubscribe<DeckSelectStateChanged>(HandleDeckSelectStateChanged);
            this.Subscribe<WeaponEditSelected>(HandleWeaponEdited);
        }));
    }

    private void HandleWeaponEdited(WeaponEditSelected ev)
    {
        this.UnSubscribe<WeaponEditSelected>(HandleWeaponEdited);
        WeaponEditInformation weaponEditInformation = ev.WeaponEditInformation;
        if (weaponEditInformation.WeaponType != CardDatabase.WeaponType.PISTOL) return;
        editDeckIndicator.enabled = false;
        GameStateManager.Instance.UpdateLevelProgress(StageInformation.Get<StageInformation.FrogSlime>());
        DeckSelectionManager.Instance.SetNextScene(SceneData.Get<SceneData.FrogSlimeFight>().SceneName);
        StartCoroutine(StartDialogueWithNextEvent(selectYourActions, () => this.Subscribe<WeaponDeckModified>(HandleRunOutOfPoints)));
    }
    private void HandleRunOutOfPoints(WeaponDeckModified ev)
    {
        if (ev.AvailablePoints < 2)
        {
            this.UnSubscribe<WeaponDeckModified>(HandleRunOutOfPoints);
            backButtonIndicator.enabled = true;
            StartCoroutine(DialogueBoxV2.Instance.Play(backButtonTutorial));
        }
    }

    //Completely removes the PISTOL weaponDeck from jackie
    private void NormalizeTutorialDecks()
    {
        playerDatabase.JackieData.selectedWeapons.Clear();
        playerDatabase.JackieData.selectedWeapons.Add(CardDatabase.WeaponType.STAFF);
        SerializableWeaponListEntry pistolDeck = playerDatabase.JackieData.GetPlayerWeaponDeck(CardDatabase.WeaponType.PISTOL);
        pistolDeck.weaponDeck = new List<SerializableActionClassInfo>
        {
            new(nameof(IronSights)),
            new(nameof(Silencer))
        };
    }


    //Helper to wait until dialogue is done, then start @param dialogue, then run a callback like setting up a new event. 
    private IEnumerator StartDialogueWithNextEvent(DialogueWrapper dialogue, Action callbackToRun)
    {
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(dialogue));
        callbackToRun();
    }

}
