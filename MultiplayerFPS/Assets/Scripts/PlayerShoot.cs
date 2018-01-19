using UnityEngine.Networking;
using UnityEngine;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;


	void Start ()
    {
        if (cam == null)
        {
            Debug.LogError("Playershoot: No camera referenced");
            this.enabled = false;
        }
        weaponManager = GetComponent<WeaponManager>();
	}
	
	void Update ()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (currentWeapon.fireRate <= 0f)
        {
		    if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1/currentWeapon.fireRate);
            } else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }
	}

    // called on all clients to show a shoot effect
    [ClientRpc]
    void RpcDoShootEffect ()
    {
        weaponManager.GetCurrentWeaponGraphics().muzzleFlash.Play();
    }


    // called on the server when shot hits a surface
    [Command]
    void CmdOnHit (Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    // Spawn hit effects on each client
    [ClientRpc]
    void RpcDoHitEffect (Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentWeaponGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 0.5f);
    }

    // called on the server when player shoots
    [Command]
    void CmdOnShoot ()
    {
        RpcDoShootEffect();
    }

    // Method is only called on the client machine, never on the server
    // shooting happens on the client side only, and damage-taking happens on the server
    [Client]
    void Shoot ()
    {
        if (!isLocalPlayer) return;
        
        Debug.Log("Shot");

        // call on shoot method on the server
        CmdOnShoot();

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            // hit something
            Debug.Log("We hit " + _hit.collider.name);

            // if hit a player, tell the server which player was hit
            if (_hit.collider.tag == "Player")
            {
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage);
            }

            // tell server to do hit effects, regardless of what is hit
            CmdOnHit(_hit.point, _hit.normal);
        }
    }

    // Commands are methods that are called only on the server
    [Command]
    void CmdPlayerShot (string _playerID, int _damage)
    {
        Debug.Log(_playerID + " has been shot");

        // damage the correct player object
        Player  _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }
}
