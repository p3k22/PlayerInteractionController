namespace P3k.PlayerInteractionController.Adapters
{
   using P3k.PlayerInteractionController.Abstractions;

   using System;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.Events;
   using UnityEngine.InputSystem;

   /// <summary>
   ///    Performs raycast-based interaction checks and dispatches events for matched input tags.
   /// </summary>
   public class PlayerInteractionController : MonoBehaviour, IInteractor
   {
      private readonly IInteractable[] _interactableHits = new IInteractable[16];

      private readonly RaycastHit[] _raycastHits = new RaycastHit[16];

      [SerializeField]
      [Tooltip("Camera used as the interaction ray origin and direction.")]
      private Camera _cameraSource;

      [SerializeField]
      [Tooltip("Maximum distance for interactable raycasts.")]
      private float _interactableDistance = 5f;

      [SerializeField]
      [Tooltip("Cooldown in seconds between interactions.")]
      private float _interactCooldown = 0.2f;

      private float _nextInteractTime;

      private IInteractable _interactableHit;

      [SerializeField]
      [Tooltip("Mappings between interaction tags, input actions, and events.")]
      private List<InteractionTagMapping> _interactionTagsMap = new();

      [field: SerializeField]
      public bool IsInteractingAllowed { get; private set; } = true;

      public GameObject GameObject => gameObject;

      /// <summary>
      ///    Gets the currently targeted interactable.
      /// </summary>
      public IInteractable InteractableHit
      {
         get => _interactableHit;
         private set
         {
            var old = _interactableHit;
            if (old == value)
            {
               return;
            }

            _interactableHit = value;

            if (old == null && value != null)
            {
               var matched = false;
               foreach (var mapping in _interactionTagsMap.Where(mapping => mapping.Tag == value.Tag))
               {
                  matched = true;
                  mapping.OnInteractableHit?.Invoke();
               }

               if (!matched)
               {
                  _interactionTagsMap.FirstOrDefault()?.OnInteractableHit?.Invoke();
               }
            }
            else if (old != null && value == null)
            {
               var matched = false;
               foreach (var mapping in _interactionTagsMap.Where(mapping => mapping.Tag == old.Tag))
               {
                  matched = true;
                  mapping.OnInteractableLost?.Invoke();
               }

               if (!matched)
               {
                  _interactionTagsMap.FirstOrDefault()?.OnInteractableLost?.Invoke();
               }
            }
         }
      }

      public int UserIndex { get; private set; }

      private void Update()
      {
         if (!IsInteractingAllowed)
         {
            InteractableHit = null;
            return;
         }

         if (!_cameraSource)
         {
            Debug.LogError("Camera source is not assigned.");
            return;
         }

         if (_interactionTagsMap == null || _interactionTagsMap.Count == 0)
         {
            Debug.LogError("Interaction Tags Map is empty Assign some Tags.");
            return;
         }

         var origin = _cameraSource.transform.position;
         var direction = _cameraSource.transform.forward;
         var ray = new Ray(origin, direction);
         var hitCount = GetInteractableHits(ray, out var interactables, out var closestIndex);
         InteractableHit = hitCount > 0 ? interactables[closestIndex] : null;

         if (hitCount > 0 && Time.time >= _nextInteractTime)
         {
            var interactable = interactables[closestIndex];
            var mapping = _interactionTagsMap.Find(m => m.Tag == interactable.Tag)
                          ?? _interactionTagsMap.FirstOrDefault();
            if (mapping == null)
            {
               return;
            }

            if (mapping.Button.action.WasPressedThisFrame())
            {
               interactable.Interact(this);
               mapping.OnInteracted?.Invoke();
               _nextInteractTime = Time.time + _interactCooldown;
            }
         }
      }

      private void OnDrawGizmos()
      {
         if (_cameraSource == null)
         {
            if (Application.isPlaying)
            {
               throw new InvalidOperationException("Camera source is not assigned.");
            }

            return;
         }

         var origin = _cameraSource.transform.position;
         var end = origin + (_cameraSource.transform.forward * _interactableDistance);
         Gizmos.color = Color.cyan;
         Gizmos.DrawLine(origin, end);
         Gizmos.DrawSphere(end, 0.05f);
      }

      public void AllowInteractions(bool allow)
      {
         IsInteractingAllowed = allow;
      }

      public void AssignUserIndex(int userIndex)
      {
         UserIndex = userIndex;
      }

      private int GetInteractableHits(Ray ray, out IInteractable[] results, out int closestIndex)
      {
         var hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, _interactableDistance);
         var interactableCount = 0;
         var closestDistance = float.MaxValue;
         closestIndex = -1;

         for (var i = 0; i < hitCount; i++)
         {
            var hitCollider = _raycastHits[i].collider;
            if (!hitCollider)
            {
               continue;
            }

            var interactable = hitCollider ? hitCollider.GetComponent<IInteractable>() : null;
            if (interactable == null)
            {
               continue;
            }

            if (interactableCount >= _interactableHits.Length)
            {
               break;
            }

            _interactableHits[interactableCount] = interactable;
            if (_raycastHits[i].distance < closestDistance)
            {
               closestDistance = _raycastHits[i].distance;
               closestIndex = interactableCount;
            }

            interactableCount++;
         }

         results = _interactableHits;
         return interactableCount;
      }

      /// <summary>
      ///    Maps interaction tags to input actions and callbacks.
      /// </summary>
      [Serializable]
      internal class InteractionTagMapping
      {
         [SerializeField]
         [Tooltip("Input action used to trigger interactions for this tag.")]
         internal InputActionReference Button;

         [SerializeField]
         [Tooltip("Interaction tag matched against interactables.")]
         internal string Tag = "Click";

         [SerializeField]
         [Tooltip("Invoked when an interactable with this tag is targeted.")]
         internal UnityEvent OnInteractableHit;

         [SerializeField]
         [Tooltip("Invoked when an interactable with this tag is no longer targeted.")]
         internal UnityEvent OnInteractableLost;

         [SerializeField]
         [Tooltip("Invoked after a successful interaction for this tag.")]
         internal UnityEvent OnInteracted;
      }
   }
}