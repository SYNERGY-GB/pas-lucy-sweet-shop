using UnityEngine;
using UnityEditor;
using System.Collections;

public class CreateEventDataList : MonoBehaviour {
    [MenuItem("Assets/Create/Event Item List")]
    public static EventList Create() {
        EventList asset = ScriptableObject.CreateInstance<EventList>();

        AssetDatabase.CreateAsset(asset, "Assets/InventoryItemList.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}
