using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour
{
    private Text FPSText;
	private void Start ()
    {
        FPSText = this.GetComponent<Text>();
        StartCoroutine(FPSCount());
	}

    IEnumerator FPSCount()
    {
        while(true)
        {
            FPSText.text = "FPS:" + (Mathf.Round(100.0f / Time.deltaTime) / 100).ToString();
            yield return new WaitForSeconds(1);
        }
    }
}