using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaixaVacina : MonoBehaviour
{
    public int expiry, position, amount;
    public bool organized, montagem;

    public AmbientController ambient;

    private void Awake()
    {
        this.expiry = Random.Range(0, 9999);
        this.amount = 30;
        this.organized = false;
        this.montagem = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Animação de diminuir de tamanho quando vacinas são consumidas
        if (montagem)
        {
            float scale = 0.1f+(1.0f / 30 * amount);
            transform.localScale = Vector3.Lerp(this.gameObject.transform.localScale, Vector3.one * scale, 2.0f * Time.deltaTime);
        }
    }

}

// Classe para comparar a data de expiração de forma a ordenar as caixas de vacina
public class CaixaVacinaComparer : IComparer<GameObject>
{
    public int Compare(GameObject c1, GameObject c2)
    {
        CaixaVacina caixa1 = c1.GetComponent<CaixaVacina>();
        CaixaVacina caixa2 = c2.GetComponent<CaixaVacina>();
        return caixa1.expiry.CompareTo(caixa2.expiry);
    }
}
