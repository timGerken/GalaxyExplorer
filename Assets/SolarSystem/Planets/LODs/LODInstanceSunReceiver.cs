// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class LODInstanceSunReceiver : MonoBehaviour
    {
        public Transform Sun;
        public bool SetMaterials = true;

        private MeshRenderer currentRenderer;
        private Vector4 originalSunPosition;

        private void Awake()
        {
            currentRenderer = GetComponent<MeshRenderer>();
            if (SetMaterials)
            {
                originalSunPosition = currentRenderer.sharedMaterial.GetVector("_SunPosition");
            }
        }

        private void Update()
        {
            if (SetMaterials && Sun && currentRenderer)
            {
                currentRenderer.sharedMaterial.SetVector("_SunPosition", Sun.position);
            }
        }

        private void OnDestroy()
        {
            if (SetMaterials)
            {
                currentRenderer.sharedMaterial.SetVector("_SunPosition", originalSunPosition);
            }
        }
    }
}