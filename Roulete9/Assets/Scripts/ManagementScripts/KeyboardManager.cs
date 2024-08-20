using System.Collections;
using UnityEngine;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    public RectTransform panel; // The panel containing your input fields
    public TMP_InputField[] inputFields; // All the TMP input fields to monitor
    private Vector3 originalPanelPosition;
    private bool keyboardVisible;
    private float keyboardHeight = 250; // Adjust based on your keyboard height

    void Start()
    {
        originalPanelPosition = panel.localPosition;
        keyboardVisible = false;
    }

    void Update()
    {
        foreach (TMP_InputField inputField in inputFields)
        {
            if (inputField.isFocused && TouchScreenKeyboard.visible && !keyboardVisible)
            {
                MovePanelUp(inputField);
                keyboardVisible = true;
                break;
            }
            else if (!TouchScreenKeyboard.visible && keyboardVisible)
            {
                StartCoroutine(RestorePanel());
                keyboardVisible = false;
                break;
            }
        }
    }

    void MovePanelUp(TMP_InputField activeInputField)
    {
        Vector3 inputFieldPosition = activeInputField.GetComponent<RectTransform>().position;
        float inputFieldY = inputFieldPosition.y;
        float screenHeight = Screen.height;

        float requiredMoveUp = keyboardHeight - (screenHeight - inputFieldY);
        if (requiredMoveUp > 0)
        {
            StartCoroutine(AnimatePanelUp(requiredMoveUp));
        }
    }

    IEnumerator AnimatePanelUp(float moveUpDistance)
    {
        Vector3 targetPosition = new Vector3(panel.localPosition.x, originalPanelPosition.y + moveUpDistance, panel.localPosition.z);
        while (Vector3.Distance(panel.localPosition, targetPosition) > 0.01f)
        {
            panel.localPosition = Vector3.Lerp(panel.localPosition, targetPosition, Time.deltaTime * 10);
            yield return null;
        }
        panel.localPosition = targetPosition;
    }

    IEnumerator RestorePanel()
    {
        while (Vector3.Distance(panel.localPosition, originalPanelPosition) > 0.01f)
        {
            panel.localPosition = Vector3.Lerp(panel.localPosition, originalPanelPosition, Time.deltaTime * 10);
            yield return null;
        }
        panel.localPosition = originalPanelPosition;
    }
}
