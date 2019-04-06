using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public class PauseMenu : MonoBehaviour
    {
        MenuManager menuManager;

        // Start is called before the first frame update
        void Start()
        {
            menuManager = FindObjectOfType<MenuManager>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        bool displaying = false;

        private void OnGUI()
        {
            displaying = menuManager.CurrentMenu == MenuManager.MenuStatus.MainMenu;

            if (displaying)
            {
            }
        }
    }
}