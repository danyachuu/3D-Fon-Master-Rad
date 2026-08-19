using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomSearchManager : MonoBehaviour
{
    [Header("Panel Navigation")]
    public GameObject previousMenuPanel;
    public GameObject searchPanel;
    public Button backButton;
    public Button searchButton;

    [Header("UI Search Controls")]
    public TMP_InputField searchInputField;
    public Transform resultsContainer;
    public GameObject resultButtonPrefab;

    [Header("Room Info UI")]
    public TMP_Text roomInfoText;

    [Header("Data Registry")]
    public List<RoomData> allRooms = new List<RoomData>();

    private RoomData selectedRoom;


    void Start()
    {
        RoomComponent[] foundRooms = FindObjectsOfType<RoomComponent>();
        foreach (RoomComponent roomComp in foundRooms)
        {
            if (roomComp.data != null && !allRooms.Contains(roomComp.data))
            {
                allRooms.Add(roomComp.data);
            }
        }

        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
            searchInputField.onSubmit.AddListener((text) => OnSearchInputChanged(text));
        }

        if (searchButton != null)
        {
            searchButton.onClick.AddListener(TriggerSearchFromButton);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OpenPreviousPanel);
        }

        ResetInfoDisplay();
    }

    public void OpenSearchPanel()
    {
        if (previousMenuPanel != null) previousMenuPanel.SetActive(false);
        if (searchPanel != null) searchPanel.SetActive(true);

        ResetInfoDisplay();
        ClearResults();
        if (searchInputField != null) searchInputField.text = "";
    }

    public void OpenPreviousPanel()
    {
        if (searchPanel != null) searchPanel.SetActive(false);
        if (previousMenuPanel != null) previousMenuPanel.SetActive(true);
    }

    public void TriggerSearchFromButton()
    {
        if (searchInputField != null)
        {
            OnSearchInputChanged(searchInputField.text);
        }
    }

    public void OnSearchInputChanged(string query)
    {
        ClearResults();

        if (string.IsNullOrEmpty(query.Trim())) return;

        string cleanQuery = query.ToLower().Trim();

        foreach (RoomData room in allRooms)
        {
            if (room.roomID.ToLower().Contains(cleanQuery) ||
                room.roomName.ToLower().Contains(cleanQuery) ||
                room.buildingSection.ToLower().Contains(cleanQuery))
            {
                CreateResultButton(room);
            }
        }
    }

    private void CreateResultButton(RoomData room)
    {
        GameObject newButton = Instantiate(resultButtonPrefab, resultsContainer);
        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text = $"{room.roomID} - {room.roomName}";
        }

        Button btn = newButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => SelectRoom(room));
        }
    }

    public void SelectRoom(RoomData room)
    {
        selectedRoom = room;

        if (roomInfoText != null)
        {
            roomInfoText.text = $"ID: {room.roomID}\n" +
                                $"Tip: {room.roomName}\n" +
                                $"Sektor: {room.buildingSection}\n" +
                                $"Sprat: {room.floor}\n" +
                                $"Opis: {room.description}";
        }

        // Place door highlighting logic call here in the next step
        Debug.Log($"Selected room: {room.roomID}. Target room data linked!");
    }
    private void ResetInfoDisplay()
    {
        if (roomInfoText != null)
        {
            roomInfoText.text = "ID: /\nTip: /\nSektor: /\nSprat: /\nOpis: /";
        }
    }

    private void ClearResults()
    {
        foreach (Transform child in resultsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
