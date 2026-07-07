using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Linq;

public record CharachterSelected(PlayerDatabase.PlayerName PlayerName) : IEvent;
public class CharacterSelect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer characterPortrait;
    [SerializeField] private Animator characterAnimation;
    [SerializeField] private GameObject hover;
    [SerializeField] private GameObject lockIndicator;
    public PlayerDatabase.PlayerName playerName;
    private bool isMouseDown = false;
#nullable enable

    private bool isLocked = false;

    public void OnMouseDown()
    {
        if (isLocked) return;
        isMouseDown = true;
    }

    public void OnMouseUp()
    {
        if (isLocked) return;
        if (isMouseDown)
        {
            SetUnHovered();
            new CharachterSelected(playerName).Invoke();
        }
        isMouseDown = false;
    }

    private void SetHovered()
    {
        hover.SetActive(true);
        SetSpriteTransparency(characterPortrait, 1f);
        characterAnimation.SetBool("IsHovered", true);
    }

    private void SetUnHovered()
    {
        isMouseDown = false;
        hover.SetActive(false);
        SetSpriteTransparency(characterPortrait, 0.75f);
        characterAnimation.SetBool("IsHovered", false);
    }

    public void OnMouseEnter()
    {
        if (isLocked) return;
        SetHovered();
    }

    public void OnMouseExit()
    {
        if (isLocked) return;
        SetUnHovered();
    }

    public void SetSpriteTransparency(SpriteRenderer r, float newTransparency)
    {
        Color c = r.color;
        c.a = newTransparency;
        r.color = c;
    }

    public void SetLockedState(bool isLocked)
    {
        this.isLocked = isLocked;
        lockIndicator.SetActive(isLocked);
    }

}
