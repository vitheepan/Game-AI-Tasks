using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed = 10f;

    void Update()
    {
        transform.position += transform.forward * Speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Bullet hit player!");
        }
        else
        {
            Debug.Log("Bullet hit " + collision.gameObject.name);
        }
        Destroy(gameObject);
    }
}
