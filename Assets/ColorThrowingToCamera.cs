using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorThrowingToCamera : MonoBehaviour
{
    public Color[] randomColors;
    private ParticleSystem pSystem;
    // Start is called before the first frame update
    void Start()
    {
        pSystem = GetComponent<ParticleSystem>();
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        pSystem.startColor = randomColors[Random.Range(0, randomColors.Length)];
    }
}
