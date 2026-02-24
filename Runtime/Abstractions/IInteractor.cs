namespace P3k.PlayerInteractionController.Abstractions
{
   using System.Linq;

   using UnityEngine;

   public interface IInteractor
   {
      GameObject GameObject { get; }

      int UserIndex { get; }

      void AssignUserIndex(int userIndex);
   }
}
