using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkRunnerHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunner NetworkRunnerPrefab;
    private NetworkRunner NetworkRunner;

    void Awake()
    {
        NetworkRunner = FindObjectOfType<NetworkRunner>();
    }

    async void Start()
    {
        if (NetworkRunner == null)
        {
            NetworkRunner = Instantiate(NetworkRunnerPrefab);
            NetworkRunner.name = "Network Runner";

            await InitializeNetworkRunner(
                NetworkRunner,
                GameMode.AutoHostOrClient,
                "TestSession",
                NetAddress.Any(),
                SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                null
            );

            Utils.DebugLog("InitializeNetworkRunner called");
        }
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        var sceneManager = runner.GetComponents<MonoBehaviour>()
                                 .OfType<INetworkSceneManager>()
                                 .FirstOrDefault();

        if (sceneManager == null)
        {
            // Handle networked objects that already exist in the scene
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }

    protected virtual Task InitializeNetworkRunner(
        NetworkRunner runner,
        GameMode gameMode,
        string sessionName,
        NetAddress address,
        SceneRef scene,
        Action<NetworkRunner> initialized
    )
    {
        INetworkSceneManager sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;

        initialized?.Invoke(runner);

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            SceneManager = sceneManager
        });
    }
}