using BountySystem;
using LevelSelectInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Systems.Persistence;
using UnityEngine;
using static BountyStateData;


public record ClearBounty(): IEvent;

#nullable enable
// A class that persists the current bounty information during level selecting
public class BountyManager : PersistentSingleton<BountyManager>
{
    private BountyStateData? _data;
    private BountyStateData ContractStateData
    {
        get
        {
            if (_data == null) _data = new GetBountyStateData().Query();

            return _data!;
        }
    }

    // All ActiveBounty should be contained within BountyInformation's Bounty Collection
    public IBounties? ActiveBounty { get; private set; } = null;
    public BountyInformation? SelectedBountyInformation { get; private set; } = null;


    protected override void Awake()
    {
        base.Awake();
        if (invalid) return;

        this.Subscribe<BountyInformationEvent>(e => SelectedBountyInformation = e.BountyType);
        this.Subscribe<ClearBounty>(_ => ActiveBounty = null);
        this.Subscribe<BountyOnClickEvent>(OnBountySelected);
    }

    public int GetBountyProgress() => GameStateManager.IS_DEVELOPMENT ? GameStateManager.DEV_MODE_BOUNTIES : ContractStateData.GetNumCompletedBounties();

    public bool IsBountyCompleteWithoutContinues(IBounties? bounty) { 
        return GetBountyCompletionState(bounty) == BountyCompletionState.CompletedWithoutContinue;
    } 

    public BountyCompletionState GetBountyCompletionState(IBounties? bounty)
    {
        if (bounty == null || ContractStateData == null) return BountyCompletionState.Incomplete;

        return ContractStateData.IsBountyCompleted(bounty);
    }

    // Returns true if a challenge was completed.
    public bool NotifyWin(bool usedContinue)
    {
        if (ActiveBounty != null)
        {
            return ContractStateData?.SetChallengeComplete(ActiveBounty, usedContinue) == true;
        }
        return false;
    }

    private void OnBountySelected(BountyOnClickEvent ev)
    {
        ActiveBounty = (ev.Bounty != ActiveBounty) ? ev.Bounty : null;
    }

}

// The serialized data for bounties that gets stored in the JSON
[System.Serializable]
public class BountyStateData
{
    [field: SerializeField] private List<ChallengeCompletionState> BountyCompletionData { get; set; } = new();
    
    // If challenge completed already, return false. Newly completed challenge returns true.
    public bool SetChallengeComplete(IBounties bounty, bool usedContinue)
    {
        ChallengeCompletionState? challengeCompletionState = BountyCompletionData.Find(data => data.BountyName == bounty.BountyName);

        Debug.Log($"Setting the challenge state for {bounty} and usedContinue: {usedContinue}");
        if (challengeCompletionState == null)
        {
            BountyCompletionData.Add(new(bounty.BountyName, true, usedContinue));
            return true;
        }

        if (challengeCompletionState.Completed)
        {
            if (!usedContinue)
            {
                challengeCompletionState.UsedContinue = usedContinue;
            }
            return false;
        }

        challengeCompletionState.Completed = true;
        challengeCompletionState.UsedContinue = usedContinue;
        return true;
    }

    public BountyCompletionState IsBountyCompleted(IBounties bounty)
    {
        ChallengeCompletionState? challengeCompletionState = BountyCompletionData.Find(data => data.BountyName == bounty.BountyName);

        return challengeCompletionState switch
        {
            { Completed: true, UsedContinue: true } => BountyCompletionState.CompletedWithContinue,
            { Completed: true } => BountyCompletionState.CompletedWithoutContinue,
            _ => BountyCompletionState.Incomplete,
        };
    }

    public int GetNumCompletedBounties()
    {
        return BountyCompletionData.Count(data => data.Completed);
    }

    public BountyStateData()
    {
        Initialize();
    }

    private void Initialize()
    {
        BountyCompletionData.Clear();
        IBounties.MapOnValues(bounty => BountyCompletionData.Add(new ChallengeCompletionState(bounty.BountyName, false, false)));
    }

    [Serializable]
    public class ChallengeCompletionState
    {
        [field: SerializeField] public string BountyName { get; set; }
        [field: SerializeField] public bool Completed { get; set; }
        [field: SerializeField] public bool UsedContinue { get; set; }

        public ChallengeCompletionState(string bountyName, bool completed, bool usedContinue)
        {
            Completed = completed;
            BountyName = bountyName;
            UsedContinue = usedContinue;
        }
    }
}
public enum BountyCompletionState
{
    Incomplete,
    CompletedWithContinue,
    CompletedWithoutContinue,
}