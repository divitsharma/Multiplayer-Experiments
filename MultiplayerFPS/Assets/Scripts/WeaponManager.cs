using UnityEngine.Networking;
using UnityEngine;

public class WeaponManager : NetworkBehaviour {

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerWeapon primaryWeapon;

    private PlayerWeapon currentWeapon; // weapon data
    private WeaponGraphics currentGraphics;

	void Start ()
    {
        EquipWeapon(primaryWeapon);
	}

    void EquipWeapon (PlayerWeapon _weapon)
    {
        currentWeapon = _weapon;

        // Instantiate weapon graphics under the weapon holder and set its layer as appropriate
        GameObject _weaponInst = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        _weaponInst.transform.SetParent(weaponHolder);

        currentGraphics = _weaponInst.GetComponent<WeaponGraphics>();
        if (currentGraphics == null) Debug.LogError("No WeaponGraphics component on the weapon.");

        if (isLocalPlayer)
        {
            Utility.SetLayerRecursively(_weaponInst, LayerMask.NameToLayer(weaponLayerName));
        }
    }

	public PlayerWeapon GetCurrentWeapon ()
    {
        return currentWeapon;
    }
    public WeaponGraphics GetCurrentWeaponGraphics ()
    {
        return currentGraphics;
    }
}
