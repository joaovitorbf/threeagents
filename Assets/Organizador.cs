using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Organizador : MonoBehaviour
{
    private bool logThisAgent = false;

    public AmbientController ambient;

    public string currentState;
    private List<GameObject> sortedCaixaList;
    private int currentSortingPosition, makeSpaceNullPosition;
    private GameObject caixaCarrying;

    // Start is called before the first frame update
    void Start()
    {
        currentState = "IDLE";
        StartCoroutine("Think");
    }

    // Função de pensamento. Agente raciocina a cada 0.5 segundos;
    private IEnumerator Think()
    {
        while (true)
        {
            yield return new WaitForSeconds(ambient.ThinkSpeed);

            // Agente parado. Verifica se é necessário começar a fazer algo.
            if (currentState == "IDLE")
            {
                if (logThisAgent) print("STATE THINK: IDLE");
                if (IsPrateleirasSorted(SensorPrateleiras(), SortListCaixas(SensorCaixas())) == false)
                {
                    currentState = "ORGANIZING_IDLE";
                } else
                {
                    if (transform.position.x > 9.7f) MoveLeft();
                    else if (transform.position.x < 9.2f) MoveRight();
                    else
                    {
                        Land();
                    }
                    
                }
            }

            // Inicio do processo de organização.
            else if (currentState == "ORGANIZING_IDLE")
            {
                if (logThisAgent) print("STATE THINK: ORGANIZING_IDLE");
                currentSortingPosition = 0;
                sortedCaixaList = SortListCaixas(SensorCaixas());
                foreach (GameObject caixa in SensorCaixas())
                {
                    caixa.GetComponent<CaixaVacina>().organized = false;
                }
                currentState = "ORGANIZING_MOVE";
            }

            // Início do processo de movimentação de uma caixa para outro lugar
            else if (currentState == "ORGANIZING_MOVE")
            {
                sortedCaixaList = SortListCaixas(SensorCaixas());
                if (logThisAgent) print("STATE THINK: ORGANIZING_MOVE");

                if (currentSortingPosition >= sortedCaixaList.Count)
                {
                    currentState = "IDLE";
                    continue;
                }

                // Verifica se o espaço que deseja colocar uma caixa já está ocupado
                // e inicia o processo de abrir espaço caso positivo
                CaixaVacina currentCaixa = sortedCaixaList[currentSortingPosition].GetComponent<CaixaVacina>();
                if (SensorPrateleiras()[currentSortingPosition] != null)
                {
                    List<GameObject> prateleiras = SensorPrateleiras();
                    for (int i = 0; i < 48; i++)
                    {
                        if (prateleiras[i] == null)
                        {
                            makeSpaceNullPosition = i;
                            break;
                        }
                    }
                    currentState = "ORGANIZING_MAKESPACE_GET";
                }

                // Se já tem espaço, inicia o processo de reposicionamento de caixa
                else if (Vector3.Distance(transform.position, currentCaixa.transform.position) > 1.25)
                {
                    MoveTowardsPosition(currentCaixa.position);
                } else
                {
                    caixaCarrying = SensorPrateleiras()[currentCaixa.position];
                    caixaCarrying.transform.position = transform.position + new Vector3(0, 0.3f, 0);
                    caixaCarrying.gameObject.transform.parent = transform;
                    currentState = "ORGANIZING_REPOSITION";
                }
            }

            // Busca a caixa que está ocupando espaço
            else if (currentState == "ORGANIZING_MAKESPACE_GET")
            {
                sortedCaixaList = SortListCaixas(SensorCaixas());
                if (logThisAgent) print("STATE THINK: ORGANIZING_MAKESPACE_GET");
                GameObject caixaToRelocate = SensorPrateleiras()[currentSortingPosition];
                if (Vector3.Distance(transform.position, caixaToRelocate.transform.position) > 1.25)
                {
                    MoveTowardsPosition(caixaToRelocate.GetComponent<CaixaVacina>().position);
                }
                else
                {
                    caixaCarrying = SensorPrateleiras()[caixaToRelocate.GetComponent<CaixaVacina>().position];
                    caixaCarrying.gameObject.transform.position = transform.position + new Vector3(0, 0.3f, 0);
                    caixaCarrying.gameObject.transform.parent = this.gameObject.transform;
                    currentState = "ORGANIZING_MAKESPACE_PUT";
                }
            }

            // Coloca a caixa que estava ocupando espaço em algum outro espaço vazio
            else if (currentState == "ORGANIZING_MAKESPACE_PUT")
            {
                sortedCaixaList = SortListCaixas(SensorCaixas());
                if (logThisAgent) print("STATE THINK: ORGANIZING_MAKESPACE_PUT");
                Vector3 positionToPut = ambient.PositionToVector3(makeSpaceNullPosition);
                if (Vector3.Distance(transform.position, positionToPut) > 1.25)
                {
                    MoveTowardsPosition(makeSpaceNullPosition);
                    //caixaCarrying.transform.position = transform.position + new Vector3(0, 0.3f, 0);
                } else
                {
                    ambient.SwitchPlaces(caixaCarrying.GetComponent<CaixaVacina>().position, makeSpaceNullPosition);
                    caixaCarrying.gameObject.transform.position = ambient.PositionToVector3(makeSpaceNullPosition);
                    caixaCarrying.gameObject.transform.parent = null;
                    currentState = "ORGANIZING_MOVE";
                }
            }

            // Leva a caixa para seu lugar ordenado
            else if (currentState == "ORGANIZING_REPOSITION")
            {
                sortedCaixaList = SortListCaixas(SensorCaixas());
                if (logThisAgent) print("STATE THINK: ORGANIZING_REPOSITION");
                if (Vector3.Distance(transform.position, ambient.PositionToVector3(currentSortingPosition)) > 1.25)
                {
                    MoveTowardsPosition(currentSortingPosition);
                    //caixaCarrying.transform.position = transform.position + new Vector3(0, 0.3f, 0);
                } else
                {
                    ambient.SwitchPlaces(caixaCarrying.GetComponent<CaixaVacina>().position, currentSortingPosition);
                    caixaCarrying.transform.position = ambient.PositionToVector3(currentSortingPosition);
                    caixaCarrying.gameObject.transform.parent = null;
                    if (ambient.prateleiras[currentSortingPosition] != null) ambient.prateleiras[currentSortingPosition].GetComponent<CaixaVacina>().organized = true;
                    currentSortingPosition += 1;
                    currentState = "ORGANIZING_MOVE";
                }
            }
        }
    }

    // Executa um movimento em direção a alguma posição das prateleiras
    private void MoveTowardsPosition(int position)
    {
        if (position % 2 == 1) Fly(); else Land();
        if (Mathf.Floor(transform.position.x) > Mathf.Floor(ambient.PositionToCoordinates(position)["x"])) MoveLeft();
        else if (Mathf.Floor(transform.position.x) < Mathf.Floor(ambient.PositionToCoordinates(position)["x"])) MoveRight();
    }

    // Sensor que pega o estado atual das caixas
    private List<GameObject> SensorCaixas()
    {
        List<GameObject> listaTag = new List<GameObject>(GameObject.FindGameObjectsWithTag("CaixaVacina"));
        return listaTag;
    }

    // Sensor que pega o estado atual das prateleiras
    private List<GameObject> SensorPrateleiras()
    {
        return ambient.prateleiras;
    }

    // Retorna uma lista organizada das caixas
    private List<GameObject> SortListCaixas(List<GameObject> caixas)
    {
        CaixaVacinaComparer comparer = new CaixaVacinaComparer();
        List<GameObject> caixas_sort = new List<GameObject>(caixas);
        caixas_sort.Sort(comparer);
        return caixas_sort;
    }

    // Verifica se as prateleiras estão organizadas com base na lista geral de caixas
    private bool IsPrateleirasSorted(List<GameObject> prateleiras, List<GameObject> caixas)
    {
        for (int i = 0; i < caixas.Count; i++)
        {
            if (prateleiras[i] == null || prateleiras[i].GetComponent<CaixaVacina>().expiry != caixas[i].GetComponent<CaixaVacina>().expiry)
            {
                return false;
            }
        }
        return true;
    }

    // Funções de movimentação (atuadores)
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
        //transform.position += new Vector3(-1, 0, 0);
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
        transform.position = new Vector3(transform.position.x, 0.25f, transform.position.z);
    }

}
