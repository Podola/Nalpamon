using TMPro;
using UnityEngine;

public class VolumeBubbleHandle : MonoBehaviour
{
    [SerializeField] private TMP_Text volumeText;

    public void Active(float value, Vector2 pos)
    {
        gameObject.SetActive(true);

        volumeText.text = $"{(int)(value * 100)}%";

        transform.position = pos;
    }

    public void Unactive()
    {
        gameObject.SetActive(false);
    }
}
