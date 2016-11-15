using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class UnityHUD : MonoBehaviour
{
    public class HelpEntry
    {
        public string hotkey;
        public string description;

        public HelpEntry(string _hotkey, string _description)
        {
            hotkey = _hotkey;
            description = _description;
        }
    }

    [System.Serializable]
    public class ConfigData
    {
        public ConfigEntry[] data;
    }

    [System.Serializable]
    public class ConfigEntry
    {
        public string key;
        public string value;
    }



    [Tooltip("UI Text element to display messages on. Leave empty to use GUI")]
    public Text debugText;

    [Tooltip("Application version. If left empty the mobile version number is used.")]
    public string version = "";

    [Tooltip("JSON file with configuration data.")]
    public string configFile = "config.json";

    [Tooltip("Toggle log")]
    public bool enableLog = true;

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
    public Font styleFont;
    public int styleFontSize = 12;

    public static bool log;
    public static bool debug;

    static List<HelpEntry> helpEntries;

    static string text = "";
    static string help = "";
    static string message = "";
    static string status = "";
    static string outputSystem = "";
    static string outputApplication = "";
    static string outputScreen = "";
    static string outputFps = "";
    static string output = "";
    static int frameCounter = 0;
    static float timeout = 0F;

    static bool configLoaded = false;
    static Dictionary<string, string> config;



    void Awake()
    {
        log = enableLog;
        debug = showDebug;



        config = new Dictionary<string, string>();
        LoadConfig(configFile);



        // setup ui overlay
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

        style.font = styleFont;
        style.fontSize = styleFontSize;

        // setup help
        helpEntries = new List<HelpEntry>();

        Help("RShift + H", "Toggle Help");
        Help("RShift + D", "Show Debug Information");
        Help("RShift + F", "Toggle Fast Forward");
        Help("RShift + S", "Save Screenshot");
    }



    void Start()
    {
        outputSystem =
            SystemInfo.deviceName + " (" + ((float)SystemInfo.systemMemorySize / 1024F).ToString("0") + "GB) - " +
            SystemInfo.graphicsDeviceName + " (" + ((float)SystemInfo.graphicsMemorySize / 1024F).ToString("0") + "GB)"; //  + " " + Screen.dpi + "dpi " + Screen.orientation;

        outputApplication = Application.productName + " " + (version != "" ? version : "v" + Application.version) + " by " + Application.companyName;

        UnityEngine.Debug.Log(Application.persistentDataPath);

        SetCursor(showMouse);

        StartCoroutine(AutoHide());
    }



    public static void Debug(string _text)
    {
        text += _text;
    }



    public static void Help(string _description)
    {
        helpEntries.Add(new HelpEntry("", _description));
    }



    public static void Help(string _hotkey, string _description)
    {
        helpEntries.Add(new HelpEntry(_hotkey, _description));
    }



    void Update()
    {
        log = enableLog;
        debug = showDebug;

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

        UnityEngine.Debug.Log("Saving screenshot \"" + uri + "\"");

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
        int hotkeyWidth;
        int descriptionWidth;

        int height;

        if (showHelp || intro)
        {
            width = helpWidth;
            height = Screen.height - 32; // helpEntries.Count * (styleFontSize * 2 + 1);
            hotkeyWidth = width / 3;

            //            ("<b>" + outputApplication + "</b>" + status + "\n\n" + help).Trim()

            style.alignment = TextAnchor.UpperLeft;

            GUILayout.BeginArea(new Rect(Screen.width / 2 - width / 2, 16, width, height), style);

            GUILayout.BeginHorizontal();
            GUILayout.Label(outputApplication + " " + status, style);
            GUILayout.EndHorizontal();

            for (int i = 0; i < helpEntries.Count; i++)
            {
                GUILayout.BeginHorizontal();

                if (helpEntries[i].hotkey != "")
                    GUILayout.Label(helpEntries[i].hotkey, style, GUILayout.Width(hotkeyWidth));

                GUILayout.Label(helpEntries[i].description, style);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
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

    static void LoadConfig(string _file)
    {
        // ok, as long as Unity's JsonUtility cannot serialize dictionaries
        // we have to go the long route and rebuild the key/value pair
        // making config files rather ugly.

        string file = System.IO.Path.Combine(Application.streamingAssetsPath, _file);

        if (File.Exists(file))
        {
            UnityEngine.Debug.Log("loading config file \"" + file + "\"");

            string json = File.ReadAllText(file);

            ConfigData configData = JsonUtility.FromJson<ConfigData>(json);

            for (int i = 0; i < configData.data.Length; i++)
            {
                config.Add(configData.data[i].key, configData.data[i].value);
            }

            configLoaded = true;
        }
        else
        {
            UnityEngine.Debug.LogWarning("config file \"" + file + "\" not found");
        }
    }



    public static string GetConfigString(string _key)
    {
        return GetConfigString(_key, "");
    }

    public static string GetConfigString(string _key, string _default)
    { 
        if (config.ContainsKey(_key))
            return config[_key];
        else
            return _default;
    }



    public static float GetConfigFloat(string _key)
    {
        return GetConfigFloat(_key, 0F);
    }

    public static float GetConfigFloat(string _key, float _default)
    {
        if (config.ContainsKey(_key))
            return float.Parse(config[_key]);
        else
            return _default;
    }



    public static int GetConfigInt(string _key)
    {
        return GetConfigInt(_key, 0);
    }

    public static int GetConfigInt(string _key, int _default)
    {
        if (config.ContainsKey(_key))
            return int.Parse(config[_key]);
        else
            return _default;
    }



    public static Vector3 GetConfigVector3(string _key)
    {
        return GetConfigVector3(_key, Vector3.zero);
    }

    public static Vector3 GetConfigVector3(string _key, Vector3 _default)
    {
        if (config.ContainsKey(_key))
        {
            string[] values = config[_key].Split(',');

            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }
        else
        {
            return _default;
        }
    }
}
