using UnityEngine;
using System.Collections;

public class UnityHUD : MonoBehaviour
{
    public bool showMouse = true;
    public bool showDebug = true;
    public bool showHelp = true;

    public int screenshotScale = 1;

    public float fastForwardMultiplier = 8F;
    static bool fastForwardActive = false;
    static float timeScaleBackup = 1F;

    GUIStyle style;
    static string text = "";
    static string status = "";
    static string systemInfo = "";
    static string applicationInfo = "";
    static string fps = "";
    static int counter = 0;
    static float timeout = 0F;




    void Start()
    {
        Texture2D backgroundTexture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        Color backgroundColor = new Color(0.0F, 0.0F, 0.0F, 0.4F);
        Color[] backgroundPixels = backgroundTexture.GetPixels();

        for (int i = backgroundPixels.Length - 1; i >= 0; i--)
            backgroundPixels[i] = backgroundColor;

        backgroundTexture.SetPixels(backgroundPixels);
        backgroundTexture.Apply();

        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.normal.background = backgroundTexture;
        style.wordWrap = true;
        style.padding = new RectOffset(4, 4, 4, 4);

        systemInfo = SystemInfo.deviceName + " (" + ((float)SystemInfo.systemMemorySize / 1024F).ToString("0") + "GB) - " + SystemInfo.graphicsDeviceName + " (" + ((float)SystemInfo.graphicsMemorySize / 1024F).ToString("0") + "GB)";
        applicationInfo = Application.productName + " v" + Application.version + " by " + Application.companyName;

        Debug.Log(Application.persistentDataPath);

        SetCursorVisibility(showMouse);
    }



    public static void Log(string _text)
    {
        text += _text;
    }



    void Update()
    {
        if (showDebug)
        {
            if (timeout > 0F)
            {
                timeout -= Time.deltaTime;
                counter++;
            }

            if (timeout <= 0F)
            {
                fps = " - " + counter.ToString() + "fps (" + (Time.deltaTime * 1000).ToString("0.0") + "ms)";
                timeout = 1F;
                counter = 0;
            }
        }

        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
                MakeScreenshot();

            if (Input.GetKeyDown(KeyCode.F))
                ToggleFastForward();

            if (Input.GetKeyDown(KeyCode.M))
                ToggleCursorVisibility();

            if (Input.GetKeyDown(KeyCode.D))
            {
                ToggleDebugVisibility();
            }

            if (Input.GetKeyDown(KeyCode.H))
                ToggleHelpVisibility();
        }

        if (fastForwardActive)
            status += " FF" + fastForwardMultiplier + "x";

        StartCoroutine(ClearStrings());
    }



    IEnumerator ClearStrings()
    {
        yield return new WaitForEndOfFrame();

        text = "";
        status = "";
    }



    void MakeScreenshot()
    {
        System.DateTime now = System.DateTime.Now;

        /*
        string uri = System.IO.Path.Combine(
            Application.dataPath,
            Application.productName + "_" + now.Year + now.Month + now.Day + "_" + now.Hour + now.Minute + now.Second + ".png"
        );
        */

        string uri = Application.productName + "_" + now.Year + now.Month + now.Day + "_" + now.Hour + now.Minute + now.Second + ".png";

        Debug.Log("Saving screenshot \"" + uri + "\"");

        Application.CaptureScreenshot(uri, screenshotScale);
    }



    void ToggleFastForward()
    {
        if (!fastForwardActive)
        {
            timeScaleBackup = Time.timeScale;
            Time.timeScale = fastForwardMultiplier;
            fastForwardActive = true;
        }
        else
        {
            Time.timeScale = timeScaleBackup;
            fastForwardActive = false;
        }
    }



    void ToggleCursorVisibility()
    {
        UnityEngine.Cursor.visible = !UnityEngine.Cursor.visible;
    }


    void SetCursorVisibility(bool _visibility)
    {
        UnityEngine.Cursor.visible = _visibility;
    }



    void ToggleDebugVisibility()
    {
        showDebug = !showDebug;
    }



    void ToggleHelpVisibility()
    {
        showHelp = !showHelp;
    }



    void OnGUI()
    {
        if (!showDebug)
            return;

        GUIContent content = new GUIContent((applicationInfo + " - " + systemInfo + fps + status + "\n" + text).Trim());

        int width = Screen.width;
        int height = 256;

        style.alignment = TextAnchor.UpperLeft;
        height = Mathf.RoundToInt(style.CalcHeight(content, (float)width));

        GUI.Box(new Rect(0, 0, width, height), content, style);
    }
}
