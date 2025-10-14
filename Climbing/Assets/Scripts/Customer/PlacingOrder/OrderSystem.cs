using System.Collections;
using UnityEngine;

public class OrderSystem : MonoBehaviour, IOrderSystem
{
    public IHandleOrder PlaceOrder(GameObject customer)
    {
        var handle = new HandleOrder();

        // Simulate prep time for now
        StartCoroutine(PrepRoutine(handle));
        return handle;
    }

    private IEnumerator PrepRoutine(HandleOrder h)
    {
        // pretend barista making tea
        yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 6f));
        h.MarkReady();
    }

}
