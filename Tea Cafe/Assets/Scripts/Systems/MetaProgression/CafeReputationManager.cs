using UnityEngine;

public class CafeReputationManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;

    public void Register(CustomerBrain brain)
    {
        brain.ReputationImpact += OnReputationImpact;
    }

    public void Unregister(CustomerBrain brain)
    {
        brain.ReputationImpact -= OnReputationImpact;
    }

    private void OnReputationImpact(int delta)
    {
        progress.cafeReputation.Add(delta);
    }
}
