using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

public class UnityHUD : MonoBehaviour
{
    public delegate void ConfigLoaded();
    public static event ConfigLoaded OnConfigLoaded;

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

    [System.Serializable]
    public class ConfigSource
    {
        public string name;
        public string prefix;
        public string remoteFile;
        public string localFile;
        public bool loaded;
    }

    [System.Serializable]
    public class Resolution
    {
        public string name;
//        public bool force;    reserved for future use
        public Vector2 size;
        public bool windowed;
        public int refreshRate;
    }


    [Tooltip("UI Text element to display messages on. Leave empty to use GUI")]
    public Text debugText;

    [Tooltip("Application version. If left empty the mobile version number is used.")]
    public string version = "";

    [Tooltip("JSON file with configuration data.")]
    public ConfigSource[] configFiles;

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
    static bool pauseActive = false;
    static float timeScaleBackup = 1F;

    [Tooltip("Width of help overlay")]
    public int helpWidth = 320;

    GUIStyle style;
    public Font styleFont;
    public int styleFontSize = 12;

    public static bool log;
    public static bool debug;

    public Resolution[] resolutions;
    public int currentResolution = 0;

    static List<HelpEntry> helpEntries;

    static string text = "";
    static string help = "";
    static string message = "";
//    static string log = "";
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

    static float noticeTimeout = 0F;
    static string noticeText = "";



    void Awake()
    {
        log = enableLog;
        debug = showDebug;



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
        Help("RShift + P", "Toggle Pause");
        Help("RShift + S", "Save Screenshot");
        Help("RShift + R", "Switch Resolution");
        Help("RShift + ,", "Decrease Quality");
        Help("RShift + .", "Increase Quality");
    }



    void Start()
    {
        // load config here to allow other scripts to subscribe to OnConfigLoaded events via the OnEnable function
        config = new Dictionary<string, string>();

        for (int i = 0; i < configFiles.Length; i++)
        {
            configFiles[i].loaded = LoadConfig(configFiles[i].localFile, configFiles[i].prefix);
        }


        outputSystem = SystemInfo.deviceName + " " + string.Join("/", GetHostAddresses());

        outputSystem += " (" + ((float)SystemInfo.systemMemorySize / 1024F).ToString("0") + "GB) - " +
            SystemInfo.graphicsDeviceName + " (" + ((float)SystemInfo.graphicsMemorySize / 1024F).ToString("0") + "GB)"; //  + " " + Screen.dpi + "dpi " + Screen.orientation;

        outputApplication = Application.productName + " " + (version != "" ? version : "v" + Application.version) + " by " + Application.companyName;

        UnityEngine.Debug.Log(Application.persistentDataPath);

        // hidden mouse pointer in the editor is pretty annoying so don't do it.
        #if !UNITY_EDITOR
            SetCursor(showMouse);
        #endif



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



    public static void Notice(string _description)
    {
        noticeText += _description + "\n";
        noticeTimeout = 4F;
    }



    void Update()
    {
        log = enableLog;
        debug = showDebug;



        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
                MakeScreenshot();

            if (Input.GetKeyDown(KeyCode.F))
                ToggleFastForward();

            if (Input.GetKeyDown(KeyCode.P))
                TogglePause();

            if (Input.GetKeyDown(KeyCode.M))
                ToggleCursor();

            if (Input.GetKeyDown(KeyCode.N))
                Notice("Test notice!");

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

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (resolutions.Length > 0)
                {
                    currentResolution++;

                    if (currentResolution >= resolutions.Length)
                        currentResolution = 0;

                    Screen.SetResolution(
                        Mathf.RoundToInt(resolutions[currentResolution].size.x),
                        Mathf.RoundToInt(resolutions[currentResolution].size.y),
                        resolutions[currentResolution].windowed,
                        resolutions[currentResolution].refreshRate
                    );
                }
            }

            if (Input.GetKeyDown(KeyCode.Period))
            {
                QualitySettings.IncreaseLevel(true);
            }

            if (Input.GetKeyDown(KeyCode.Comma))
            {
                QualitySettings.DecreaseLevel(true);
            }
        }



        if (showDebug)
        {
            if (timeout > 0F)
            {
                timeout -= Time.deltaTime;
                frameCounter++;
            }

            if (timeout <= 0F)
            {
                outputFps = "<color=" + (frameCounter < Screen.currentResolution.refreshRate ? "#AA0000" : "#00AA00") + ">" + frameCounter.ToString() + "fps (" + (Time.deltaTime * 1000).ToString("0.0") + "ms)</color>";
                outputScreen = Screen.width + "x" + Screen.height;
                timeout = 1F;
                frameCounter = 0;
            }

            status += " Q:" + QualitySettings.GetQualityLevel();

            if (fastForwardActive)
                status += " FF:" + fastForwardMultiplier + "x";

            if (pauseActive)
                status += " P";
        }



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
        string uri = Application.productName + "_" + System.DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".png";

        UnityEngine.Debug.Log("Saving screenshot \"" + uri + "\"");

