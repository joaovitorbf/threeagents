using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Montador : MonoBehaviour
{
    private bool logThisAgent = false;

    public AmbientController ambient;

    public string currentState;

    public GameObject CaixaTermica;

    // Start is called before the first frame update
    void Start()
    {
        currentState = "IDLE";
        StartCoroutine("Think");
    }

    private IEnumerator Think()
    {
        while (true)
        {
            yield return new WaitForSeconds(ambient.ThinkSpeed);

            if (Random.Range(0, 5) == 0) ambient.requisicoes += 10;

            // Agente não está fazendo nada. Verifica se é necessário fazer algo.
            if (currentState == "IDLE")
            {
                if (logThisAgent) print("STATE THINK: IDLE");
                if (SensorQuantidadeVacinas() > 0 && SensorRequisicoes() >= 10)
                {
                    currentState = "MONTAGEM";
                }
            }

            // Processo de montagem das caixas térmicas
            else if (currentState == "MONTAGEM")
            {
                if (logThisAgent) print("STATE THINK: MONTAGEM");
                List<GameObject> montagem = SensorMontagem();
                foreach (GameObject item in montagem)
                {
                    if (item != null && ambient.requisicoes >= 10) {
                        CaixaVacina caixa = item.GetComponent<CaixaVacina>();
                        if (caixa.amount >= 10) caixa.amount -= 10;
                        if (caixa.amount <= 0)
                        {
                            ambient.montagem[caixa.position] = null;
                            GameObject.Destroy(item);
                        }
                        yield return new WaitForSeconds(ambient.ThinkSpeed*10f);
                        GameObject caixaTermica = Instantiate(CaixaTermica) as GameObject;
                        ambient.requisicoes -= 10;
                        caixaTermica.GetComponent<CaixaTermica>().transform.position = transform.position + new Vector3(1, 0, 0);
                        currentState = "IDLE";
                        break;
                    }
                }
            }
        }
    }

    // Sensor que retorna o ambiente de montagem
    private List<GameObject> SensorMontagem()
    {
        return ambient.montagem;
    }

    // Sensor que retorna o número de requisições
    private int SensorRequisicoes()
    {
        return ambient.requisicoes;
    }

    // Sensor que retorna a quantidade de vacinas na área de montagem
    private int SensorQuantidadeVacinas()
    {
        int cnt = 0;
        foreach (GameObject item in ambient.montagem)
        {
            if (item != null)
            {
                cnt += item.GetComponent<CaixaVacina>().amount;
            }
        }
        return cnt;
    }

}
