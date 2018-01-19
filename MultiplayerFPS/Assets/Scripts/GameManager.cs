using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public MatchSettings matchSettings;

    [SerializeField]
    private GameObject sceneCam;

    void Awake ()
    {
        // ensuring only one instance of GameManager exists
        if (instance != null) Debug.LogError("More than one GameManager in scene");
        else instance = this;

    }

    public void SetSceneCameraActive(bool _isActive)
    {
        if (sceneCam == null) return;

        sceneCam.SetActive(_isActive);
    }

    #region Player registering

    private const string PLAYER_ID_PREFIX = "Player ";

    // the players currently on the network
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();


	public static void RegisterPlayer (string _netID, Player _player)
    {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
    }

    public static void UnregisterPlayer (string _playerID)
    {
        players.Remove(_playerID);
    }

    public static Player GetPlayer (string _playerID)
    {
        return players[_playerID];
    }



    // for visualizing current players
    //void OnGUI ()
    //{
    //    GUILayout.BeginArea(new Rect(200, 200, 300, 500));
    //    GUILayout.BeginVertical();

    //    foreach (string _id in players.Keys)
    //        GUILayout.Label(_id + "  -  " + players[_id].transform.name);

    //    GUILayout.EndVertical();
    //    GUILayout.EndArea();
    //}

    #endregion
}
