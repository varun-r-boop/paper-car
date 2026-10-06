using UnityEngine;

namespace PaperCar
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [SerializeField] float maxImpulse = 14f;
        [SerializeField] float maxSpeed = 18f;
        [SerializeField] Rigidbody body;

        Vector3 _spawnPosition;
        Quaternion _spawnRotation;

        public Rigidbody Body => body;

        void Awake()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();

            body.constraints =
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        public void ApplyThrust(Vector3 direction, float intensity)
        {
            Vector3 flat = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (flat.sqrMagnitude < 0.0001f)
                return;

            float impulse = Mathf.Clamp01(intensity) * maxImpulse;
            body.AddForce(flat.normalized * impulse, ForceMode.Impulse);
        }

        void FixedUpdate()
        {
            Vector3 v = GetLinearVelocity();
            v.y = 0f;
            if (v.magnitude > maxSpeed)
                SetLinearVelocity(v.normalized * maxSpeed);
            else
                SetLinearVelocity(v);
        }

        public bool IsSettled(float speedThreshold)
        {
            Vector3 v = GetLinearVelocity();
            v.y = 0f;
            return v.magnitude < speedThreshold && body.angularVelocity.magnitude < speedThreshold * 2f;
        }

        public void Freeze()
        {
            SetLinearVelocity(Vector3.zero);
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }

        public void ResetForMatch()
        {
            body.isKinematic = false;
            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            SetLinearVelocity(Vector3.zero);
            body.angularVelocity = Vector3.zero;
        }

        Vector3 GetLinearVelocity()
        {
#if UNITY_6000_0_OR_NEWER
            return body.linearVelocity;
#else
            return body.velocity;
#endif
        }

        void SetLinearVelocity(Vector3 velocity)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = velocity;
#else
            body.velocity = velocity;
#endif
        }
    }
}
