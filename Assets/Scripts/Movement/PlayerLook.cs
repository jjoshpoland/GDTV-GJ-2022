using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        public PlayerControllerMovement movement;
        public Player player;
        public GameObject lookTarget;

        [Header("Settings")]
        public float lookTargetOffsetY = 1f;

        [Header("Camera")]
        // the layer mask to use when trying to detect view blocking
        // (this way we dont zoom in all the way when standing in another entity)
        // (-> create a entity layer for them if needed)
        public LayerMask viewBlockingLayers;

        private Camera cam;

        [Header("Physical Interaction")]
        [Tooltip("Layers to use for raycasting. Check Default, Walls, Player, Zombie, Doors, Interactables, Item, etc. Uncheck IgnoreRaycast, AggroArea, Water, UI, etc.")]
        public LayerMask raycastLayers = Physics.DefaultRaycastLayers;

        public Vector3 lookDirectionFar
        {
            get
            {
                return cam.transform.forward;
            }
        }

        public Vector3 lookDirectionRaycasted
        {
            get
            {
                // same for local and other players
                // (positionRaycasted uses camera || syncedDirectionRaycasted anyway)
                return (lookPositionRaycasted - transform.position).normalized;
            }
        }

        // the far position, directionFar projected into nirvana
        public Vector3 lookPositionFar
        {
            get
            {
                Vector3 position = cam.transform.position;
                return position + lookDirectionFar * 9999f;
            }
        }

        // the raycasted position is needed for lookDirectionRaycasted calculation
        // and for firing, so we might as well reuse it here
        public Vector3 lookPositionRaycasted
        {
            get
            {
                // raycast based on position and direction, project into nirvana if nothing hit
                // (not * infinity because might overflow depending on position)
                RaycastHit hit;
                return Utils.RaycastWithout(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, gameObject, raycastLayers)
                       ? hit.point
                       : lookPositionFar;
            }
        }

        void Awake()
        {
            cam = Camera.main;
        }

        void FixedUpdate()
        {
            if (!player.IsAlive)
            {
                // hide the look target if not alive
                lookTarget.SetActive(false);

                return;
            }

            // rotate to look towards cursor position
            Vector3 mousePoint = Mouse.current.position.ReadValue();

            var mouseRay = cam.ScreenPointToRay(mousePoint);

            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, 500, viewBlockingLayers))
            {
                var mouseWorld = cam.ScreenToViewportPoint(mousePoint);
                mouseWorld.z = transform.position.z;
                mouseWorld.y = transform.position.y;

                var hitPos = hit.point
                                .ChangeY(transform.position.y + lookTargetOffsetY);

                lookTarget.transform.position = hitPos;
            }
            else
            {
                lookTarget.transform.position = lookTarget.transform.position.ChangeY(transform.position.y + lookTargetOffsetY); //player.transform.position.ChangeZ(player.transform.position.z + 5);
            }

            Vector3 targetPosition = lookTarget.transform.position.ChangeY(transform.position.y);
            player.transform.LookAt(targetPosition);
        }

        // debugging ///////////////////////////////////////////////////////////////
        void OnDrawGizmos()
        {
            if (cam == null) return;

            // draw camera forward
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, cam.transform.position + cam.transform.forward * 9999f);

            // draw all the different look positions
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, lookPositionFar);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, lookPositionRaycasted);
        }
    }
}