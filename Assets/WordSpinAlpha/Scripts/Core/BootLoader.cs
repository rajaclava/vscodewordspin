using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class BootLoader : MonoBehaviour
    {
        private void Start()
        {
            SceneNavigator.Instance?.OpenEntryMenu();
        }
    }
}
