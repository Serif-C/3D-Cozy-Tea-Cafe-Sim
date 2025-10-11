using UnityEngine;

public class SeatingManager : MonoBehaviour
{
    [SerializeField] private TransformTarget[] seats;
    
    public ITarget AssignSeat()
    {
        int random = UnityEngine.Random.Range(0, seats.Length);
        return seats[random];
    }
}
