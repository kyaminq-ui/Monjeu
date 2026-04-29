using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    // =========================================================
    // RÉFÉRENCES
    // =========================================================
    [Header("Références")]
    public Image fillImage;
    public TextMeshProUGUI timeText;

    // =========================================================
    // COULEURS
    // =========================================================
    [Header("Couleurs")]
    public Color colorGreen  = new Color(0.20f, 0.80f, 0.20f);
    public Color colorOrange = new Color(1.00f, 0.50f, 0.00f);
    public Color colorRed    = new Color(0.80f, 0.10f, 0.10f);

    // =========================================================
    // SEUILS
    // =========================================================
    [Header("Seuils (secondes restantes)")]
    public float thresholdOrange = 8f;
    public float thresholdRed    = 5f;

    private float maxDuration;

    // =========================================================
    // INITIALISATION
    // =========================================================
    void Start()
    {
        if (TurnManager.Instance == null) return;
        maxDuration = TurnManager.Instance.turnDuration;
        TurnManager.Instance.OnTurnStart += _ => RefreshMaxDuration();
    }

    private void RefreshMaxDuration()
    {
        maxDuration = TurnManager.Instance.turnDuration;
        transform.localScale = Vector3.one;
    }

    // =========================================================
    // MISE À JOUR
    // =========================================================
    void Update()
    {
        if (TurnManager.Instance == null || !TurnManager.Instance.IsCombatActive) return;

        float remaining = TurnManager.Instance.TimeRemaining;
        float ratio = (maxDuration > 0f) ? remaining / maxDuration : 0f;

        // Barre de remplissage
        if (fillImage != null)
            fillImage.fillAmount = ratio;

        // Texte (arrondi au supérieur)
        if (timeText != null)
            timeText.text = Mathf.CeilToInt(remaining).ToString();

        // Couleur selon le temps restant
        Color targetColor;
        if (remaining > thresholdOrange)      targetColor = colorGreen;
        else if (remaining > thresholdRed)    targetColor = colorOrange;
        else                                   targetColor = colorRed;

        if (fillImage != null) fillImage.color = targetColor;
        if (timeText != null)  timeText.color  = targetColor;

        // Pulsation d'urgence sous le seuil rouge
        if (remaining <= thresholdRed && remaining > 0f)
        {
            float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 10f);
            transform.localScale = Vector3.one * pulse;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }
}
