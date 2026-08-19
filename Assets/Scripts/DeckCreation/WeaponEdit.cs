using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static CardDatabase;
using static ISubWeaponType;
using static WeaponEditInformation;

public record WeaponEditSelected(WeaponEditInformation WeaponEditInformation) : IEvent;
public class WeaponEdit : MonoBehaviour
{
    [SerializeField] private Color baseColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    private bool isMouseDown = false;
    public TMP_Text editText;
    private bool isLocked = false;
    private WeaponEditInformation WeaponEditInformation { get; set; }

#nullable enable

    public void InitializeWeaponEdit(WeaponType type, bool hasSubFolders, GetRenderableCards getCards)
    {
        WeaponEditInformation = new WeaponEditInformation(type, hasSubFolders, getCards);
    }

    public void OnMouseDown()
    {
        if (isLocked) return;
        GetComponent<SpriteRenderer>().color = hoverColor;
        isMouseDown = true;
    }

    public void OnMouseUp()
    {
        if (isLocked) return;
        if (isMouseDown)
        {
            GetComponent<SpriteRenderer>().color = baseColor;
            new WeaponEditSelected(WeaponEditInformation).Invoke();
        }
        isMouseDown = false;
    }

    public void OnMouseEnter()
    {
        if (isLocked) return;
        GetComponent<SpriteRenderer>().color = hoverColor;
    }

    public void OnMouseExit()
    {
        if (isLocked) return;
        GetComponent<SpriteRenderer>().color = baseColor;
        isMouseDown = false;
    }

    public void SetText(string text)
    {
        editText.text = text;
    }

    public void SetLocked(bool isLocked)
    {
        this.isLocked = isLocked;
        SetText(isLocked ? "LOCKED" : "EDIT DECK");
    }
}

public delegate List<ActionClass> GetRenderableCards(CardDatabase cardDatabase);
public record WeaponEditInformation(WeaponType WeaponType, bool ShowSubFolders, GetRenderableCards GetCards);

