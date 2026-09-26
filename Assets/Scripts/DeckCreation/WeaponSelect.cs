using TMPro;
using UnityEngine;

public record WeaponSelectEvent(WeaponSelect WeaponSelect, CardDatabase.WeaponType Type) : IEvent;
public class WeaponSelect : MonoBehaviour
{
    public CardDatabase.WeaponType type;
    [SerializeField] private bool hasSubFolders;
    [SerializeField] private GameObject checkmark;
    [SerializeField] private WeaponEdit weaponEdit;
    [SerializeField] private SpriteRenderer cardBodySprite;
    [SerializeField] private GameObject lockedIndicator;
    [SerializeField] private GameObject hoverIndicator;
    [SerializeField] private TMP_Text hoverIndicatorText;
    private Color baseColor = Color.white;
    private Color hoverColor = new Color(0.6f, 0.6f, 0.6f);

#nullable enable
    public delegate void WeaponSelectDelegate(WeaponSelect weaponSelect, CardDatabase.WeaponType type);
    public static event WeaponSelectDelegate? WeaponSelectEvent;

    private bool isMouseDown = false;
    private bool isLocked = false;

    private void Start()
    {
        weaponEdit.InitializeWeaponEdit(
            type,
            hasSubFolders,
            db => hasSubFolders ? db.GetDefaultSubFolderData(type) : db.GetCardsByType(type)
        );

        // Let's lock the deck if: we have subfolders and there is not a single unlocked weapon. But do not touch locked state if not. (for tutorial)
        if (hasSubFolders && CardDatabase.GetUnlockedSubFoldersFor(type).Count < 1) SetLockedState(true);
    }


    public void SetSelected(bool isSelected)
    {
        checkmark.SetActive(isSelected);
        hoverIndicatorText.SetText(isSelected ? "Deselect" : "Select");
    }

    public void OnMouseDown()
    {
        if (isLocked) return;
        SetColor(baseColor);
        isMouseDown = true;
    }

    public void OnMouseUp()
    {
        if (isLocked) return;
        if (isMouseDown)
        {
            SetColor(baseColor);
            new WeaponSelectEvent(this, type).Invoke();
        }
        isMouseDown = false;
    }

    public void OnMouseEnter()
    {
        if (isLocked) return;
        SetColor(hoverColor);
        hoverIndicator.SetActive(true);
    }

    public void OnMouseExit()
    {
        if (isLocked) return;
        SetColor(baseColor);
        isMouseDown = false;
        hoverIndicator.SetActive(false);
    }

    public void SetColor(Color newColor)
    {
        cardBodySprite.color = newColor;
    }

    public void SetLockedState(bool isLocked)
    {
        this.isLocked = isLocked;
        weaponEdit.SetLocked(isLocked);
        lockedIndicator.SetActive(isLocked);
    }

}
