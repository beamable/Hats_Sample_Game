using UnityEngine;
using VIS.StickyNotes.MonoBehaviours;

namespace VIS.StickyNotes.Examples
{
    [RequireComponent(typeof(StickyNote))]
    public class StickyConsoleExample : MonoBehaviour
    {
        void Start()
        {
            GetComponent<StickyNote>().ConsoleTextEntered += onConsoleTextEntered;
        }

        private void onConsoleTextEntered(StickyNote arg1, string arg2)
        {
            Debug.Log(string.Format("Text entered: {0}", arg2));
        }
    }
}
