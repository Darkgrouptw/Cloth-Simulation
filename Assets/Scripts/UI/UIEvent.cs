using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEvent : MonoBehaviour
{
    public GameObject Balls;
    public GameObject[] Cloths;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Cloth System-Status"))
            PlayerPrefs.SetInt("Cloth System Status", 0);

        if (!PlayerPrefs.HasKey("Cloth System-Ball active"))
            PlayerPrefs.SetInt("Cloth System-Ball active", 0);

        int status = PlayerPrefs.GetInt("Cloth System Status");
        SetBallActivByStatus(status);

        int ballStatus = PlayerPrefs.GetInt("Cloth System-Ball active");
        Balls.SetActive(ballStatus == 1);
    }

    private void SetBallActivByStatus(int status)
    {
        for (int i = 0; i < Cloths.Length; i++)
            Cloths[i].SetActive(i == status);
    }

    public void SetClothActive(int status)
    {
        PlayerPrefs.SetInt("Cloth System-Status", status);
        SceneManager.LoadScene(0);
    }

    public void SetBallActive()
    {
        int ballStatus = PlayerPrefs.GetInt("Cloth System-Ball active");
        if (ballStatus == 1)
            PlayerPrefs.SetInt("Cloth System-Ball active", 0);
        else
            PlayerPrefs.SetInt("Cloth System-Ball active", 1);
        SceneManager.LoadScene(0);
    }
}
