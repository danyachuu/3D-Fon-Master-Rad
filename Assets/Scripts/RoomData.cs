using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomData", menuName = "Faculty/Room Data")]

public class RoomData : MonoBehaviour
{
    [Header("Room Identification")]
    public string roomID;
    public string roomName;

    [Header("Location Info")]
    public string buildingSection;
    public string floor;

    [Header("Scene Reference")]
    public GameObject doorObject;
    
}
