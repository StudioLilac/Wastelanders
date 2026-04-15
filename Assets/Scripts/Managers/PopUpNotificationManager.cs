
using UnityEngine;

public record DisplayWarning(PopupType PopupType): IEvent;
public class PopUpNotificationManager : MonoBehaviour
{
    public static PopUpNotificationManager Instance { get; private set; }

    [SerializeField] private PopUpNotification floatingNotificationPrefab;
    [SerializeField] private Canvas canvas;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        canvas.sortingOrder = UISortOrder.WarningPopup.GetOrder();
        this.Subscribe<DisplayWarning>(ev => DisplayWarning(ev.PopupType));
    }

    public void DisplayWarning(PopupType popupType)
    {
        string HandleConflict(PopupType.SpeedConflict data)
        {
            data.DuplicatedItem.ShakePlayerAction();
            return $"{data.DuplicatedItem.Name} already queued at this speed!";
        }

        string message = popupType switch
        {
            PopupType.SpeedConflict data => HandleConflict(data),
            PopupType.SelectActionFirst => "Select an Action first!",
            PopupType.SelectPlayerFirst => "Select a Player first!",
            PopupType.InsufficientResources => "Insufficient resources!",
            PopupType.CustomPopup data => data.Content,
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(message))
        {
            CreateFloatingWarning(message);
        }
    }

    private void CreateFloatingWarning(string message)
    {
        PopUpNotification notification = Instantiate(floatingNotificationPrefab, canvas.transform);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        notification.Initialize(localPoint, message);
    }

}