using UnityEngine;

namespace Utils {
    public class InfiniteScrollHandler : MonoBehaviour {
        [SerializeField] public SpriteRenderer bg1;
        [SerializeField] public SpriteRenderer bg2;
        [SerializeField] public float speed;

        private Transform mainTransform;
        private Transform queuedTransform;
        private float bgWidth;

        void Start() {
            mainTransform = bg1.transform;
            queuedTransform = bg2.transform;

            bgWidth = bg1.bounds.size.x;

            queuedTransform.position = new Vector2(mainTransform.position.x + bgWidth, mainTransform.position.y);
        }

        void Update() {
            float deltaX = speed * Time.deltaTime;

            bg1.transform.position += Vector3.left * deltaX;
            bg2.transform.position += Vector3.left * deltaX;

            // check if the main tile has fully moved off-screen
            if (mainTransform.position.x <= 0) {
                queuedTransform.position =
                    new Vector2(mainTransform.position.x + bgWidth, queuedTransform.position.y);
                
                // ohhh im straight up deconstructin it (python reference)
                (mainTransform, queuedTransform) = (queuedTransform, mainTransform);
            }
        }
    }
}