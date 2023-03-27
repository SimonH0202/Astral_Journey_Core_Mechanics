using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour
{

    public Slider slider;

    public void SetStat(int stat)
    {
        slider.value = stat;
    }

    public void SetMaxStat(int stat)
    {
        slider.maxValue = stat;
        slider.value = stat;
    }

    public void SetStat(float stat)
    {
        slider.value = stat;
    }

    public void SetMaxStat(float stat)
    {
        slider.maxValue = stat;
        slider.value = stat;
    }
}
