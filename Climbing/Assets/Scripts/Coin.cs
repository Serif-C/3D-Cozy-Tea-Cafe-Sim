using UnityEngine;

public interface ICoinCollector
{
    public void AddCoinOnWallet(int amount);
}

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinAmount = 10;
    public float bobSpeed = 1f;
    public float bobHeight = 0.1f;
    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Bob and spin
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + Vector3.up * offset;
        transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
    }

    public void SetAmountWithTip(int amount)
    {
        coinAmount += amount;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerWallet wallet = other.gameObject.GetComponent<PlayerWallet>();

        if (wallet != null && other.tag == "Player")
        {
            wallet.AddCoinOnWallet(coinAmount);
            Destroy(gameObject);
        }
    }
}
