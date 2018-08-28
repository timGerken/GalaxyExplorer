using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Bloomer : MonoBehaviour
{
    public Shader BloomShader;

    [Range(1, 16)]
    public int Iterations = 1;

    [Range(1, 8)]
    public int StepsPerIteration = 1;

    [Range(0f, 32f)]
    public float Intensity = 1f;

    [Range(0f, 1f)]
    public float Threshold = .5f;

    public bool IterativeDownScale = true;
    public bool IterativeUpScale = true;

    public int IterationsToSkipDown = 0;
    public int IterationsToSkipUp = 0;

    public float DeltaDown = 1f;
    public float DeltaUp = 1f;

    [NonSerialized]
    Material bloomMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        int numRenderTexturesCreated = 0;
        int numRenderTexturesReleased = 0;

        if (BloomShader == null)
        {
            Debug.Log("BLOOMER: Need to set bloom shader");
            return;
        }

        if (bloomMaterial == null)
        {
            bloomMaterial = new Material(BloomShader);
            bloomMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        bloomMaterial.SetFloat("_SampleDeltaDown", DeltaDown);
        bloomMaterial.SetFloat("_SampleDeltaUp", DeltaUp);
        bloomMaterial.SetTexture("_SourceTex", source);
        bloomMaterial.SetFloat("_Intensity", Intensity);
        bloomMaterial.SetFloat("_Threshold", Threshold);

        // Need to manage the number of iterations to go down and up to save on draw calls
        // Need to decide how large each up or down step should be to reduce the number of draw calls e.g. downsample by 1/4 instead of 1/2

        RenderTexture[] textures = new RenderTexture[Iterations];

        int width = source.width;
        int height = source.height;
        RenderTextureFormat format = source.format;

        RenderTexture currentDestination = textures[0] = RenderTexture.GetTemporary(width, height, 0, format);
        numRenderTexturesCreated++;

        Graphics.Blit(source, currentDestination);
        RenderTexture currentSource = currentDestination;

        bool isPreFilteringPass = true;

        int divider = (int)Mathf.Pow(2, StepsPerIteration);

        int i = 1;

        if (IterativeDownScale)
        {
            for (; i < Iterations; i++)
            {
                width /= divider;
                height /= divider;

                // Don't keep going if we're already a tiny image
                if (height < 2 || width < 2)
                {
                    break;
                }

                currentDestination = textures[i] = RenderTexture.GetTemporary(width, height, 0, format);
                numRenderTexturesCreated++;

                if (i > IterationsToSkipDown)
                {
                    if (isPreFilteringPass)
                    {
                        // Do prefiltering at the same time as initial down pass
                        Graphics.Blit(currentSource, currentDestination, bloomMaterial, 0);
                        isPreFilteringPass = false;
                    }
                    else if (!IterativeUpScale && i == Iterations - 1)
                    {
                        // for the final pass, blend it too
                        Graphics.Blit(currentSource, destination, bloomMaterial, 5);
                    }
                    else
                    {
                        // Do normal downscaling
                        Graphics.Blit(currentSource, currentDestination, bloomMaterial, 1);
                    }
                }
                else
                {
                    // Skip this iteration
                    currentDestination = currentSource;
                }

                currentSource = currentDestination;
            }
        }
        else
        {
            // Skip Iterative downscaling
            i = Iterations - 1;

            int sizeRatio = (int)Mathf.Pow(2, Iterations * StepsPerIteration);
            width /= sizeRatio;
            height /= sizeRatio;

            for (int j = 1; j < textures.Length; j++)
            {
                currentDestination = textures[j] = RenderTexture.GetTemporary(width, height, 0, format);
                numRenderTexturesCreated++;
            }

            Graphics.Blit(currentSource, currentDestination, bloomMaterial, 0);

            currentSource = currentDestination;
        }

        if (IterativeUpScale)
        {
            for (i -= 2; i >= IterationsToSkipUp; i--)
            {
                currentDestination = textures[i];
                textures[i] = null;

                if (i == IterationsToSkipUp)
                {
                    // final iteration blends original source
                    Graphics.Blit(currentSource, destination, bloomMaterial, 4);
                }
                else
                {
                    Graphics.Blit(currentSource, currentDestination, bloomMaterial, 2);
                }

                currentSource = currentDestination;
            }
        }

        foreach (RenderTexture texture in textures)
        {
            RenderTexture.ReleaseTemporary(texture);
            numRenderTexturesReleased++;
        }

        RenderTexture.ReleaseTemporary(currentDestination);
        numRenderTexturesReleased++;
        //Debug.LogFormat("BLOOMER: Created {0}, Released {1}", numRenderTexturesCreated, numRenderTexturesReleased);
    }
}
