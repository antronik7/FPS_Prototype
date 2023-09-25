using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [SerializeField] private float xSpawnRange;
    [SerializeField] private float ySpawnRange;
    [SerializeField] private float zSpawnRangeMin;
    [SerializeField] private float zSpawnRangeMax;
    [SerializeField] private Vector3 hitRotation;
    [SerializeField] private float hitRotationSpeed;

    private bool isHit = false;

    void Awake()
    {
        transform.position = new Vector3(Random.Range(xSpawnRange * -1f, xSpawnRange), Random.Range(0f, ySpawnRange), Random.Range(zSpawnRangeMin, zSpawnRangeMax));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isHit == false)
            return;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(hitRotation), hitRotationSpeed * Time.deltaTime);
    }

    public void Hit()
    {
        isHit = true;
    }
}
