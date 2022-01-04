using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 10.0f;
    public int damage = 1;
    void Update()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
    }
    void OnTriggerEnter(Collider other)
    { 
        PlayerCharacter player = other.GetComponent<PlayerCharacter>();
        if (player != null)
        { // Проверяем, является ли этот другой объект объектом PlayerCharacter.
            Debug.Log("Player hit");
        }
        Destroy(this.gameObject);
    }
}
