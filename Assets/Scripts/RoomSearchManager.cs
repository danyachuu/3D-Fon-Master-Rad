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

    [Header("Highlight Settings")]
    public Material wallHackMaterial;

    private List<Renderer> activeHighlightedRenderers = new List<Renderer>();
    private List<Material> originalMaterials = new List<Material>();

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

    public void ToggleMenuUI()
    {
        bool isAnyPanelActive = (searchPanel != null && searchPanel.activeSelf) ||
                               (previousMenuPanel != null && previousMenuPanel.activeSelf);

        if (isAnyPanelActive)
        {
            if (searchPanel != null) searchPanel.SetActive(false);
            if (previousMenuPanel != null) previousMenuPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (previousMenuPanel != null) previousMenuPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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

        ClearActiveHighlight();
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
        ResetInfoDisplay();

        if (string.IsNullOrEmpty(query.Trim())) return;

        string cleanQuery = query.ToLower().Trim();

        foreach (RoomData room in allRooms)
        {
            if (room != null &&
           ((!string.IsNullOrEmpty(room.roomID) && room.roomID.ToLower().Contains(cleanQuery)) ||
            (!string.IsNullOrEmpty(room.roomName) && room.roomName.ToLower().Contains(cleanQuery)) ||
            (!string.IsNullOrEmpty(room.buildingSection) && room.buildingSection.ToLower().Contains(cleanQuery))))
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
            roomInfoText.text = $"ID: {room.roomID}\n\n" +
                                $"Tip: {room.roomName}\n\n" +
                                $"Sektor: {room.buildingSection}\n\n" +
                                $"Sprat: {room.floor}\n\n" +
                                $"Opis: {room.description}";
        }

        HighlightTargetDoor(room);
        Debug.Log($"Selected room: {room.roomID}. Target room data linked!");
    }

    private void HighlightTargetDoor(RoomData room)
    {
        ClearActiveHighlight();

        RoomComponent[] sceneDoors = FindObjectsOfType<RoomComponent>();
        foreach (RoomComponent doorComp in sceneDoors)
        {
            if (doorComp.data == room)
            {
                Renderer[] renderers = doorComp.GetComponentsInChildren<Renderer>();

                foreach (Renderer rend in renderers)
                {
                    if (rend != null && wallHackMaterial != null)
                    {
                        activeHighlightedRenderers.Add(rend);

                        Material[] originalMats = rend.sharedMaterials;
                        for (int i = 0; i < originalMats.Length; i++)
                        {
                            originalMaterials.Add(originalMats[i]);
                        }

                        Material[] hackArray = new Material[originalMats.Length];
                        for (int i = 0; i < hackArray.Length; i++)
                        {
                            hackArray[i] = wallHackMaterial;
                        }

                        rend.allowOcclusionWhenDynamic = false;

                        rend.materials = hackArray;
                    }
                }
            }
        }
    }

    public void ClearActiveHighlight()
    {
        int matIndex = 0;
        foreach (Renderer rend in activeHighlightedRenderers)
        {
            if (rend != null)
            {
                int slotCount = rend.sharedMaterials.Length;
                Material[] restoredMats = new Material[slotCount];

                for (int i = 0; i < slotCount; i++)
                {
                    if (matIndex < originalMaterials.Count)
                    {
                        restoredMats[i] = originalMaterials[matIndex];
                        matIndex++;
                    }
                }

                rend.materials = restoredMats;
                rend.allowOcclusionWhenDynamic = true;
            }
        }

        activeHighlightedRenderers.Clear();
        originalMaterials.Clear();
    }

    private void ResetInfoDisplay()
    {
        if (roomInfoText != null)
        {
            roomInfoText.text = "ID: /\n\nTip: /\n\nSektor: /\n\nSprat: /\n\nOpis: /";
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
