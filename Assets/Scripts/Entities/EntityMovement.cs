using System.Collections;
using UnityEngine;

// Handles the movement system for the entity class
[RequireComponent(typeof(EntityClass))]
public class EntityMovementHandler : MonoBehaviour
{
    private Coroutine _movementCoroutine = null;
    private const float PLAY_RUNNING_ANIMATION_DELTA = 0.03f;
    private const string IS_MOVING = "IsMoving";
    private EntityClass entityClass;

    private void Awake()
    {
        entityClass = GetComponent<EntityClass>();
    }

    private void StopMovingImmediate()
    {
        StopCurrentMovementAnimation();
        entityClass.SetAnimationBool(IS_MOVING, false);
    }

    public virtual IEnumerator MoveToPosition(Vector3 destination, float radius, float duration, Vector3? lookAtPosition = null)
    {
        RequestMove(destination, radius, duration, lookAtPosition);
        yield return new WaitUntil(() => _movementCoroutine == null);
    }
    private void RequestMove(Vector3 destination, float radius, float duration, Vector3? lookAtPosition)
    {
        Vector3 originalPosition = transform.position;
        Vector3 adjustedDestination = new Vector3(
            destination.x,
            destination.y,
            destination.z + entityClass.GetSortingZ()
        );

        Vector3 diffInLocation = adjustedDestination - originalPosition;
        float distance = diffInLocation.magnitude;

        if (Mathf.Approximately(distance, 0f))
        {
            StopMovingImmediate(); 
            return;
        }

        float maxProportionTravelled = (distance - radius) / distance;

        if (distance > radius + PLAY_RUNNING_ANIMATION_DELTA)
        {
            entityClass.UpdateFacing(diffInLocation, lookAtPosition);

            entityClass.SetAnimationBool(IS_MOVING, true);
        }

        StopCurrentMovementAnimation();
        _movementCoroutine = StartCoroutine(AnimateMove(originalPosition, adjustedDestination, duration, maxProportionTravelled));
    }

    private IEnumerator AnimateMove(Vector3 startPos, Vector3 endPos, float duration, float maxProportion)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = (elapsedTime / duration) * maxProportion;
            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        transform.position = Vector3.Lerp(startPos, endPos, maxProportion);

        entityClass.SetAnimationBool(IS_MOVING, false);

        _movementCoroutine = null;
    }

    private void StopCurrentMovementAnimation()
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
    }
}