using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomData", menuName = "Faculty/Room Data")]

public class RoomData : ScriptableObject
{
    [Header("Room Identification")]
    public string roomID;
    public string roomName;

    [Header("Location Info")]
    public string buildingSection;
    public string floor;

    [Header("Details & Description")]
    [TextArea(3, 6)]
    public string description;

    [Header("Scene Reference")]
    public GameObject doorObject;
    
}
