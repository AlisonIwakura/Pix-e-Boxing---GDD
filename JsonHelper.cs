using UnityEngine;

public static class JsonHelper
{
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    public static T[] FromJson<T>(string json)
    {
        string wrapped = "{\"Items\":" + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(wrapped).Items;
    }
}
