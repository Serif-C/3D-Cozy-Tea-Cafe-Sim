using UnityEngine;

public interface IInteractable
{
    string Prompt { get; }

    bool CanInteract(PlayerInteractor player);

    void Interact(PlayerInteractor player);
}
