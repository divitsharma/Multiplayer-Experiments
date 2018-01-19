using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    private string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";
    [SerializeField]
    GameObject playerGraphics;

    [SerializeField]
    GameObject playerUIPrefab;
    //[HideInInspector]
    public GameObject playerUIInstance;

      void Start ()
    {
        // if this object is not the local player of this client
        if (!isLocalPlayer)
        {
            // disabling movement listeners for non-local players
            DisableComponents();
            // all players are on local player layer by default, change if not local player
            AssignRemoteLayer();
        }
        else
        {
            // Disable local player graphics
            Utility.SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

            // Create player UI
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            // Configure player UI
            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if (ui == null) Debug.LogError("No playerui component on playerui prefab");
            ui.SetPlayerController(GetComponent<PlayerController>());

            GetComponent<Player>().SetupPlayer();
        }

    }

    // called when client is set up locally
    public override void OnStartClient ()
    {
        base.OnStartClient();

        string _netId = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();
        GameManager.RegisterPlayer(_netId, _player);
    }

    void AssignRemoteLayer ()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void DisableComponents ()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    // called when this component is disabled
    void OnDisable ()
    {
        if (isLocalPlayer)
            GameManager.instance.SetSceneCameraActive(true);

        // Unregister
        GameManager.UnregisterPlayer(transform.name);

        // Destroy the playerUI
        Destroy(playerUIInstance);
    }

}
