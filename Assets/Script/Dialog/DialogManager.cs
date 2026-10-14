using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private Dialog[] dialogs; // 0 - Naval

    [SerializeField] private DialogData[] dialogDatas;

    public static DialogManager instance;

    void Awake()
    {
        instance = this;
    }

    public void GoToEuropeQuestion()
    {
        dialogs[0].gameObject.SetActive(true);
        dialogs[0].DialogGoToEuropeInit(dialogDatas[0]);
    }

    public void YesGoToEurope()
    {
        dialogs[0].gameObject.SetActive(false);
        EuropeManager.instance.AllowToGoToEurope();
    }

    public void NoGoToEurope()
    {
        dialogs[0].gameObject.SetActive(false);
    }

    public void ToggleAiMoveDialog(bool flag)
    {
        if (flag)
            dialogs[1].NotiText.text = 
            ($"Year: {GameManager.instance.Year}\nAI is moving...");

        dialogs[1].gameObject.SetActive(flag);
            
    }

    public void ToggleNavalDialog(bool flag)
    {
        dialogs[0].gameObject.SetActive(flag);
    }
}