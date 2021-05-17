using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballFXBehaviour : MonoBehaviour
{
    public float Speed = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void End()
    {
        // TODO: FX sizzle?
        Destroy(gameObject);
    }

}
