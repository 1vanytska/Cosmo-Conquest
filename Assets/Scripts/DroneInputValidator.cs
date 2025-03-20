using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public class DroneInputValidator : MonoBehaviour
{
    public TMP_InputField inputKronus;
    public TMP_InputField inputLyrion;
    public TMP_InputField inputMystara;
    public TMP_InputField inputEclipsia;
    public TMP_InputField inputFiora;

    public TMP_Text errorText;
    public AudioSource clickSound;

    public Button submitButton;

    void Start()
    {
        submitButton.onClick.AddListener(ValidateInputs);
    }

    void ValidateInputs()
    {
        clickSound.Play();

        int kronus = ParseInput(inputKronus.text);
        int lyrion = ParseInput(inputLyrion.text);
        int mystara = ParseInput(inputMystara.text);
        int eclipsia = ParseInput(inputEclipsia.text);
        int fiora = ParseInput(inputFiora.text);

        if (kronus < 0 || lyrion < 0 || mystara < 0 || eclipsia < 0 || fiora < 0 ||
            kronus > 1000 || lyrion > 1000 || mystara > 1000 || eclipsia > 1000 || fiora > 1000)
        {
            errorText.text = "Enter numbers between 0 and 1000!";
            return;
        }

        if (!(kronus >= lyrion && lyrion >= mystara && mystara >= eclipsia && eclipsia >= fiora))
        {
            errorText.text = "Condition violated: Kronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora.";
            return;
        }

        if (kronus + lyrion + mystara + eclipsia + fiora != 1000)
        {
            errorText.text = "The total must be exactly 1000!";
            return;
        }

        errorText.text = "Distribution successful!";
    }

    int ParseInput(string input)
    {
        int result;
        if (!int.TryParse(input, out result))
        {
            result = -1;
        }
        return result;
    }
}
