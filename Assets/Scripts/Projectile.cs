using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // auto destroy
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Example: destroy on impact
        Destroy(gameObject);
    }
}
