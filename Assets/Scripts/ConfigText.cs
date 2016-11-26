using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConfigText : MonoBehaviour
{
    public string key;

    private Text text;



    void Awake()
    {
        text = GetComponent<Text>();

        if (text == null)
        {
            Debug.LogError(name + " has no Text component");
            return;
        }

        GetConfig();
    }



    void Apply(string _value)
    {
        text.text = _value;
    }



    void GetConfig()
    {
        string value = UnityHUD.GetConfigString(key);

        if (value != null)
            Apply(value);
    }



    void OnEnable()
    {
        UnityHUD.OnConfigLoaded += GetConfig;
    }



    void OnDisable()
    {
        UnityHUD.OnConfigLoaded -= GetConfig;
    }
}
