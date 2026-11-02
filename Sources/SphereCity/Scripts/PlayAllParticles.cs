using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAllParticles : MonoBehaviour
{
    public ParticleSystem[] particles;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        foreach (var dustParticle in particles)
        {
            dustParticle.Play(false);
        }
    }
}
