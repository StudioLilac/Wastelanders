using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
#nullable enable
    public delegate void OnHitDelegate();

    public float projSpeed = 10f;
    [SerializeField]
    private GameObject? projectilePrefab;

    public IEnumerator ProjectileAnimation(Vector3 origin, Vector3 targetPosition)
    {
        return StartProjectileAnimationWithPosition(origin, targetPosition);
    }

    private IEnumerator StartProjectileAnimationWithPosition(Vector3 origin, Vector3 targetPosition)
    {
        if (projectilePrefab == null) yield break;
        Vector3 originalPosition = origin;
        float elapsedTime = 0f;

        Vector3 diffInLocation = targetPosition - originalPosition;

        if ((Vector2)diffInLocation == Vector2.zero) yield break;

        float distance = Mathf.Sqrt(diffInLocation.x * diffInLocation.x + diffInLocation.y * diffInLocation.y);

        //UpdateFacing(diffInLocation, destination);
        GameObject spitProjectile = Instantiate(projectilePrefab, originalPosition, UpdateAngleWithPosition(origin, targetPosition));
        SpriteRenderer spitSpriteRenderer = spitProjectile.GetComponent<SpriteRenderer>();
        spitSpriteRenderer.sortingOrder = new GetFadeSortingOrder().Query() ?? 0;
        spitSpriteRenderer.sortingLayerName = new GetFadeSortingLayer().Query() ?? string.Empty;
        float flipTimer = 0.0f;
        float flipInterval = 0.2f;
        float spitDuration = distance / projSpeed;

        while (elapsedTime < spitDuration)
        {
            spitProjectile.GetComponent<Transform>().position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / spitDuration);
            elapsedTime += Time.deltaTime;
            flipTimer += Time.deltaTime;

            if (flipTimer >= flipInterval)
            {
                spitSpriteRenderer.flipY = !spitSpriteRenderer.flipY;
                flipTimer = 0.0f;
            }
            yield return null;
        }

        Destroy(spitProjectile);
    }

    private Quaternion UpdateAngleWithPosition(Vector3 origin, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - origin;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
