using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Temporary UI for testing network connections.
/// Attach to a Canvas with Host/Client/Stop buttons.
/// </summary>
public class NetworkTestUI : MonoBehaviour
{
    [Header("References")]
    public ConnectionManager connectionManager;

    [Header("Buttons")]
    public Button hostButton;
    public Button clientButton;
    public Button stopButton;

    [Header("Status")]
    public TextMeshProUGUI statusText;

    private void Start()
    {
        if (connectionManager == null)
        {
            connectionManager = FindObjectOfType<ConnectionManager>();
        }

        // Wire up buttons
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostClicked);
        
        if (clientButton != null)
            clientButton.onClick.AddListener(OnClientClicked);
        
        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopClicked);

        UpdateStatus("Ready - Click Host or Client");
    }

    private void OnHostClicked()
    {
        connectionManager?.StartHost();
        UpdateStatus("Started as Host");
    }

    private void OnClientClicked()
    {
        connectionManager?.StartClient();
        UpdateStatus("Connecting as Client...");
    }

    private void OnStopClicked()
    {
        connectionManager?.StopConnection();
        UpdateStatus("Stopped");
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        
        Debug.Log($"[NetworkTestUI] {message}");
    }
}
