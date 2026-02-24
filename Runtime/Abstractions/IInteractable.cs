namespace P3k.PlayerInteractionController.Abstractions
{
   using System.Linq;

   using UnityEngine;

   /// <summary>
   ///    Represents an interactable object in the game.
   /// </summary>
   public interface IInteractable
   {
      /// <summary>
      ///    Gets the associated GameObject instance that this property represents.
      /// </summary>
      /// <remarks>
      ///    This property provides access to the GameObject, which can be used to manipulate or
      ///    retrieve information about the object in the game environment.
      /// </remarks>
      GameObject GameObject { get; }

      /// <summary>
      ///    Gets the tag associated with the current object, which can be used to store additional data.
      /// </summary>
      /// <remarks>
      ///    The tag can be any string value that provides context or identification for the object.
      ///    It is typically used for categorization or metadata purposes.
      /// </remarks>
      string Tag { get; }

      /// <summary>
      ///    Initiates an interaction process with the specified interactor.
      /// </summary>
      /// <param name="interactor">The interactor initiating the interaction.</param>
      void Interact(IInteractor interactor);
   }
}
