using LevelSelectInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaponDeckSerialization;

public class DeckSelectionTutorial : MonoBehaviour
{
    [SerializeField] private PlayerDatabase playerDatabase;
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

    [SerializeField] private bool activateTutorial;

#nullable enable
    private void Start()
    {
        StartCoroutine(ExecuteGameStart());
    }

    private void OnDestroy()
    {
        WeaponSelect.WeaponSelectEvent -= HandleWeaponSelected;
        CharacterSelect.CharacterSelectedEvent -= HandleCharacterSelected;
        WeaponEdit.WeaponEditEvent -= HandleWeaponEdited;
        DeckSelectionManager.Instance.PlayerActionDeckModifiedEvent -= HandleRunOutOfPoints;
    }

    private IEnumerator ExecuteGameStart()
    {
        if (Mathf.Approximately(GameStateManager.Instance.CurrentLevelProgress, StageInformation.QUEEN_PREPARATION_STAGE.LevelID))
        {
            GameStateManager.Instance.UpdateLevelProgress(StageInformation.QUEEN_BEETLE_STAGE);
            DeckSelectionManager.Instance.SetNextScene(SceneData.Get<SceneData.PreQueenFight>().SceneName);
            yield break;
        }


        bool showTutorial =
            SceneData.Get<SceneData.TutorialFight>() == GameStateManager.Instance.PreviousScene &&
            GameStateManager.Instance.CurrentLevelProgress > StageInformation.DECK_SELECTION_TUTORIAL.LevelID;

        if (Mathf.Approximately(GameStateManager.Instance.CurrentLevelProgress, StageInformation.DECK_SELECTION_TUTORIAL.LevelID) || showTutorial || activateTutorial)
        {
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
            yield return StartCoroutine(StartDialogueWithNextEvent(selectYourCharacter.Dialogue, () => { jackieSelect.GetComponent<BoxCollider2D>().enabled = true; CharacterSelect.CharacterSelectedEvent += HandleCharacterSelected; }));
        }
    }

    private void HandleCharacterSelected(PlayerDatabase.PlayerName playerName)
    {
        CharacterSelect.CharacterSelectedEvent -= HandleCharacterSelected;
        weaponSelectBoxCollidersToDisable.ForEach(ws => ws.GetComponent<PolygonCollider2D>().enabled = false);
        StartCoroutine(StartDialogueWithNextEvent(selectYourWeapon.Dialogue, () =>
        {
            weaponSelectBoxCollidersToDisable.ForEach(ws => ws.GetComponent<PolygonCollider2D>().enabled = true);
            WeaponSelect.WeaponSelectEvent += HandleWeaponSelected;
        }));
    }

    private void HandleWeaponSelected(WeaponSelect weaponSelect, CardDatabase.WeaponType type)
    {
        if (type != CardDatabase.WeaponType.PISTOL) return;
        WeaponSelect.WeaponSelectEvent -= HandleWeaponSelected;
        StartCoroutine(StartDialogueWithNextEvent(editYourWeapon.Dialogue, () => {
            foreach (WeaponEdit boxCollider in weaponEditBoxCollidersToDisable)
            {
                boxCollider.GetComponent<BoxCollider2D>().enabled = true;
            }
            WeaponEdit.WeaponEditEvent += HandleWeaponEdited; }));
    }

    private void HandleWeaponEdited(WeaponEditInformation weaponEditInformation)
    {
        if (weaponEditInformation.WeaponType != CardDatabase.WeaponType.PISTOL) return;
        WeaponEdit.WeaponEditEvent -= HandleWeaponEdited;
        GameStateManager.Instance.UpdateLevelProgress(StageInformation.FROG_SLIME_STAGE);
        DeckSelectionManager.Instance.SetNextScene(SceneData.Get<SceneData.FrogSlimeFight>().SceneName);
        StartCoroutine(StartDialogueWithNextEvent(selectYourActions.Dialogue, () => { DeckSelectionManager.Instance.PlayerActionDeckModifiedEvent += HandleRunOutOfPoints; }));
    }
    private void HandleRunOutOfPoints(int points)
    {
        if (points < 2)
        {
            DeckSelectionManager.Instance.PlayerActionDeckModifiedEvent -= HandleRunOutOfPoints;
            StartCoroutine(DialogueManager.Instance.StartDialogue(backButtonTutorial.Dialogue));
        }
    }

    //Completely removes the PISTOL weaponDeck from jackie
    private void NormalizeTutorialDecks()
    {
        playerDatabase.JackieData.selectedWeapons.Remove(CardDatabase.WeaponType.PISTOL);
        SerializableWeaponListEntry pistolDeck = playerDatabase.JackieData.GetPlayerWeaponDeck(CardDatabase.WeaponType.PISTOL);
        pistolDeck.weaponDeck = new List<SerializableActionClassInfo>
        {
            new(nameof(IronSights)),
            new(nameof(Silencer))
        };
        WeaponProficiency pointsAvailableForPistol = playerDatabase.JackieData.GetProficiencyPointsTuple(CardDatabase.WeaponType.PISTOL);
        pointsAvailableForPistol.CurrentPoints = 4;
    }


    //Helper to wait until dialogue is done, then start @param dialogue, then run a callback like setting up a new event. 
    private IEnumerator StartDialogueWithNextEvent(List<DialogueText> dialogue, Action callbackToRun)
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(dialogue));
        callbackToRun();
    }

}
