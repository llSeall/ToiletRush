using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button continueButton;

    [Header("Panels")]
    public GameObject loadScenePanel;

    private CanvasGroup panelCanvas;
    private Coroutine panelRoutine;

    void Start()
    {
        if (loadScenePanel != null)
        {
            panelCanvas = loadScenePanel.GetComponent<CanvasGroup>();
            if (panelCanvas == null)
                panelCanvas = loadScenePanel.AddComponent<CanvasGroup>();

            loadScenePanel.SetActive(false);
        }

        continueButton.interactable = SaveManager.HasSave();
    }

    // ---------------- CONTINUE ----------------
    public void OpenLoadScenePanel()
    {
        if (!SaveManager.HasSave()) return;

        if (panelRoutine != null)
            StopCoroutine(panelRoutine);

        loadScenePanel.SetActive(true);
        panelRoutine = StartCoroutine(OpenPanelAnim());
    }

    // ---------------- CLOSE ----------------
    public void CloseLoadScenePanel()
    {
        if (panelRoutine != null)
            StopCoroutine(panelRoutine);

        panelRoutine = StartCoroutine(ClosePanelAnim());
    }

    // ================= ANIMATION =================

    IEnumerator OpenPanelAnim()
    {
        panelCanvas.alpha = 0f;
        panelCanvas.interactable = false;
        panelCanvas.blocksRaycasts = false;

        RectTransform rt = loadScenePanel.transform as RectTransform;
        rt.localScale = Vector3.one * 0.9f;

        float t = 0f;
        float duration = 0.25f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;

            panelCanvas.alpha = Mathf.Lerp(0f, 1f, p);
            rt.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, p);

            yield return null;
        }

        panelCanvas.alpha = 1f;
        rt.localScale = Vector3.one;
        panelCanvas.interactable = true;
        panelCanvas.blocksRaycasts = true;
    }

    IEnumerator ClosePanelAnim()
    {
        panelCanvas.interactable = false;
        panelCanvas.blocksRaycasts = false;

        RectTransform rt = loadScenePanel.transform as RectTransform;

        float t = 0f;
        float duration = 0.2f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;

            panelCanvas.alpha = Mathf.Lerp(1f, 0f, p);
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, p);

            yield return null;
        }

        panelCanvas.alpha = 0f;
        loadScenePanel.SetActive(false);
    }
}
