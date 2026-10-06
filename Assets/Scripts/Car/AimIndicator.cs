using UnityEngine;

namespace PaperCar
{
    public class AimIndicator : MonoBehaviour
    {
        [SerializeField] float autoRotateSpeed = 90f;
        [SerializeField] float manualRotateSpeed = 120f;
        [SerializeField] Transform arrow;

        public Vector3 CurrentDirection
        {
            get
            {
                Vector3 dir = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;
            }
        }

        public void SetArrow(Transform arrowTransform)
        {
            arrow = arrowTransform;
        }

        public void SetActiveAim(bool active)
        {
            if (arrow != null)
                arrow.gameObject.SetActive(active);
            enabled = active;
        }

        void Update()
        {
            float yaw = autoRotateSpeed;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                yaw = -manualRotateSpeed;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                yaw = manualRotateSpeed;

            transform.Rotate(0f, yaw * Time.deltaTime, 0f, Space.World);
        }
    }
}
