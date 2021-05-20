using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transportador : MonoBehaviour
{
    private bool logThisAgent = false;

    public string currentState;
    public AmbientController ambient;
    private GameObject caixaFetch;
    private GameObject caixaCarry;

    // Start is called before the first frame update
    void Start()
    {
        currentState = "IDLE";
        StartCoroutine("Think");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Think()
    {
        while (true)
        {
            yield return new WaitForSeconds(ambient.ThinkSpeed);

            // Agente não está fazendo nada. Pensa se é necessário fazer algo.
            if (currentState == "IDLE")
            {
                if (logThisAgent) print("STATE THINK: IDLE");
                if (SensorRequisicoes() > SensorQuantidadeVacinas() && SensorQuantidadeCaixas() < 6)
                {
                    currentState = "FETCH_PREPARE";
                } else
                {
                    if (transform.position.x > 5.7f) MoveLeft();
                    else if (transform.position.x < 5.2f) MoveRight();
                    else if (transform.position.z > 3.7f) MoveDown();
                    else if (transform.position.z < 3.2f) MoveUp();
                    else
                    {
                        Land();
                    }
                }
            }

            // Preparar a busca de uma caixa
            else if (currentState == "FETCH_PREPARE")
            {
                if (logThisAgent) print("STATE THINK: FETCH_PREPARE");
                List<GameObject> prateleiras = SensorPrateleiras();
                caixaFetch = null;
                for (int i = 0; i < prateleiras.Count; i++)
                {
                    if (prateleiras[i] != null && prateleiras[i].GetComponent<CaixaVacina>().organized == true)
                    {
                        caixaFetch = prateleiras[i];
                        break;
                    }
                }
                if (caixaFetch != null)
                {
                    currentState = "FETCH_GET";
                }
            }

            // Buscar a caixa ordenada mais próxima da área de montagem.
            else if (currentState == "FETCH_GET")
            {
                if (logThisAgent) print("STATE THINK: FETCH_GET");
                CaixaVacina caixa = caixaFetch.GetComponent<CaixaVacina>();
                if (Vector3.Distance(transform.position, ambient.PositionToVector3(caixa.position) - new Vector3(0, 0, 1)) > 0.7f)
                {
                    MoveTowardsPosition(caixa.position);
                }
                else
                {
                    caixaCarry = ambient.prateleiras[caixa.position];
                    caixaCarry.tag = "CaixaVacinaMontagem";
                    ambient.prateleiras[caixa.position] = null;
                    ActuatorGrabCaixa(caixaCarry);
                    currentState = "FETCH_PUT";
                }
            }

            // Colocar a caixa em um espaço da área de montagem.
            else if (currentState == "FETCH_PUT")
            {
                if (logThisAgent) print("STATE THINK: FETCH_PUT");
                if (SensorNextMontagemAvailable() == -1) continue;
                Vector3 target = ambient.MontagemPositionToVector3(SensorNextMontagemAvailable());
                if (Vector3.Distance(transform.position, target - new Vector3(1, 0, 0)) > 0.7f)
                {
                    MoveTowardsMontagemPosition(SensorNextMontagemAvailable());
                } else
                {
                    ActuatorPutCaixa(caixaCarry, SensorNextMontagemAvailable());
                    currentState = "IDLE";
                }
            }
        }
    }

    // Se movimenta em direção a uma posicao das prateleiras
    private void MoveTowardsPosition(int position)
    {
        Vector3 target = ambient.PositionToVector3(position) - new Vector3(0, 0, 1);
        if (position % 2 == 1) Fly(); else Land();
        if (Mathf.Floor(transform.position.z) < Mathf.Floor(target.z)) MoveUp();
        else if (Mathf.Floor(transform.position.z) > Mathf.Floor(target.z)) MoveDown();
        else if (Mathf.Floor(transform.position.x) < Mathf.Floor(target.x)) MoveRight();
        else if (Mathf.Floor(transform.position.x) > Mathf.Floor(target.x)) MoveLeft();
    }

    // Se movimenta em direção a uma posicao da area de montagem
    private void MoveTowardsMontagemPosition(int position)
    {
        Vector3 target = ambient.MontagemPositionToVector3(position) - new Vector3(1, 0, 0);
        if (Mathf.Floor(transform.position.x) < Mathf.Floor(target.x)) MoveRight();
        else if (Mathf.Floor(transform.position.x) > Mathf.Floor(target.x)) MoveLeft();
        else if (Mathf.Floor(transform.position.z) > Mathf.Floor(target.z)) MoveDown();
        else if (Mathf.Floor(transform.position.z) < Mathf.Floor(target.z)) MoveUp();
    }

    // Atuador de pegar uma caixa
    private void ActuatorGrabCaixa(GameObject caixa)
    {
        caixa.transform.position = transform.position + new Vector3(0, 0.5f, 0);
        caixa.gameObject.transform.parent = this.transform;
    }

    // Atuador de colocar uma caixa em um lugar
    private void ActuatorPutCaixa(GameObject caixa, int montagemPosition)
    {
        if (ambient.montagem[montagemPosition] == null)
        {
            caixa.gameObject.transform.parent = null;
            ambient.montagem[montagemPosition] = caixa;
            ambient.montagem[montagemPosition].GetComponent<CaixaVacina>().position = montagemPosition;
            ambient.montagem[montagemPosition].GetComponent<CaixaVacina>().montagem = true;
            caixa.transform.position = ambient.MontagemPositionToVector3(montagemPosition);
        }
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


    // Sensor que retorna a quantidade de caixas na área de montagem
    private int SensorQuantidadeCaixas()
    {
        int cnt = 0;
        foreach (GameObject item in ambient.montagem)
        {
            if (item != null)
            {
                cnt += 1;
            }
        }
        return cnt;
    }

    // Sensor que retorna o número de requisições
    private int SensorRequisicoes()
    {
        return ambient.requisicoes;
    }

    // Sensor que retorna a lista das prateleiras
    private List<GameObject> SensorPrateleiras()
    {
        return ambient.prateleiras;
    }

    // Sensor que retorna a lista da área de montagem
    private List<GameObject> SensorMontagem()
    {
        return ambient.montagem;
    }


    // Procura um espaço vazio na área de montagem
    private int SensorNextMontagemAvailable()
    {
        for (int i = 0; i < ambient.montagem.Count; i++)
        {
            if (ambient.montagem[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    // Funções de movimentação
    private void MoveUp()
    {
        StartCoroutine(ambient.smoothMove(this.gameObject, transform.position + new Vector3(0, 0, 1), 1));
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    private void MoveDown()
    {
        StartCoroutine(ambient.smoothMove(this.gameObject, transform.position + new Vector3(0, 0, -1), 1));
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    private void MoveLeft()
    {
        StartCoroutine(ambient.smoothMove(this.gameObject, transform.position + new Vector3(-1, 0, 0), 1));
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }
    private void MoveRight()
    {
        StartCoroutine(ambient.smoothMove(this.gameObject, transform.position + new Vector3(1, 0, 0), 1));
        transform.rotation = Quaternion.Euler(0, 90, 0);
    }
    private void Fly()
    {
        transform.position = new Vector3(transform.position.x, 0.4f, transform.position.z);
    }
    private void Land()
    {
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }
}
