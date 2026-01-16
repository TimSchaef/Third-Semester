using System.IO;
using UnityEngine;

public class ScreenshotWithResolution : MonoBehaviour
{
    [Header("Key")]
    public KeyCode screenshotKey = KeyCode.P;

    [Header("Output")]
    public string folderName = "Screenshots";
    public bool alsoWriteTxt = true;

    private void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        int w = Screen.width;
        int h = Screen.height;

        string folderPath = Path.Combine(Application.persistentDataPath, folderName);
        Directory.CreateDirectory(folderPath);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string baseName = $"screenshot_{timestamp}_{w}x{h}";

        string pngPath = Path.Combine(folderPath, baseName + ".png");

        // Screenshot speichern
        ScreenCapture.CaptureScreenshot(pngPath);

        // Auflösung ausgeben
        Debug.Log($"Screenshot angefordert: {w} x {h} -> {pngPath}");

        // Optional: Auflösung zusätzlich als Textdatei speichern
        if (alsoWriteTxt)
        {
            string txtPath = Path.Combine(folderPath, baseName + ".txt");
            File.WriteAllText(txtPath, $"{w} x {h}");
        }
    }
}
