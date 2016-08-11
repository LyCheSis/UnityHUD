using UnityEngine;
using System.Collections;

public class UnityHUD : MonoBehaviour
{
    GUIStyle style;
    static string text = "";
    static string systemInfo = "";
    static string applicationInfo = "";
    static string fps = "";
    static int counter = 0;
    static float timeout = 0F;

    void Start()
    {
        style = new GUIStyle();
        style.normal.textColor = Color.white;
        //        style.normal.background = Color.black;
        style.padding = new RectOffset(4, 4, 4, 4);

        systemInfo = SystemInfo.deviceName + " (" + ((float)SystemInfo.systemMemorySize / 1024F).ToString("0") + "GB) - " + SystemInfo.graphicsDeviceName + " (" + ((float)SystemInfo.graphicsMemorySize / 1024F).ToString("0") + "GB)";
        applicationInfo = Application.productName + " v" + Application.version + " by " + Application.companyName;

    }

    public static void Log(string _text)
    {
        text += _text;
    }

    void Update()
    {
        text = "";

        if (timeout > 0F)
        {
            timeout -= Time.deltaTime;
            counter++;
        }

        if (timeout <= 0F)
        {
            fps = " - " + counter.ToString() + "fps (" + (Time.deltaTime * 1000).ToString("0.0") + "ms)\n";
            timeout = 1F;
            counter = 0;
        }
    }

    void OnGUI()
    {
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(new Rect(0, 0, Screen.width, 160), systemInfo + fps + text, style);

        style.alignment = TextAnchor.LowerRight;
        GUI.Box(new Rect(0, Screen.height - 160, Screen.width, 160), applicationInfo, style);
    }
}
