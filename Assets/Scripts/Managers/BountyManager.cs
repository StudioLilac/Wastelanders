using UnityEngine;
using System;
using Systems.Persistence;
using System.Collections.Generic;
using System.Linq;
using BountySystem;
using LevelSelectInformation;
using System.Collections;


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


    public int GetBountyProgress() => ContractStateData.GetNumCompletedBounties();
    public bool IsBountyCompleted(IBounties? bounty)
    {
        if (bounty == null) return false;

        return ContractStateData?.IsBountyCompleted(bounty) ?? false;
    }

    public bool NotifyWin()
    {
        if (ActiveBounty != null)
        {
            return ContractStateData?.SetChallengeComplete(ActiveBounty) == true;
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
    public bool SetChallengeComplete(IBounties bounty)
    {
        ChallengeCompletionState? challengeCompletionState = BountyCompletionData.Find(data => data.BountyName == bounty.BountyName);

        if (challengeCompletionState.Completed) return false;

        if (challengeCompletionState == null) BountyCompletionData.Add(new(bounty.BountyName, true));
        else challengeCompletionState.Completed = true;
        return true;
    }

    public bool IsBountyCompleted(IBounties bounty)
    {
        ChallengeCompletionState? challengeCompletionState = BountyCompletionData.Find(data => data.BountyName == bounty.BountyName);

        return challengeCompletionState?.Completed ?? false;
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
        IBounties.MapOnValues(bounty => BountyCompletionData.Add(new ChallengeCompletionState(bounty.BountyName, false)));
    }

    [Serializable]
    public class ChallengeCompletionState
    {
        [field: SerializeField] public string BountyName { get; set; }
        [field: SerializeField] public bool Completed { get; set; }

        public ChallengeCompletionState(string bountyName, bool completed)
        {
            Completed = completed;
            BountyName = bountyName;
        }
    }
}