        Application.CaptureScreenshot(uri, screenshotScale);
    }



    public static void MakeScreenshot(int _screenshotScale)
    {
        string uri = Application.productName + "_" + System.DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".png";

        UnityEngine.Debug.Log("Saving screenshot \"" + uri + "\"");

        Application.CaptureScreenshot(uri, _screenshotScale);
    }



    void ToggleFastForward()
    {
        if (!fastForwardActive)
        {
            if (!pauseActive)
                timeScaleBackup = Time.timeScale;

            Time.timeScale = fastForwardMultiplier;
            fastForwardActive = true;
            pauseActive = false;
        }
        else
        {
            Time.timeScale = timeScaleBackup;
            fastForwardActive = false;
            pauseActive = false;
        }
    }



    void TogglePause()
    {
        if (!pauseActive)
        {
            if (!fastForwardActive)
                timeScaleBackup = Time.timeScale;

            Time.timeScale = 0F;
            pauseActive = true;
            fastForwardActive = false;
        }
        else
        {
            Time.timeScale = timeScaleBackup;
            pauseActive = false;
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
        Rect rect;
        GUIContent content;
        int width;
        int hotkeyWidth;

        int height;

        GUI.skin.label = style;
        GUI.skin.window = style;

        if (showDebug)
        {
            content = new GUIContent(("<b>" + outputApplication + "</b> " + outputSystem + " - " + outputScreen + " - " + outputFps + status + "\n" + text).Trim());

            width = Screen.width;
            height = Mathf.RoundToInt(style.CalcHeight(content, (float)width));

            rect = new Rect(0, 0, width, height);
            GUI.Box(rect, content, style);
        }

        if (showHelp || intro)
        {
            width = helpWidth;
            height = 0;

            hotkeyWidth = width / 3;

            rect = new Rect(Screen.width / 2 - width / 2, 16, width, height);
            rect = GUILayout.Window(0, rect, HelpWindow, "", GUILayout.Width(width));
        }

        if (noticeTimeout > 0F)
        {
            content = new GUIContent((noticeText).Trim());

            width = helpWidth;
            height = 0;

            noticeTimeout -= Time.deltaTime;
            if (noticeTimeout <= 0F)
            {
                noticeTimeout = 0F;
                noticeText = "";
            }

            width = helpWidth;
            height = Mathf.RoundToInt(style.CalcHeight(content, (float)width));

            rect = new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
            GUI.Box(rect, content, style);
        }
    }



    void HelpWindow(int _windowId)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(outputApplication + " " + status);
        GUILayout.EndHorizontal();

        for (int i = 0; i < helpEntries.Count; i++)
        {
            GUILayout.BeginHorizontal();

            if (helpEntries[i].hotkey != "")
                GUILayout.Label(helpEntries[i].hotkey); // , style, GUILayout.Width(160));

            GUILayout.Label(helpEntries[i].description);
            GUILayout.EndHorizontal();
        }
    }



    static bool LoadConfg(string _file)
    {
        return LoadConfig(_file, "");
    }



    static bool LoadConfig(string _file, string _prefix)
    {
        // ok, as long as Unity's JsonUtility cannot serialize dictionaries
        // we have to go the long route and rebuild the key/value pair
        // making config files rather ugly.

        if (_file.Substring(0, 4) == "http")
        {
            
        }
        else
        {
            string file = System.IO.Path.Combine(Application.streamingAssetsPath, _file);

            if (File.Exists(file))
            {
                UnityEngine.Debug.Log("loading config file \"" + file + "\"");

                string json = File.ReadAllText(file);

                ConfigData configData = JsonUtility.FromJson<ConfigData>(json);

                for (int i = 0; i < configData.data.Length; i++)
                {
                    config.Add(_prefix + configData.data[i].key, configData.data[i].value);
                }

                if (OnConfigLoaded != null)
                    OnConfigLoaded();

                return true;
            }
            else
            {
                UnityEngine.Debug.LogWarning("config file \"" + file + "\" not found");
            }
        }

        return false;
    }



    public static IEnumerator LoadWebConfig(string _url, string _prefix)
    {
        UnityEngine.Debug.Log("loading config file \"" + _url + "\"");

        WWW www = new WWW(_url);

        yield return www;

        ConfigData configData = JsonUtility.FromJson<ConfigData>(www.text);

        for (int i = 0; i < configData.data.Length; i++)
        {
            config.Add(_prefix + configData.data[i].key, configData.data[i].value);
        }
    }



    public static string GetConfigString(string _key)
    {
        if (config == null)
            return null;

        if (config.ContainsKey(_key))
            return config[_key];
        else
            return null;
    }



    public static string GetConfigString(string _key, string _default)
    {
        if (config == null)
            return null;

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



    public static string[] GetHostAddresses()
    {
        try
        {
            string hostName = Dns.GetHostName();

            outputSystem = hostName;

            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);

            List<string> addresses = new List<string>();

            if (hostAddresses.Length > 0)
            {
                for (int i = 0; i < hostAddresses.Length; i++)
                {
                    switch (hostAddresses[i].AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            addresses.Add(hostAddresses[i].ToString());
                            break;
                    }
                }
            }

            return addresses.ToArray();
        }
        catch (System.Exception e)
        {
            return new string[0];

//            Console.WriteLine("Exception caught!!!");
//            Console.WriteLine("Source : " + e.Source);
//            Console.WriteLine("Message : " + e.Message);
        }
    }
}
