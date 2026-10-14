using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    [SerializeField] private TMP_Text notiText;
    public TMP_Text NotiText { get { return notiText; } set { notiText = value; } }

    [SerializeField] private TMP_Text yesText;

    [SerializeField] private TMP_Text noText;

    [SerializeField] private Button btnYes;

    [SerializeField] private Button btnNo;

    [SerializeField] private Button btnOkay;

    [SerializeField] private TMP_Text okayText;

    [SerializeField] private Image charImage;


    [Header("CombatDialog")]

    // Attacker
    [SerializeField] private Image attackerFactionImage;

    [SerializeField] private Image[] attackerImage;

    [SerializeField] private TMP_Text[] attackerNameText;

    [SerializeField] private TMP_Text[] attackerStrText;

    // Defender
    [SerializeField] private Image defenderFactionImage;

    [SerializeField] private Image[] defenderImage;

    [SerializeField] private TMP_Text[] defenderNameText;

    [SerializeField] private TMP_Text[] defenderStrText;    

    public void DialogGoToEuropeInit(DialogData data)
    {
        if (notiText != null) notiText.text = data.question;

        if (yesText != null) yesText.text = data.answers[0];

        if (noText != null) noText.text = data.answers[1];
    }

    public void DialogCombatAnalysisInit(Unit attacker, Unit defender)
    {
        Debug.Log(attackerFactionImage);
        attackerFactionImage.sprite = attacker.Faction.ShieldIcon;
        attackerImage[0].sprite = attacker.UnitSprite.sprite;
        attackerNameText[0].text = attacker.UnitName;
        attackerStrText[0].text = attacker.Strength.ToString();

        defenderFactionImage.sprite = defender.Faction.ShieldIcon;
        defenderImage[0].sprite = defender.UnitSprite.sprite;
        defenderNameText[0].text = defender.UnitName;
        defenderStrText[0].text = defender.Strength.ToString();
    }

    public void DialogCombatResultInit(Unit winner, Unit loser)
    {
        if (notiText != null)
            notiText.text = $"<color=yellow>{winner.Faction.Nation} {winner.UnitName}</color> defeats " + 
            $"<color=yellow>{loser.Faction.Nation} {loser.UnitName}</color>";

        if (btnYes != null) btnYes.gameObject.SetActive(false);

        if (btnNo != null) btnNo.gameObject.SetActive(false);

        if (btnOkay != null) btnOkay.gameObject.SetActive(true);
    }
}