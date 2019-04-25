﻿using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    /* Game objects to be instantiated on the network */
    public GameObject interloperGO;

    /* Local Selected Character. 
     * Needs to keep this always updated, since it's a single source of infomation about the character. */
    private CBaseManager selectedCharacter;

    #region Photon Callbacks

    /* Called when local player left the room. We load the lobby then. */
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        LoadArena();
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        LoadArena();
    }

    #endregion


    void Start()
    {
        Instance = this;
        InstantiateCharacters();
    } 

    private void InstantiateCharacters()
    {
        if (Interloper.LocalInterloperInstance == null) {
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            interloperGO = PhotonNetwork.Instantiate(this.interloperGO.name, BoardManager.Instance.GetVectorFromTileId((byte)PhotonNetwork.LocalPlayer.ActorNumber), Quaternion.identity, 0);
            // #critical we use the ACTOR NUMBER to set spawn positions. TODO change that in the future!!
        }
    }

    /* Loads the MainBoard level if we are master client. */
    private void LoadArena()
    {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.LoadLevel(Consts.MAIN_MAP);
        }
    }

    #region Public methods

    /* A wrapper on PhotonNetwork.LeaveRoom(). We might need to do more logic when players leave.*/
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /* If we are allowed to move there -> Fire an event. */
    public void MovePlayerToTile(Tile selectedTile)
    {
        if (selectedCharacter != null) {
            Debug.Log("Selected tileid: " + selectedTile.Id + " - IsMyTurn: " + TurnManager.Instance.IsMyTurn() + " - isOccupied: " + selectedTile.isOccupied + " - canMove: " + selectedCharacter.State.CanMove);

            if (TurnManager.Instance.IsMyTurn()
                && !selectedTile.isOccupied
                && selectedCharacter.State.CanMove) {

                EventHub.Instance.FireEvent(new TileSelectedEvent(selectedTile));
                //Deselect the character.
                selectedCharacter.State.CharacterMoved();
            }
        }
    }

    public void SelectPlayer(CBaseManager characterHit)
    {
        EventHub.Instance.FireEvent(new CharacterSelectedEvent(characterHit.Properties.CharacterID));
        selectedCharacter = characterHit;
    }

    public CBaseManager GetSelectedCharacter()
    {
        return selectedCharacter;
    }

    #endregion
}