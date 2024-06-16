using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class ForceDisplay : MonoBehaviour
{
    public GameObject uiPanel;  // Панель для отображения сил
    public GameObject textPrefab;  // Префаб для создания текстовых компонентов
    public bool save_forces = false;
    public bool load_forces = false;

    private Dictionary<string, Text> forceTexts = new Dictionary<string, Text>();
    private Dictionary<GameObject, Vector3> totalForces = PhysicalFootprint.totalForces;

    private string savePath;

    void Start()
    {
        if (uiPanel == null)
        {
            Debug.LogError("UI Panel is not assigned!");
        }

        if (textPrefab == null)
        {
            Debug.LogError("Text Prefab is not assigned!");
        }

        string relativePath = Path.Combine("SavedData", "forces.json");
        savePath = Path.Combine(Application.dataPath, relativePath);
        Debug.Log("Save path: " + savePath);
        if (load_forces) LoadForces();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            SceneManager.LoadScene(0);
        }
        foreach (var kvp in totalForces)
        {
            if (kvp.Key.tag != "Uncontactable")
                 UpdateForce(kvp.Key.name, kvp.Value.magnitude);
        }

       if (save_forces) SaveForces();
    }

    public void UpdateForce(string objectName, float forceMagnitude)
    {
        if (!forceTexts.ContainsKey(objectName))
        {
            GameObject newTextObject = Instantiate(textPrefab, uiPanel.transform);
            Text newText = newTextObject.GetComponent<Text>();

            if (newText == null)
            {
                Debug.LogError("Text component not found in Text prefab!");
                return;
            }

            forceTexts[objectName] = newText;
        }

        if (forceTexts.ContainsKey(objectName))
        {
            if (forceTexts[objectName] != null)
            {
                forceTexts[objectName].text = objectName + ": " + forceMagnitude.ToString("F2") + " N";
            }
            else
            {
                Debug.LogError("Text component is null for object: " + objectName);
            }
        }
        else
        {
            Debug.LogError("Key not found in forceTexts dictionary: " + objectName);
        }
    }

    private void SaveForces()
    {
        SerializableDictionary<string, float> forcesToSave = new SerializableDictionary<string, float>();
        foreach (var kvp in forceTexts)
        {
            if (kvp.Value != null)
            {
                forcesToSave.Add(kvp.Key, float.Parse(kvp.Value.text.Split(':')[1].Replace(" N", "")));
            }
        }

        string json = JsonUtility.ToJson(forcesToSave);
        File.AppendAllText(savePath, json);
    }


    private void LoadForces()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);

            foreach (var kvp in wrapper.dictionary)
            {
                UpdateForce(kvp.Key, kvp.Value);
            }
        }
    }

    [System.Serializable]
    private class SerializationWrapper
    {
        public Dictionary<string, float> dictionary;

        public SerializationWrapper(Dictionary<string, float> dictionary)
        {
            this.dictionary = dictionary;
        }
    }
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public void Add(TKey key, TValue value)
        {
            keys.Add(key);
            values.Add(value);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dictionary[keys[i]] = values[i];
            }
            return dictionary;
        }
    }
}
