using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaixaTermica : MonoBehaviour
{
    public AmbientController ambient;
    private Vector3 initScale;
    // Start is called before the first frame update
    void Start()
    {
        ambient = GameObject.FindGameObjectWithTag("AmbientController").GetComponent<AmbientController>();
        initScale = transform.localScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Movimento padrão das caixas termicas ao serem montadas

        if (transform.position.z > 1.5)
        {
            this.GetComponent<Rigidbody>().MovePosition(transform.position + (new Vector3(0, 0, -1- Mathf.Abs(0.1f/ambient.ThinkSpeed -1f)) * Time.deltaTime));
        }
        else if (transform.position.x > 1.5f)
        {
            this.GetComponent<Rigidbody>().MovePosition(transform.position + (new Vector3(-1 - Mathf.Abs(0.1f / ambient.ThinkSpeed -1f), 0, 0) * Time.deltaTime));
        }
         else
        {
            Destroy(this.gameObject);
        }

        if (transform.position.x < 4.5f)
        {
            transform.localScale = Vector3.Lerp(initScale, Vector3.zero, 1-(transform.position.x-1f)/3.5f);
        }
    }
}
