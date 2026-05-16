using UnityEngine;

namespace Project.Assets._Project._Scripts
{
    public class Persistent : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
