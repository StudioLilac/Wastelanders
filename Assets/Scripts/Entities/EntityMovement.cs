using System.Collections;
using UnityEngine;

// Handles the movement system for the entity class
[RequireComponent(typeof(EntityClass))]
public class EntityMovementHandler : MonoBehaviour
{
    private Coroutine _movementCoroutine = null;
    private const float PLAY_RUNNING_ANIMATION_DELTA = 0.03f;
    private const string IS_MOVING = "IsMoving";
    public const string STAGGERED_ANIMATION_NAME = "IsStaggered";
    private EntityClass entityClass;

    private void Awake()
    {
        entityClass = GetComponent<EntityClass>();
    }

    private void StopMovingImmediate()
    {
        StopCurrentMovementAnimation();
        entityClass.SetAnimationBool(IS_MOVING, false);
        entityClass.SetAnimationBool(STAGGERED_ANIMATION_NAME, false);
    }

    public virtual IEnumerator MoveToPosition(Vector3 destination, float radius, float duration, Vector3? lookAtPosition = null)
    {
        RequestMove(destination, radius, duration, lookAtPosition);
        yield return new WaitUntil(() => _movementCoroutine == null);
    }

    private void RequestMove(Vector3 destination, float radius, float duration, Vector3? lookAtPosition)
    {
        StopMovingImmediate();
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
            return;
        }

        float maxProportionTravelled = (distance - radius) / distance;

        if (distance > radius + PLAY_RUNNING_ANIMATION_DELTA)
        {
            entityClass.UpdateFacing(diffInLocation, lookAtPosition);
            entityClass.SetAnimationBool(IS_MOVING, true);
        }

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

    public virtual IEnumerator StaggerBack(Vector3 staggeredPosition)
    {
        RequestStagger(staggeredPosition);
        yield return new WaitUntil(() => _movementCoroutine == null);
    }


    private void RequestStagger(Vector3 staggeredPosition)
    {
        StopMovingImmediate();

        Vector3 originalPosition = transform.position;
        Vector3 diffInLocation = staggeredPosition - originalPosition;
        if ((Vector2)diffInLocation == Vector2.zero) return;
        
        entityClass.UpdateFacing(-diffInLocation, null);
        entityClass.SetAnimationBool(STAGGERED_ANIMATION_NAME, true);

        float duration = CardComparator.COMBAT_BUFFER_TIME;
        _movementCoroutine = StartCoroutine(AnimateStagger(originalPosition, staggeredPosition, duration));
    }

    private IEnumerator AnimateStagger(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = GetStaggerCurve(elapsedTime, duration);

            transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        entityClass.SetAnimationBool(STAGGERED_ANIMATION_NAME, false);
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
    private float GetStaggerCurve(float elapsedTime, float duration)
    {
        float speed = 0.7f; //Lower value is faster
        float power = 5f; //Modifies the curvature of the curve
        return (Mathf.Pow(speed, power) / Mathf.Pow(((-elapsedTime) / duration - speed), power) + 1);
    }
}