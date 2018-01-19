using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerSetup))]
[RequireComponent(typeof(PlayerController))]
public class Player : NetworkBehaviour {

    [SerializeField]
    private int maxHealth = 100;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disabledObjectsOnDeath;

    [SerializeField]
    private GameObject deathEffect;

    [SerializeField]
    private GameObject spawnEffect;

    // damage is taken on the server, so need to sync health across all clients
    [SyncVar]
    private int currHealth;

    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    private bool firstSetup = true;

    // called from PlayerSetup script
    public void SetupPlayer ()
    {
        if (isLocalPlayer)
        {
            // Switch cameras
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }

        CmdBroadcastNewPlayerSetup();   
    }

    [Command]
    private void CmdBroadcastNewPlayerSetup ()
    {
        RpcSetupPlayerOnAllClients();
    }
    [ClientRpc]
    private void RpcSetupPlayerOnAllClients ()
    {
        if (firstSetup)
        {
            // determine which components were enabled on this player after PlayerSetup
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
                wasEnabled[i] = disableOnDeath[i].enabled;

            firstSetup = false;
        }

        SetDefaults();
    }

    // for debugging
    //void Update()
    //{
    //    if (isLocalPlayer)
    //    {
    //        if (Input.GetKeyDown(KeyCode.K))
    //        {
    //            RpcTakeDamage(40);
    //        }
    //    }
    //}

    // TakeDamage is called on all clients
    [ClientRpc]
    public void RpcTakeDamage (int _amt)
    {
        if (isDead) return;

        currHealth -= _amt;
        Debug.Log(transform.name + " now has " + currHealth + " health."); // only on server

        if (currHealth <= 0)
        {
            Die();
        }
    }

    void Die ()
    {
        isDead = true;

        // Disable components on player to disallow movement, etc
        foreach (Behaviour b in disableOnDeath)
            b.enabled = false;

        // Disable objects
        foreach (GameObject g in disabledObjectsOnDeath)
            g.SetActive(false);

        //Collider _col = GetComponent<Collider>();
        //if (_col != null) _col.enabled = false;

        // Spawn death effect
        GameObject _gfxInst = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_gfxInst, 3f);

        // (don't) switch cameras
        if (isLocalPlayer)
        {
            //GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        // disable the spring joint and enable rigidbody gravity
        GetComponent<PlayerController>().SetJointSettings(0f);
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().freezeRotation = false;

        Debug.Log(transform.name + " is DEAD");

        // Respawn
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn ()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        // get one of the registered spawn points
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        // Switch cameras
        GameManager.instance.SetSceneCameraActive(false);
        GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);

        // error margin
        yield return new WaitForSeconds(0.5f);

        SetupPlayer();

        Debug.Log(transform.name + " respawned");
    }

    public void SetDefaults ()
    {
        isDead = false;

        currHealth = maxHealth;

        // enable the components that were disabled on death
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        foreach (GameObject g in disabledObjectsOnDeath)
            g.SetActive(true);

        //Collider _col = GetComponent<Collider>();
        //if (_col != null) _col.enabled = true;

        // Create spawn effects
        GameObject _gfxInst = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(_gfxInst, 3f);

        // enable the spring joint and enable rigidbody gravity
        GetComponent<PlayerController>().ResetJointSettings();
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().freezeRotation = true;
    }
}
