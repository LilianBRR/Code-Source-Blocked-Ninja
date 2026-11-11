using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndDoorScript : MonoBehaviour
{
    private GameObject _winMenu;

    private void Start()
    {
        _winMenu = GameObject.FindWithTag("WinMenu");
        if(_winMenu != null)
        {
            _winMenu.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;

        StartCoroutine(SwitchToMainMenu());
    }

    IEnumerator SwitchToMainMenu()
    {
        yield return new WaitForSecondsRealtime(4f);

        Time.timeScale = 0f;

        _winMenu.SetActive(true);
        _winMenu.GetComponentInChildren<WinMenuManager>().StopGame();
    }
}
