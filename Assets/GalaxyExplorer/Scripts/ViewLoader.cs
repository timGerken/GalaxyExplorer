﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Load and Unload functionality
/// </summary>
namespace GalaxyExplorer
{
    public delegate void SceneLoaded(string oldSceneName);

    public class ViewLoader : MonoBehaviour
    {
        [SerializeField]
        private string IntroScene = "LogoScene";

        [SerializeField]
        private string CoreSystemsScene = "CoreSystems";

        [SerializeField]
        private string EarthPinScene = "EarthPin";

        public delegate void SceneIsLoadedCallback();
        public SceneIsLoadedCallback OnSceneIsLoaded;

        public static string CurrentView
        {
            get; private set;
        }

        public static string PreviousView
        {
            get; private set;
        }

        private static Stack<string> viewBackStack = new Stack<string>();
     

        public void LoadViewAsync(string viewName, SceneLoaded sceneLoadedCallback = null)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                Debug.LogError("ViewLoader: no scene name specified when calling LoadViewAsync() - cannot load the scene");
                return;
            }

            if (!IsIntroFlowScene(viewName) && viewName != null) 
            {
                viewBackStack.Push(viewName);
            }

            StartCoroutine(LoadViewAsyncInternal(viewName, sceneLoadedCallback));
        }

        private IEnumerator LoadViewAsyncInternal(string viewName, SceneLoaded sceneLoadedCallback = null)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(viewName, LoadSceneMode.Additive);

            if (loadOperation == null)
            {
                throw new InvalidOperationException(string.Format("ViewLoader: Unable to load {0}. Make sure that the scene is enabled in the Build Settings.", viewName));
            }

            while (!loadOperation.isDone)
            {
                yield return null;
            }
           
            Debug.Log("ViewLoader: Loaded " + viewName);

            if (OnSceneIsLoaded != null)
            {
                OnSceneIsLoaded.Invoke();
            }

            if (sceneLoadedCallback != null)
            {
                sceneLoadedCallback(CurrentView);
            }

            PreviousView = (CurrentView == null) ? viewName : CurrentView;
            CurrentView = viewName;
        }

        public void LoadPreviousScene(SceneLoaded sceneLoadedCallback = null)
        {
            if (viewBackStack.Count > 0)
            {
                string viewToLoad = viewBackStack.Pop();

                if (!string.IsNullOrEmpty(viewToLoad))
                {
                    LoadViewAsync(viewToLoad, sceneLoadedCallback);
                }
            }
        }

        public void PopSceneFromStack()
        {
            if (viewBackStack.Count > 0)
            {
                PreviousView = viewBackStack.Pop();
            }
        }

        public void UnloadScene(string view)
        {
            SceneManager.UnloadSceneAsync(view);
        }

        public void UnLoadCurrentView(bool keepOnStack)
        {
            if (CurrentView != null)
            {
                if (!keepOnStack)
                {
                    viewBackStack.Pop();
                }

                SceneManager.UnloadSceneAsync(CurrentView);
            }
        }

        public void UnLoadView(string view, bool keepOnStack)
        {
            if (view != null)
            {
                if (!keepOnStack)
                {
                    viewBackStack.Pop();
                }

                SceneManager.UnloadSceneAsync(view);
            }
        }

        /// <summary>
        /// Retrns true if scene view belongs to intro scenes and isnt supposed to be added in the scene stack
        /// </summary>
        /// <param name="view">View to check</param>
        /// <returns>True if its part of intro</returns>
        private bool IsIntroFlowScene(string view)
        {
            return (view != null ? view.CompareTo(IntroScene) == 0 || view.CompareTo(CoreSystemsScene) == 0 || view.CompareTo(EarthPinScene) == 0 : false);
        }
    }
}