using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocarDeCena : MonoBehaviour
{
    private string cenaParaCarregar;
    [SerializeField] private float tempoDeEspera = 1f;

    public void CarregarCena(string cena)
    {
        SceneManager.LoadScene(cena);
    }

    IEnumerator EsperarECarregarCena(string cena)
    {
        yield return new WaitForSeconds(tempoDeEspera);
        SceneManager.LoadScene(cena);
    }
}
