using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public string VersionName = "0.1";
    public GameObject UsernameMenu;
    public GameObject ConnectPanel;
    public GameObject Credits;
    public Text CreditsButtonText;
    public GameObject Instructions;
    public Text InstructionsButtonText;

    public InputField UsernameInput;
    public InputField CreateGameInput;
    public InputField JoinGameInput;

    public Button JoinGameButton;
    public Button CreateGameButton;

    public GameObject StartButton;

    public string sceneToLoad = "MemeMe";

    private void Awake()
    {
        PhotonNetwork.ConnectUsingSettings(VersionName);
    }

    // Start is called before the first frame update
    private void Start()
    {
        UsernameMenu.SetActive(true);
    }

    private void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("Connected to master");
    }

    public void ChangeUserNameInput()
    {
        if (UsernameInput.text.Length > 0 && PhotonNetwork.connectionState == ConnectionState.Connected)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    public void ChangeJoinGameInput()
    {
        if (JoinGameInput.text.Length > 0)
        {
            JoinGameButton.interactable = true;
        }
        else
        {
            JoinGameButton.interactable = false;
        }
    }

    public void ChangeCreateGameInput()
    {
        if (CreateGameInput.text.Length > 0)
        {
            CreateGameButton.interactable = true;
        }
        else
        {
            CreateGameButton.interactable = false;
        }
    }

    public void SetUserName()
    {
        UsernameMenu.SetActive(false);
        PhotonNetwork.playerName = UsernameInput.text;
    }

    public void CreateGame()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(CreateGameInput.text, roomOptions, null);
    }

    public void JoinGame()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;
        PhotonNetwork.JoinOrCreateRoom(JoinGameInput.text, roomOptions, TypedLobby.Default);
    }

    public void ToggleCredits()
    {
        CreditsButtonText.text = Credits.activeSelf ? "Credits" : "Hide";
        Credits.SetActive(!Credits.activeSelf);
    }

    public void ToggleInstructions()
    {
        InstructionsButtonText.text = Instructions.activeSelf ? "Instructions" : "Hide";
        Instructions.SetActive(!Instructions.activeSelf);
    }

    private void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(sceneToLoad);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
