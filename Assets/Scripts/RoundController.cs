using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundController : MonoBehaviour
{
    Slider slider;
    Text roundText;
    // Start is called before the first frame update
    void Start()
    {
        slider = this.GetComponent<Slider>();
        roundText = this.GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateRoundText()
    {
        roundText.text = "Rounds: " + (int)slider.value;
    }
}
