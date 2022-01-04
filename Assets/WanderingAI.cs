using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  WanderingAI  : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    private GameObject _fireball;

    // Start is called before the first frame update

    public float speed = 2.0f;
    public float obstacleRange = 3.0f;
    void Update()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.SphereCast(ray, 0.75f, out hit))
        {
            if (hit.distance < obstacleRange)
            {
                float angle = Random.Range(-110, 110);
                transform.Rotate(0, angle, 0);
            }
        }
        if (Physics.SphereCast(ray, 0.75f, out hit))
        {
            GameObject hitObject = hit.transform.gameObject;
            if (hitObject.GetComponent<PlayerCharacter>())
            {
                if (_fireball == null)
                { // Та же самая логика с пустым игровым объектом, что и в сценарии SceneController.
                    _fireball = Instantiate(fireballPrefab) as GameObject;
                    _fireball.transform.position =
                    transform.TransformPoint(Vector3.forward * 1.5f);
                    _fireball.transform.rotation = transform.rotation;
                }
            }
            else if (hit.distance < obstacleRange)
            {
                float angle = Random.Range(-110, 110);
                transform.Rotate(0, angle, 0);
            }
        }
    }
}
   
