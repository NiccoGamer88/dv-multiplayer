using System;
using System.Collections;
using DV.UserManagement;
using Multiplayer.Networking.Packets.Clientbound;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Multiplayer.Components.SaveGame;

public class StartGameData_ServerSave : AStartGameData
{
    private SaveGameData saveGameData;

    private Vector3 position;
    private Vector3 rotation;

    public void SetFromPacket(ClientboundSaveGameDataPacket packet)
    {
        saveGameData = SaveGameManager.MakeEmptySave();
        saveGameData.SetString("World", packet.World);
        saveGameData.SetString("Game_mode", packet.GameMode);
        DifficultyToUse = DifficultyDataUtils.GetDifficultyFromJSON(JObject.Parse(packet.SerializedDifficulty), false);

        saveGameData.SetFloat("Player_money", packet.Money);

        saveGameData.SetStringArray("Licenses_Jobs", packet.AcquiredJobLicenses);
        saveGameData.SetStringArray("Licenses_General", packet.AcquiredGeneralLicenses);
        saveGameData.SetStringArray("Garages", packet.UnlockedGarages);

        position = packet.Position;
        rotation = new Vector3(0, packet.Rotation, 0);

        saveGameData.SetBool("Tutorial_01_completed", true);
        saveGameData.SetBool("Tutorial_02_completed", true);
        saveGameData.SetBool("Tutorial_03_completed", true);
    }

    public override void Initialize()
    {
        throw new InvalidOperationException($"Use {nameof(SetFromPacket)} instead!");
    }

    public override SaveGameData GetSaveGameData()
    {
        if (saveGameData == null)
            throw new InvalidOperationException($"{nameof(SetFromPacket)} must be called before {nameof(GetSaveGameData)}!");
        return saveGameData;
    }

    public override IEnumerator DoLoad(Transform playerContainer)
    {
        Transform playerTransform = playerContainer.transform;
        playerTransform.position = position + WorldMover.currentMove;
        playerTransform.eulerAngles = rotation + WorldMover.currentMove;

        if (saveGameData.GetString("Game_mode") == "FreeRoam")
            LicenseManager.Instance.GrabAllUnlockables();
        else
            StartingItemsController.Instance.AddStartingItems(saveGameData, true);
        carsAndJobsLoadingFinished = true;
        yield break;
    }

    public override string GetPostLoadMessage()
    {
        return null;
    }

    public override bool ShouldCreateSaveGameAfterLoad()
    {
        return false;
    }
}