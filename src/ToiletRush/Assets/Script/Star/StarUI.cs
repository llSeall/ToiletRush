using UnityEngine;
using UnityEngine.UI;

public class StarUI : MonoBehaviour
{
    public Image[] stars; // ใส่ 3 รูป
    public Sprite starOn;
    public Sprite starOff;

    public void ShowStars(int count)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = i < count ? starOn : starOff;
        }
    }
}
