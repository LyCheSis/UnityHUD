using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnityHUD : MonoBehaviour
{
    [Tooltip("UI Text element to display messages on. Leave empty to use GUI")]
    public Text debugText;

    [Tooltip("Application version. If left empty the mobile version number is used.")]
    public string version = "";

    [Tooltip("Show/Hide mouse pointer")]
    public bool showMouse = true;

    [Tooltip("Show/Hide debug information")]
    public bool showDebug = true;

    private bool showHelp = false;

    [Tooltip("Show/Hide system information")]
    public bool showSystemInfo = true;

    [Tooltip("Show/Hide application information")]
    public bool showApplicationInfo = true;

    [Tooltip("Show help as introduction after start")]
    public bool intro = true;

    [Tooltip("Duration for which to show introduction")]
    public float introDuration = 4F;

    [Tooltip("Duration for which to show messages")]
    public float messageDuration = 4F;

    [Tooltip("Scale of screen shot")]
    public int screenshotScale = 1;

    [Tooltip("Multiplier while in fast forward mode")]
    public float fastForwardMultiplier = 8F;
    static bool fastForwardActive = false;
    static float timeScaleBackup = 1F;

    [Tooltip("Width of help overlay")]
    public int helpWidth = 320;

    GUIStyle style;
    static string text = "";
    static string help = "RShift+H: Show Help\nRShift+D: Show Debug Information\nRShift+F: Toggle Fast Forward\nRShift+S: Save Screenshot\n";
    static string message = "";
    static string status = "";
    static string outputSystem = "";
    static string outputApplication = "";
    static string outputScreen = "";
    static string outputFps = "";
    static string output = "";
    static int frameCounter = 0;
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

        outputSystem =
            SystemInfo.deviceName + " (" + ((float)SystemInfo.systemMemorySize / 1024F).ToString("0") + "GB) - " +
            SystemInfo.graphicsDeviceName + " (" + ((float)SystemInfo.graphicsMemorySize / 1024F).ToString("0") + "GB)"; //  + " " + Screen.dpi + "dpi " + Screen.orientation;

        outputApplication = Application.productName + " " + (version != "" ? version : "v" + Application.version) + " by " + Application.companyName;

        Debug.Log(Application.persistentDataPath);

        SetCursor(showMouse);

        StartCoroutine(AutoHide());
    }



    public static void Log(string _text)
    {
        text += _text;
    }



    public static void Help(string _text)
    {
        help += help;
    }



    void Update()
    {
        if (showDebug)
        {
            if (timeout > 0F)
            {
                timeout -= Time.deltaTime;
                frameCounter++;
            }

            if (timeout <= 0F)
            {
                outputFps = frameCounter.ToString() + "fps (" + (Time.deltaTime * 1000).ToString("0.0") + "ms)";
                outputScreen = Screen.width + "x" + Screen.height;
                timeout = 1F;
                frameCounter = 0;
            }
        }

        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
                MakeScreenshot();

            if (Input.GetKeyDown(KeyCode.F))
                ToggleFastForward();

            if (Input.GetKeyDown(KeyCode.M))
                ToggleCursor();

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (showHelp)
                {
                    showHelp = false;
                    showDebug = true;
                }
                else
                {
                    ToggleDebug();
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
                ToggleHelp();
        }

        if (fastForwardActive)
            status += " FF" + fastForwardMultiplier + "x";

        StartCoroutine(ClearStrings());
    }



    void LateUpdate()
    {
        if (debugText != null)
        {
            output = "";

            if (showApplicationInfo)
                output += outputApplication + " - ";

            if (showSystemInfo)
                output += outputSystem + " - ";

            output += outputScreen + " - " + outputFps + status + "\n" + text;

            debugText.text = output;
        }
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



    public void ToggleCursor()
    {
        UnityEngine.Cursor.visible = !UnityEngine.Cursor.visible;
    }


    public void SetCursor(bool _visibility)
    {
        UnityEngine.Cursor.visible = _visibility;
    }



    public void ToggleDebug()
    {
        showDebug = !showDebug;
    }



    public void ToggleHelp()
    {
        showHelp = !showHelp;
    }



    IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(introDuration);

        intro = false;
    }



    void OnGUI()
    {
        GUIContent content;
        int width;
        int height;


        if (showHelp || intro)
        {
            content = new GUIContent(("<b>" + outputApplication + "</b>" + status + "\n\n" + help).Trim());

            width = helpWidth;
            height = 256;

            style.alignment = TextAnchor.UpperLeft;
            height = Mathf.RoundToInt(style.CalcHeight(content, (float)width));

            GUI.Box(new Rect(Screen.width / 2 - helpWidth / 2, (Screen.height / 2 - height / 2) / 2, width, height), content, style);
        }
        else if (showDebug)
        {
            content = new GUIContent(("<b>" + outputApplication + "</b> " + outputSystem + " - " + outputScreen + " - " + outputFps + status + "\n" + text).Trim());

            width = Screen.width;
            height = 256;

            style.alignment = TextAnchor.UpperLeft;
            height = Mathf.RoundToInt(style.CalcHeight(content, (float)width));

            GUI.Box(new Rect(0, 0, width, height), content, style);
        }
    }
}
