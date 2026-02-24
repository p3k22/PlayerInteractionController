namespace P3k.PlayerInteractionController.Adapters
{
   using System;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UI;

   /// <summary>
   ///    Controls activation and animation of reticle UI elements by tag.
   /// </summary>
   public class ReticleController : MonoBehaviour
   {
      private static int _animate;

      [SerializeField]
      [Tooltip("Whether reticles should be enabled on start.")]
      private bool _allowReticles = true;

      [SerializeField]
      private List<ReticleReference> _reticleReferences;

      [SerializeField]
      [Tooltip("If the reticle image has an Animator, this trigger will be fired to animate it.")]
      private string _defaultReticleAnimatorTriggerName = "Animate";

      /// <summary>
      ///    Initializes animator trigger hashes and deactivates all reticles.
      /// </summary>
      private void Awake()
      {
         _animate = Animator.StringToHash(_defaultReticleAnimatorTriggerName);
         AllowReticles(_allowReticles);
         DeactivateReticles();
      }

      /// <summary>
      ///    Activates the reticle matching the provided tag and deactivates the rest.
      /// </summary>
      /// <param name="tag">The reticle tag to activate.</param>
      public void ActivateReticle(string tag)
      {
         if (!_allowReticles)
         {
            return;
         }

         foreach (var reference in _reticleReferences)
         {
            reference.Reticle.enabled = reference.Name == tag;
         }
      }

      /// <summary>
      ///    Allows or disallows reticles from being activated.
      /// </summary>
      /// <param name="allow"></param>
      public void AllowReticles(bool allow = true)
      {
         _allowReticles = allow;
         if (!allow)
         {
            DeactivateReticles();
         }
      }

      /// <summary>
      ///    Triggers the animation for the reticle matching the provided tag.
      /// </summary>
      /// <param name="tag">The reticle tag to animate.</param>
      public void AnimateReticle(string tag)
      {
         if (!_allowReticles)
         {
            return;
         }

         var reference = _reticleReferences.FirstOrDefault(reference => reference.Name == tag);
         if (reference == null || !reference.Reticle)
         {
            return;
         }

         var animator = reference.Animator;
         if (animator == null)
         {
            return;
         }

         animator.SetTrigger(_animate);
      }

      /// <summary>
      ///    Deactivates all reticles.
      /// </summary>
      public void DeactivateReticles()
      {
         foreach (var reference in _reticleReferences)
         {
            reference.Reticle.enabled = false;
         }
      }
   }

   /// <summary>
   ///    Stores references to a reticle image and its identifying name.
   /// </summary>
   [Serializable]
   internal sealed class ReticleReference
   {
      [SerializeField]
      internal Image Reticle;

      [SerializeField]
      internal string Name;

      private Animator _animator;

      internal Animator Animator => _animator == null ? _animator = Reticle.GetComponent<Animator>() : _animator;
   }
}
