using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestGPSSystem : MonoBehaviour
{
    [System.Serializable]
    public class Quest
    {
        public string questName;
        public Transform target;
        public QuestType questType;
        public Sprite questIcon;
        [Header("Quest Chain")]
        public Quest unlockAfterQuest;
        [HideInInspector] public bool isCompleted;
        public bool startLocked;
        [HideInInspector] public RectTransform markerUI;
        [HideInInspector] public TextMeshProUGUI distanceText;
        [HideInInspector] public Vector3 smoothPos;
        [Header("Quest Order")]
        public int questOrder = 0;
    }

    public enum QuestType
    {
        DisappearWhenReached,
        Permanent
    }
 
    [Header("Player")]
    public Transform player;

    [Header("Quest List")]
    public List<Quest> quests = new List<Quest>();

    [Header("Marker Prefab")]
    public GameObject markerPrefab;

    [Header("UI Parent")]
    public Transform markerParent;

    [Header("Default Icon")]
    public Sprite defaultIcon;

    [Header("Screen Settings")]
    public float screenEdgeOffset = 80f;

    [Header("Scale Settings")]
    public float minDistance = 5f;
    public float maxDistance = 80f;
    public float minScale = 0.6f;
    public float maxScale = 1.6f;

    [Header("Smooth Movement")]
    public float smoothSpeed = 10f;

    [Header("Marker Collision")]
    public float markerSpacing = 35f;

    [Header("World Offset")]
    public Vector3 worldOffset;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        quests.Sort((a, b) => a.questOrder.CompareTo(b.questOrder));
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        foreach (var quest in quests)
        {
            CreateMarker(quest);
        }
    }

    void Update()
    {
        foreach (var quest in quests)
        {
            if (quest.target == null || quest.markerUI == null)
                continue;

            UpdateQuestMarker(quest);
        }

        PreventMarkerOverlap();
    }

    void CreateMarker(Quest quest)
    {
        GameObject marker = Instantiate(markerPrefab, markerParent);

        quest.markerUI = marker.GetComponent<RectTransform>();
        quest.distanceText = marker.GetComponentInChildren<TextMeshProUGUI>();

        Image icon = marker.GetComponentInChildren<Image>();

        if (icon != null)
        {
            icon.sprite = quest.questIcon != null ? quest.questIcon : defaultIcon;
        }
        if (quest.startLocked)
        {
            marker.SetActive(false);
        }
        quest.smoothPos = quest.markerUI.position;
    }

    void UpdateQuestMarker(Quest quest)
    {
        Vector3 worldPos = quest.target.position + worldOffset;

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0)
            screenPos *= -1;

        float minX = screenEdgeOffset;
        float maxX = Screen.width - screenEdgeOffset;
        float minY = screenEdgeOffset;
        float maxY = Screen.height - screenEdgeOffset;

        screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
        screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

        // Smooth movement ลดอาการสั่น
        quest.smoothPos = Vector3.Lerp(
            quest.smoothPos,
            screenPos,
            Time.deltaTime * smoothSpeed
        );

        quest.markerUI.position = quest.smoothPos;

        UpdateDistanceAndScale(quest);
    }

    void UpdateDistanceAndScale(Quest quest)
    {
        Vector3 playerPos = player.position;
        Vector3 targetPos = quest.target.position;

        playerPos.y = 0;
        targetPos.y = 0;

        float distance = Vector3.Distance(playerPos, targetPos);

        if (quest.distanceText != null)
            quest.distanceText.text = Mathf.RoundToInt(distance) + " m";

        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
        float scale = Mathf.Lerp(minScale, maxScale, t);

        quest.markerUI.localScale = Vector3.one * scale;

        if (quest.questType == QuestType.DisappearWhenReached && distance < minDistance)
        {
            CompleteQuest(quest);
        }
    }
    void CompleteQuest(Quest quest)
    {
        quest.isCompleted = true;

        if (quest.markerUI != null)
            quest.markerUI.gameObject.SetActive(false);

        int nextOrder = quest.questOrder + 1;

        foreach (var q in quests)
        {
            if (q.questOrder == nextOrder && q.markerUI != null)
            {
                q.markerUI.gameObject.SetActive(true);
            }
        }
    }
    void UnlockNextQuests(Quest completedQuest)
    {
        foreach (var q in quests)
        {
            if (q.unlockAfterQuest == completedQuest)
            {
                if (q.markerUI != null)
                    q.markerUI.gameObject.SetActive(true);
            }
        }
    }
    void PreventMarkerOverlap()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            for (int j = i + 1; j < quests.Count; j++)
            {
                if (quests[i].markerUI == null || quests[j].markerUI == null)
                    continue;

                float dist = Vector2.Distance(
                    quests[i].markerUI.position,
                    quests[j].markerUI.position
                );

                if (dist < markerSpacing)
                {
                    Vector3 offset = Vector3.up * (markerSpacing - dist);

                    quests[j].markerUI.position += offset;
                }
            }
        }
    }

    public void AddQuest(Transform target, QuestType type, Sprite icon)
    {
        Quest newQuest = new Quest();

        newQuest.target = target;
        newQuest.questType = type;
        newQuest.questIcon = icon;

        CreateMarker(newQuest);

        quests.Add(newQuest);
    }
}