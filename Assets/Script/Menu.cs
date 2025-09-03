using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void jogo()
    {
        SceneManager.LoadScene("jogo");
    }

    public void MenuPrincipal()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void Escolha()
    {
        SceneManager.LoadScene("Escolha");
    }

    public void Story()
    {
        SceneManager.LoadScene("Story");
    }


    public void Creditos()
    {
        SceneManager.LoadScene("Creditos");
    }

}