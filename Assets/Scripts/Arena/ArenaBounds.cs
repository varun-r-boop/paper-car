using UnityEngine;

namespace PaperCar
{
    public class ArenaBounds : MonoBehaviour
    {
        [SerializeField] Vector2 halfExtents = new Vector2(5f, 5f);

        public Vector2 HalfExtents
        {
            get => halfExtents;
            set => halfExtents = value;
        }

        public bool Contains(Vector3 worldPosition)
        {
            Vector3 local = worldPosition - transform.position;
            return Mathf.Abs(local.x) <= halfExtents.x && Mathf.Abs(local.z) <= halfExtents.y;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.35f);
            Vector3 size = new Vector3(halfExtents.x * 2f, 0.05f, halfExtents.y * 2f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.05f, size);
        }
    }
}
