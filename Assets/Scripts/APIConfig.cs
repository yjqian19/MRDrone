using UnityEngine;

[CreateAssetMenu(fileName = "APIConfig", menuName = "Config/APIConfig")]
public class APIConfig : ScriptableObject
{
    [SerializeField] private string apiKey;
    public string APIKey => apiKey;
}
