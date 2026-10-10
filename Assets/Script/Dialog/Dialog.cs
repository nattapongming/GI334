using TMPro;
using UnityEngine;

public class Dialog : MonoBehaviour
{
    [SerializeField] private TMP_Text notiText;
    public TMP_Text NotiText { get { return notiText; } set { notiText = value; } }

    [SerializeField] private TMP_Text yesText;

    [SerializeField] private TMP_Text noText;

    public void DialogGoToEuropeInit(DialogData data)
    {
        if (notiText != null) notiText.text = data.question;

        if (yesText != null) yesText.text = data.answers[0];

        if (noText != null) noText.text = data.answers[1];
    }
}