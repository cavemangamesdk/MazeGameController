using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _consoleTxt;
    [SerializeField] private TextMeshProUGUI _statusTxt;

    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _disconnectButton;
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TMP_InputField _portInputField;

    [SerializeField] private GameObject _connectionParent;
    
    private string _url;
    private string _dataToSend;

    private bool _sendData;

    private Socket _socket;
    private IPEndPoint _endPoint;
    private IPAddress _ipAddress;

    private void Awake()
    {
        _ipInputField.onValidateInput += delegate(string s, int i, char c)
        {
            if (s.Length >= 15)
            {
                return '\0';
            }

            return char.ToUpper(c);
        };
    }

    private void Start()
    {
        Input.gyro.enabled = true;
        Input.gyro.updateInterval = 0.01f;
        _connectButton.onClick.AddListener(Connect);
        _disconnectButton.onClick.AddListener(Disconnect);
    }

    private void Update()
    {
        if (_socket == null) return;
        
        HandleData();
        HandleUI();
    }

    private void HandleUI()
    {
        _statusTxt.text = _socket.Connected ? "CONNECTED" : "DISCONNECTED";
    }

    private void HandleData()
    {
        // var gyroInput = Input.gyro.attitude.eulerAngles;
        //
        // var xAxis = (gyroInput.x >= 180) ? gyroInput.x - 360 : gyroInput.x;
        // var yAxis = (gyroInput.y >= 180) ? gyroInput.y - 360 : gyroInput.y;
        //
        // xAxis = (float)Math.Round(xAxis,2);
        // yAxis = (float)Math.Round(yAxis,2);
        //
        // xAxis = -xAxis;
        
        float xAxis = Mathf.Atan2(Input.gyro.gravity.x, -Input.gyro.gravity.z) * Mathf.Rad2Deg;
        float yAxis = Mathf.Atan2(Input.gyro.gravity.y, -Input.gyro.gravity.z) * Mathf.Rad2Deg;

        // Adjust the tilt values to range from -180 to 180 degrees
        xAxis = (xAxis > 180f) ? xAxis - 360f : xAxis;
        yAxis = (yAxis > 180f) ? yAxis - 360f : yAxis;

        yAxis = -yAxis;
        
        var data = $"{yAxis},{xAxis}";
        byte[] dataToSend = Encoding.ASCII.GetBytes(data);
        _socket.Send(dataToSend);
    }

    private void Connect()
    {
        var ipString = _ipInputField.text;
        var portString = _portInputField.text;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        
        _ipAddress = IPAddress.Parse(ipString);
        _endPoint = new IPEndPoint(_ipAddress, 5100);
        
        _socket.Connect(_endPoint);
        
        _disconnectButton.gameObject.SetActive(true);
        _connectionParent.SetActive(false);
    }

    private void Disconnect()
    {
        if (_socket.Connected == true)
            _socket.Close();

        _ipInputField.text = string.Empty;
        _portInputField.text = String.Empty;
        
        _connectionParent.SetActive(true);
        _disconnectButton.gameObject.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        _socket.Close();
    }
}
