﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace GalaxyExplorer
{
    public class InputRouter : MonoBehaviour
    {
        private TransitionManager transition = null;

        private bool isCtrlHeld = false;    // true is left or right ctrl key is held down

        void Start()
        {
            // Register key events
            KeyboardManager.KeyEvent keyDownEvent = KeyboardManager.KeyEvent.KeyDown;
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Space, keyDownEvent), SpaceTapKeyboardHandler);
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Backspace, keyDownEvent), BackSpaceKeyboardHandler);

            KeyboardManager.KeyEvent keyHeldEvent = KeyboardManager.KeyEvent.KeyHeld;
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.LeftControl, keyHeldEvent), CtrlKeyboardHandler);
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.RightControl, keyHeldEvent), CtrlKeyboardHandler);

            KeyboardManager.KeyEvent keyUpEvent = KeyboardManager.KeyEvent.KeyUp;
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.LeftControl, keyUpEvent), CtrlKeyboardHandler);
            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.RightControl, keyUpEvent), CtrlKeyboardHandler);

            transition = FindObjectOfType<TransitionManager>();
        }


        private void SpaceTapKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
        {
            HandleOnInputUp(GazeManager.Instance.HitObject ? GazeManager.Instance.HitObject.GetComponentInParent<IInputHandler>() : null);
        }

        private void CtrlKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
        {       
            if (keyCodeEvent.KeyEvent == KeyboardManager.KeyEvent.KeyHeld)
            {
                isCtrlHeld = true;
            }
            else if (keyCodeEvent.KeyEvent == KeyboardManager.KeyEvent.KeyUp)
            {
                isCtrlHeld = false;
            }
        }

        private void BackSpaceKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
        {
            if (isCtrlHeld)
            {
                transition.LoadPrevScene();
            }
        }

        private void HandleOnInputUp(IInputHandler handler)
        {
            if (handler != null)
            {
                handler.OnInputUp(null);
            }
        }

    }
}