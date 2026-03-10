using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract record PopupType
{
    public sealed record SpeedConflict(IBattleQueueDisplayable DuplicatedItem) : PopupType;
    public sealed record SelectActionFirst : PopupType;
    public sealed record SelectPlayerFirst : PopupType;
    public sealed record InsufficientResources(int Current, int Required) : PopupType;
    public sealed record ContentLocked : PopupType;
    public sealed record CustomPopup(string Content) : PopupType;
    public sealed record None : PopupType;
}