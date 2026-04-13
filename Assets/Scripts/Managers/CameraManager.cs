using Cinemachine;
using System;
using UnityEngine;


#nullable enable
public record SetCameraCenter(EntityClass Entity) : IEvent;
public record ActivateDynamicCameraEvent() : IEvent;
public record ActivateBaseCameraEvent() : IEvent;
public record GetDynamicCamera() : IQuery<CinemachineVirtualCamera?>;
public record GetBaseCamera() : IQuery<CinemachineVirtualCamera?>;
public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera baseCamera = null!;
    [SerializeField] private CinemachineVirtualCamera dynamicCamera = null!;

    private void Awake()
    {
        this.Answer<GetActiveCamera, CinemachineVirtualCamera?>(_ => (dynamicCamera.Priority > baseCamera.Priority) ? dynamicCamera : baseCamera);
        this.Answer<GetDynamicCamera, CinemachineVirtualCamera?>(_ => dynamicCamera);
        this.Answer<GetBaseCamera, CinemachineVirtualCamera?>(_ => baseCamera);
        this.Subscribe<GameStateChanged>(evt => HandleGameStateChanged(evt.NewState));
        this.Subscribe<EntityFacingChanged>(evt => UpdateCameraBounds(evt.Entity));
        this.Subscribe<SetCameraCenter>(evt => SetCameraCenter(evt.Entity));
        this.Subscribe<RemoveEntityFromTeam>(evt => HandleEntityRemoved(evt.Entity));
        this.Subscribe<ActivateDynamicCameraEvent>(_ => ActivateDynamicCamera());
        this.Subscribe<ActivateBaseCameraEvent>(_ => ActivateBaseCamera());
        gameObject.AddComponent<ScreenShakeHandler>();
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.GAME_WIN:
            case GameState.GAME_LOSE:
            case GameState.SELECTION:
                ActivateBaseCamera();
                break;
            case GameState.FIGHTING:
                ActivateDynamicCamera();
                break;
            default: break;
        }
    }

    private void HandleEntityRemoved(EntityClass entity)
    {
        if (dynamicCamera.Follow == entity.transform)
        {
            dynamicCamera.Follow = null;
        }
    }

    private void SetCameraCenter(EntityClass entity)
    {
        dynamicCamera.Follow = entity.transform;
        UpdateCameraBounds(entity);
    }

    private void UpdateCameraBounds(EntityClass entity)
    {
        if (entity.transform != dynamicCamera.Follow) return;

        if (entity.IsFacingRight())
        {
            var transposer = dynamicCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            transposer.m_ScreenX = 0.25f;
        }
        else if (!entity.IsFacingRight())
        {
            var transposer = dynamicCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            transposer.m_ScreenX = 0.75f;
        }
    }

    private void ActivateDynamicCamera()
    {
        baseCamera.Priority = 0;
        dynamicCamera.Priority = 1;
    }

    private void ActivateBaseCamera()
    {
        baseCamera.Priority = 1;
        dynamicCamera.Priority = 0;
    }
}
