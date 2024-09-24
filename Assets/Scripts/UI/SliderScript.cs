using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderText;

    void Start()
    {
        slider.onValueChanged.AddListener((v) => { sliderText.text = v.ToString(); });
    }
}
