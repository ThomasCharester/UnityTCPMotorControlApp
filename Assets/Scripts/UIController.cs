using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    void Awake()
    {
        //ConnectToServer();
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance == this)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] GameObject Connect;
    [SerializeField] GameObject Control;

    [SerializeField] GameObject Addresses;
    [SerializeField] TMP_Dropdown AddressesDrop;
    List<string> AddressesList = new List<string>();

    [SerializeField] TextMeshProUGUI target;

    [SerializeField] TextMeshProUGUI error;
    [SerializeField] float hideTime = 1.0f;

    //ÊÎÑÒÛËÜÈ
    public string currentAngle = "";
    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();

    void Start()
    {
        EspRecieveSender.Instance.ServerConnectionEvent += ConnectionProcess;
        EspRecieveSender.Instance.GetDataFromServerEvent += UpdateCurrentAngle;
        LoadAddresses();

        ci.NumberFormat.CurrencyDecimalSeparator = ".";
    }
    void Update()
    {
        if (Control.activeSelf) target.text = currentAngle;
    }
    void UpdateCurrentAngle(string angle) 
    {
        currentAngle = angle.Remove(angle.IndexOf('\0'));
    }
    public void ChangeTarget(float count)
    {
        EspRecieveSender.Instance.SendMessageToServer("c" + count.ToString().Replace(',','.') + '$');
    }
    void ConnectionProcess(bool connectionStatus, string address)
    {
        SwapUI(connectionStatus);
        SaveAddress(address);
    }

    private void SwapUI(bool connectionStatus)
    {
        Connect.SetActive(!connectionStatus);
        Control.SetActive(connectionStatus);
    }

    public void ShowError(string errorText)
    {
        error.text = errorText;
        Invoke(nameof(HideError), hideTime);
    }
    public void HideError() => error.text = null;
    void LoadAddresses()
    {
        for (int i = 0; PlayerPrefs.HasKey(i.ToString()); i++)
            AddressesList.Add(PlayerPrefs.GetString(i.ToString()));

        if (AddressesList == null || AddressesList.Count <= 0) return;

        Addresses.SetActive(true);
        AddressesDrop.AddOptions(AddressesList);
    }
    void SaveAddress(string address)
    {
        if (AddressesList.Contains(address) || address == null) return;
        
        PlayerPrefs.SetString(AddressesList.Count.ToString(), address);
        AddressesList.Add(address);
    }
    public List<string> GetAddresses() { return AddressesList; }

    public void ConnectSelected()
    {
        if (AddressesDrop.value <= 0) return;
        EspRecieveSender.Instance.SetIpAddress(AddressesDrop.options[AddressesDrop.value].text);
        EspRecieveSender.Instance.StartConnection();
    }
}
