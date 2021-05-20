using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AmbientController : MonoBehaviour
{
    // Controlador do ambiente
    // Guarda o estado atual do ambiente
    // e funções/propriedades comuns

    // Referencias aos agentes
    public GameObject Montador;
    public GameObject Organizador;
    public GameObject Transportador;

    // Referencias aos objetos de caixa
    public GameObject CaixaTermica;
    public GameObject CaixaVacina;

    // Listas do ambiente
    public List<GameObject> prateleiras { get; set; }
    public List<GameObject> montagem { get; set; }

    // Velocidade de pensamento dos agentes (em segundos)
    public float ThinkSpeed;

    // Variaveis da interface gráfica
    public Text requisicoesText;
    public Text estadosText;
    public int requisicoes;

    private void Awake()
    {
        // Configuração
        requisicoes = 90;
        ThinkSpeed = 0.1f;

        // Inicializa a lista das prateleiras
        prateleiras = new List<GameObject>();
        for (int i = 0; i < 48; i++)
        {
            prateleiras.Add(null);
        }

        //Inicializa a lista da área de montagem
        montagem = new List<GameObject>();
        for (int i = 0; i < 6; i++)
        {
            montagem.Add(null);
        }

        // Coloca 10 caixas em posições aleatórias nas prateleiras
        for (int i = 0; i < 20; i++)
        {
            int position = Random.Range(0, 24) * 2 + Random.Range(0, 2);
            while (prateleiras[position] != null)
            {
                position = position = Random.Range(0, 24) * 2 + Random.Range(0, 2);
            }
            prateleiras[position] = Instantiate(CaixaVacina) as GameObject;
            prateleiras[position].GetComponent<CaixaVacina>().position = position;
            Dictionary<string, float> coords = PositionToCoordinates(position);
            prateleiras[position].GetComponent<CaixaVacina>().transform.position = new Vector3(coords["x"], coords["y"], coords["z"]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Textos da interface
        requisicoesText.text = "Requisições: " + requisicoes;
        estadosText.text = "Organizador: " + Organizador.GetComponent<Organizador>().currentState;
        estadosText.text += "\nTransportador: " + Transportador.GetComponent<Transportador>().currentState;
        estadosText.text += "\nMontador: " + Montador.GetComponent<Montador>().currentState;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            requisicoes += 10;
        }
    }

    // Transforma um número de posição 0 - 48 das prateleiras em coordenadas
    public Dictionary<string, float> PositionToCoordinates(int position)
    {
        float x = (12.5f + ((position/2) % 6)) - Mathf.Floor(position / 2 / 12) * 10f;
        float y = (position % 2) * 0.4f;
        float z = 9.5f;
        if (position/2 <= 5 || (position/2 >= 12 && position/2 <= 17)) z -= 2f;
        Dictionary<string, float> output = new Dictionary<string, float>();
        output.Add("x", x);
        output.Add("y", y);
        output.Add("z", z);
        return output;
    }

    // Transforma um número de posição 0 - 48 das prateleiras em um Vector3
    public Vector3 PositionToVector3(int position)
    {
        Dictionary<string, float> coords = PositionToCoordinates(position);
        return new Vector3(coords["x"], coords["y"], coords["z"]);
    }

    // Transforma um número de posição 0 - 48 da area de montagem em um Vector3
    public Vector3 MontagemPositionToVector3(int position)
    {
        float x = 13.5f - Mathf.Floor(position / 3);
        float y = 0.0f;
        float z = 4.5f - (position % 3);
        return new Vector3(x, y, z);
    }

    // Troca duas caixas de lugar nas prateleiras
    public void SwitchPlaces(int pos1, int pos2)
    {
        GameObject temp = prateleiras[pos2];
        prateleiras[pos2] = prateleiras[pos1];
        prateleiras[pos1] = temp;
        if (prateleiras[pos1] != null) prateleiras[pos1].GetComponent<CaixaVacina>().position = pos1;
        if (prateleiras[pos2] != null) prateleiras[pos2].GetComponent<CaixaVacina>().position = pos2;
    }

    // Coroutine de movimentação suave
    public IEnumerator smoothMove(GameObject obj, Vector3 target, float speed)
    {
        Vector3 initialObjPos = obj.transform.position;
        while (Vector3.Distance(obj.transform.position, target) > 0.01)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, target, (Time.deltaTime * 10 * speed) / ThinkSpeed);
            yield return null;
        }
        obj.transform.position = target;
    }
}
