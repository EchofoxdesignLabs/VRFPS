using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
    public class ShadowMapper : MonoBehaviour
    {
        #region ❐  Properties and fields
        
        public RenderTexture shadowMapCopy;
        private Camera _camera;
        private static readonly int ScreenSpaceShadowmapTexture = Shader.PropertyToID("_ScreenSpaceShadowmapTexture");

        #endregion
        private void OnEnable()
        {
            RenderPipelineManager.endCameraRendering += CameraRender;
            _camera = GetComponent<Camera>();
        }

        // Unity calls this method automatically when it disables this component
        private void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -= CameraRender;
        }

        void CameraRender(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera != _camera) return;

            var rt = Shader.GetGlobalTexture(ScreenSpaceShadowmapTexture);
            if (!rt) return;

            var myCommandBuffer = new CommandBuffer();
            myCommandBuffer.Blit(rt, shadowMapCopy);

            context.ExecuteCommandBuffer(myCommandBuffer);
            context.Submit();
        }
    }
