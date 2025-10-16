using System.Collections;
using UnityEngine;


/*
 * Decides how orders are fulfilled:
 * - Customer should walk up the counter
 * - Customer symbols an order (icon of the order over a thinking cloud above their head)
 * - Walk to their table after a couple of seconds
 */
public class OrderSystem : MonoBehaviour, IOrderSystem
{
    public IHandleOrder PlaceOrder(GameObject customer)
    {
        var handle = new HandleOrder();

        StartCoroutine(DescribeOrder(handle));
        return handle;
    }

    private IEnumerator DescribeOrder(HandleOrder h)
    {
        // The time the customer shows their Order
        yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 6f));
        h.MarkReady();
    }

}
