using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ImageCropperNamespace
{

    public class GameManager : MonoBehaviour
    {

        public Text playerList;
        public Text roomCode;
        public Text roundNumberText;
        public GameObject voteWaiting;
        public Button startButton;
        public Button submitEntryButton;
        public Slider roundSlider;
        public Slider promptSlider;
        public Slider roundTimerSlider;
        public Slider voteTimerSlider;
        public Slider resultTimerSlider;

        public Canvas lobbyCanvas;
        public Canvas cropCanvas;
        public Canvas promptCanvas;
        public Canvas votingCanvas;
        public Canvas resultsCanvas;
        public Canvas endingCanvas;
        public GameObject submissionPrefab;
        public GameObject playerResultsPrefab;

        public InputField captionInputField;

        public AudioSource lobbyAudio;
        public AudioSource gameAudio;
        public AudioSource voteAudio;

        List<string> players = new List<string>();
        public PhotonView PV;
        public ImageCropperDemo IMD;

        public TextAsset[] textAssets;
        public Texture2D croppedImage;

        float promptDelay = 10f;
        float promptTime;
        bool promptOn = false;

        float roundDelay = 120f;
        float roundTime;
        bool roundOn = false;

        float voteDelay = 40f;
        float voteTime;
        bool voteOn = false;

        float resultDelay = 10f;
        float resultTime;
        bool resultOn = false;

        int roundsToPlay = 1;
        int roundNumber = 0;
        int playersReady = 0;
        int score = 0;

        Dictionary<int, System.Tuple<Texture2D, string>> playerToPostMap = new Dictionary<int, System.Tuple<Texture2D, string>>();
        List<int> playerSubmissions = new List<int>();
        List<GameObject> submissions = new List<GameObject>();
        List<System.Tuple<string, int>> playerToScore = new List<System.Tuple<string, int>>();
        List<GameObject> scores = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            lobbyAudio.Play();
            players.Add(PhotonNetwork.player.NickName);
            UpdatePlayers();
            roomCode.text = "Room Code: " + PhotonNetwork.room.Name;
            if (PhotonNetwork.isMasterClient)
            {
                roomCode.text = "You are the Host\n" + roomCode.text;
            }
            else
            {
                startButton.gameObject.SetActive(false);
                roundSlider.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (promptOn)
            {
                float progress = (Time.time - promptTime) / promptDelay;
                promptSlider.value = (1 - progress);
                if (progress >= 1)
                {
                    promptOn = false;
                    StartRound();
                }
            }

            if (roundOn)
            {
                float progress = (Time.time - roundTime) / roundDelay;
                roundTimerSlider.value = (1 - progress);
                if (progress >= 1)
                {
                    roundOn = false;
                    EndRound();
                }
            }

            if (voteOn)
            {
                float progress = (Time.time - voteTime) / voteDelay;
                voteTimerSlider.value = (1 - progress);
                if (progress >= 1)
                {
                    voteOn = false;
                    EndVote();
                }
            }

            if (resultOn)
            {
                float progress = (Time.time - resultTime) / resultDelay;
                resultTimerSlider.value = (1 - progress);
                if (progress >= 1)
                {
                    resultOn = false;
                    NextRoundOrEnd();
                }
            }
        }

        private void OnMasterClientSwitched(PhotonPlayer newMaster)
        {
            if (PhotonNetwork.isMasterClient)
            {
                roomCode.text = "You are now the Host\n" + roomCode.text;
                startButton.gameObject.SetActive(true);
                roundSlider.gameObject.SetActive(true);
            }
        }

        private void OnPhotonPlayerConnected(PhotonPlayer player)
        {
            players.Add(player.NickName);
            UpdatePlayers();
            if (PhotonNetwork.isMasterClient)
            {
                string allPlayers = SerializePlayers();
                PV.RPC("ReceivePlayers", player, allPlayers);
            }
        }

        private void OnPhotonPlayerDisconnected(PhotonPlayer player)
        {
            players.Remove(player.NickName);
            UpdatePlayers();
        }

        private string SerializePlayers()
        {
            string allPlayers = "";
            foreach (string currentPlayer in players)
            {
                allPlayers += currentPlayer + "\n";
            }
            return allPlayers;
        }


        private void UpdatePlayers()
        {
            playerList.text = "Players in Lobby:\n";
            foreach (string player in players)
            {
                playerList.text += player + "\n";
            }
        }

        [PunRPC]
        void ReceivePlayers(string newPlayers)
        {
            string[] allPlayers = newPlayers.Split('\n');
            foreach (string newPlayer in allPlayers)
            {
                players.Add(newPlayer);
            }
            UpdatePlayers();
        }

        public void StartGame()
        {
            PV.RPC("EveryoneStart", PhotonTargets.All, (int)roundSlider.value);
        }

        [PunRPC]
        void EveryoneStart(int rounds)
        {
            lobbyAudio.Stop();
            roundsToPlay = rounds;
            lobbyCanvas.gameObject.SetActive(false);
            promptCanvas.gameObject.SetActive(true);
            promptSlider.value = 1;
            promptTime = Time.time;
            promptOn = true;
        }


        void StartRound()
        {
            gameAudio.Play();
            roundNumber += 1;
            promptCanvas.gameObject.SetActive(false);
            cropCanvas.gameObject.SetActive(true);
            roundNumberText.text = "Round: " + roundNumber;
            roundTimerSlider.value = 1;
            roundTime = Time.time;
            roundOn = true;
            cropCanvas.GetComponentInChildren<RawImage>().enabled = false;
        }

        public void ReceiveCroppedImage(Texture2D editedImage)
        {
            croppedImage = editedImage;
        }

        public void ButtonEndRound()
        {
            roundOn = false;
            EndRound();
        }

        void EndRound()
        {
            string caption = captionInputField.text;
            voteTimerSlider.value = 1;
            cropCanvas.gameObject.SetActive(false);
            votingCanvas.gameObject.SetActive(true);
            if (PV.isMine)
            {
                PV.RPC("TellHostReadyForVote", PhotonNetwork.masterClient, PhotonNetwork.player, croppedImage, caption);
            }
        }

        [PunRPC]
        void TellHostReadyForVote(PhotonPlayer player, Texture2D cropImage, string caption)
        {
            playersReady++;
            Debug.Log(player.NickName);
            if (cropImage != null)
            {
                Debug.Log("image not null!");
            }
            playerSubmissions.Add(player.ID);
            playerToPostMap.Add(player.ID, new System.Tuple<Texture2D, string>(cropImage, caption));
            if (playersReady >= players.Count)
            {
                playersReady = 0;
                SpawnSubmissions();
                PV.RPC("StartVote", PhotonTargets.All);
            }
        }

        void SpawnSubmissions()
        {
            int numSubmissions = playerToPostMap.Count;
            float interval = (265f * 2f) / (numSubmissions - 1);
            if (interval > 600) interval = 0f;
            Debug.Log("Number of submissions: " + numSubmissions);
            for (int i = 0; i < numSubmissions; ++i)
            {
                GameObject submission = PhotonNetwork.Instantiate("Submission", Vector3.zero, Quaternion.identity, 0);
                if (submission != null)
                {
                    Debug.Log("spawn successful!");
                }
                submission.transform.SetParent(votingCanvas.transform);
                submission.transform.localScale = Vector3.one;
                Debug.Log("interval: " + interval + " and i: " + i + " and the value is: " + (-265f + interval * i));
                submission.transform.localPosition = new Vector3(-265f + interval * i, -70, 0);
                GameObject imageHolder = submission.transform.GetChild(0).gameObject;
                RawImage imHolderRaw = imageHolder.GetComponent<RawImage>();
                imHolderRaw.enabled = true;

                int playerID = playerSubmissions[i];
                Texture2D croppedPost = playerToPostMap[playerID].Item1;
                string caption = playerToPostMap[playerID].Item2;

                imHolderRaw.texture = croppedPost;

                Vector2 size = imHolderRaw.rectTransform.sizeDelta;
                if (croppedPost.height <= croppedPost.width)
                    size = new Vector2(170f, 170f * (croppedPost.height / (float)croppedPost.width));
                else
                    size = new Vector2(170f * (croppedPost.width / (float)croppedPost.height), 170f);
                imHolderRaw.rectTransform.sizeDelta = size;

                GameObject captionObject = submission.transform.GetChild(1).gameObject;
                captionObject.GetComponentInChildren<Text>().text = caption;

                Button button = submission.transform.GetChild(2).gameObject.GetComponent<Button>();
                button.onClick.AddListener(delegate { Vote(playerID); });

                submissions.Add(submission);
            }
        }

        [PunRPC]
        void StartVote()
        {
            // Show submissions
            gameAudio.Stop();
            voteAudio.Play();
            voteWaiting.SetActive(false);
            voteTimerSlider.gameObject.SetActive(true);
            voteOn = true;
            voteTime = Time.time;
        }

        

        [PunRPC]
        void AddVotePoint()
        {
            score++;
        }

        public void Vote(int playerID)
        {
            PhotonPlayer player = PhotonPlayer.Find(playerID);
            PV.RPC("AddVotePoint", player);
            voteOn = false;
            votingCanvas.gameObject.SetActive(false);
            resultsCanvas.gameObject.SetActive(true);
            Debug.Log("My score is: " + score);
            EndVote();
        }

        void EndVote()
        {
            resultsCanvas.transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = "Waiting for everyone to finish voting...";
            if (PV.isMine)
            {
                PV.RPC("TellHostFinishedVoting", PhotonNetwork.masterClient, PhotonNetwork.player, score);
            }
        }

        [PunRPC]
        void TellHostFinishedVoting(PhotonPlayer player, int score)
        {
            playerToScore.Add(new System.Tuple<string, int>(player.NickName, score));
            Debug.Log("Added your score. " + playerToScore.Count + " out of " + players.Count);
            if (playerToScore.Count >= players.Count)
            {
                PV.RPC("ChangeResultsHeader", PhotonTargets.All);
                CleanUpVoteScene();
                ShowScores();
            }
        }

        [PunRPC]
        void ChangeResultsHeader()
        {
            resultsCanvas.transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = "Results after round " + roundNumber;
        }

        void CleanUpVoteScene()
        {
            for (int i = 0; i < submissions.Count; ++i)
            {
                PhotonNetwork.Destroy(submissions[i]);
            }
            submissions.Clear();
            playerToPostMap.Clear();
            playerSubmissions.Clear();
        }

        void ShowScores()
        {
            GameObject Results = votingCanvas.transform.GetChild(2).gameObject;
            // List<System.Tuple<string, int>> playerToScore = new List<System.Tuple<string, int>>();
            playerToScore.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            int len = players.Count;
            Debug.Log("amount of players: " + len);
            for (int i = len - 1; i >= 0; --i)
            {
                Debug.Log("Entered loop");
                GameObject resultInstance = PhotonNetwork.Instantiate("PlayerResult", Vector3.zero, Quaternion.identity, 0);
                if (resultInstance != null) Debug.Log("Spawned instance successfully");
                resultInstance.transform.SetParent(resultsCanvas.transform);
                //resultInstance.transform.SetParent(Results.transform);
                resultInstance.transform.localScale = Vector3.one;
                resultInstance.transform.localPosition = new Vector3(0, 110 - 90 * (-i + len - 1), 0);
                resultInstance.transform.GetChild(1).GetComponentInChildren<Text>().text = playerToScore[i].Item1;
                resultInstance.transform.GetChild(2).GetComponentInChildren<Text>().text = playerToScore[i].Item2.ToString();

                scores.Add(resultInstance);
            }
            playerToScore.Clear();
            PV.RPC("StartResultsState", PhotonTargets.All);
        }

        [PunRPC]
        void StartResultsState()
        {
            voteAudio.Stop();
            resultOn = true;
            resultTime = Time.time;
            resultTimerSlider.gameObject.SetActive(true);
            resultTimerSlider.value = 1;
        }

        void NextRoundOrEnd()
        {
            if (PhotonNetwork.isMasterClient)
            {
                for (int i = 0; i < scores.Count; ++i)
                {
                    PhotonNetwork.Destroy(scores[i]);
                }
                scores.Clear();
            }
            if (roundNumber == roundsToPlay)
            {
                resultsCanvas.gameObject.SetActive(false);
                endingCanvas.gameObject.SetActive(true);
            } else if (roundNumber < roundsToPlay) {
                resultsCanvas.gameObject.SetActive(false);
                StartRound();
            } else
            {
                Debug.LogError("Round count exceeded expected rounds. Should never happen");
            }

        }

        public void Crop()
        {
            IMD.Crop(textAssets[roundNumber - 1]);
        }
    }
}
