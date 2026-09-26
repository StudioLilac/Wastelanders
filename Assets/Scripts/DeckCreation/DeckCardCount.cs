using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckCardCount : MonoBehaviour
{
    [SerializeField] private TMP_Text textui;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private Image textimage;
    private DeckSelectionState currentState;

    void Awake()
    {
        this.Subscribe<WeaponDeckModified>(ev => UpdateSelf());
        this.Subscribe<EquipWeaponChanged>(ev => UpdateSelf());
        this.Subscribe<DeckSelectStateChanged>(DeckSelectStateChanged);
    }

    void DeckSelectStateChanged(DeckSelectStateChanged ev)
    {
        currentState = ev.State;
        textui.gameObject.SetActive(ev.State != DeckSelectionState.CharacterSelection);
        warningText.gameObject.SetActive(ev.State != DeckSelectionState.CharacterSelection);
        textimage.gameObject.SetActive(ev.State != DeckSelectionState.CharacterSelection);
        UpdateSelf();
    }

    private void UpdateSelf()
    {
        UpdateCount();
        ResetWarnings();
        CheckSpeedBalance();
        CheckEmptyDecks();
        AutoSavePrompt();
    }

    private void UpdateCount()
    {        
        var deckCards = new GetCurrentDeckCards().Query();
        if (textui != null)
        {
            textui.text = deckCards != null ? deckCards.Count.ToString() : "0";
        }
    }
    private void ResetWarnings() => warningText.text = "";

    void CheckEmptyDecks()
    {
        var playerData = new GetCurrentEditingPlayer().Query();
        if (playerData != null)
        {
            foreach (var weapon in playerData.selectedWeapons)
            {
                var weaponDeck = playerData.GetPlayerWeaponDeck(weapon);
                if (weaponDeck.weaponDeck.Count == 0)
                {
                    string weaponName = weapon.ToString();
                    weaponName = char.ToUpper(weaponName[0]) + weaponName.Substring(1).ToLower();
                    warningText.text = $"{weaponName} deck is empty!";
                    warningText.color = Color.red;
                }
            }
        }
    }

    void CheckSpeedBalance()
    {
        var deck = new GetCurrentDeckCards().Query();
        if (deck == null) return;
        deck = InitializeActions(deck);

        int[] speedCounts = new int[5]; // Assume only 1-5 speed, then form a histogram of speed values in the deck.       
        deck.ForEach(card => speedCounts[card.Speed - 1] += 1);

        int unbalancedCount = 4;
        List<string> results = Enumerable.Range(0, speedCounts.Length)
                            .Select(i => speedCounts[i] >= unbalancedCount ? $"Speed {i + 1} has {speedCounts[i]} Actions. ": "")
                            .Where(msg => msg != "")
                            .ToList();

        if (results.Count != 0)
        {
            warningText.text = $"Warning: Unbalanced Speeds.\n{string.Join("\n", results)}";
            warningText.color = Color.yellow;
        }
    }
    private void AutoSavePrompt()
    {
        if (currentState == DeckSelectionState.DeckSelection)
        {
            warningText.text = "Deck configuration is autosaved.";
            warningText.color = Color.yellow;
        }
    }

    // Instantiate the templates so they run Awake and actually have the correct data within them.
    List<ActionClass> InitializeActions(List<ActionClass> actions) =>
        actions.Select(ac => {
            var dummy = Instantiate(ac);
            dummy.gameObject.SetActive(false);
            return dummy;
        }).ToList();
}
