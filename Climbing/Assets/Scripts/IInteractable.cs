using UnityEngine;

public interface IInteractable
{
    // A short message for UI prompts, like "Pick up herb" or "Brew tea"
    string Prompt { get; }

    bool CanInteract(PlayerInteractor player);

    void Interact(PlayerInteractor player);
}
