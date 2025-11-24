Shader "Unlit/FullScreenOutlineShader"
{
    Properties
    {
        _EdgeThickness("Edge Thickness (px)", Float) = 0.1
        _DepthThreshold("Depth Threshold", Float) = 0.002
        _NormalThreshold("Normal Threshold", Float) = 0.3
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }

        Pass
        {
            Name "FullScreenOutline"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // --------------------------------------------
            // Fullscreen triangle without any includes
            // --------------------------------------------

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            Varyings Vert(Attributes a)
            {
                Varyings o;

                // Fullscreen triangle
                float2 uv = float2((a.vertexID << 1) & 2, a.vertexID & 2);
                o.uv  = uv;
                o.pos = float4(uv * 2.0 - 1.0, 0.0, 1.0);

                return o;
            }

            // --------------------------------------------
            // Textures (scene color, depth, normals)
            // --------------------------------------------

            Texture2D    _BlitTexture;
            SamplerState sampler_BlitTexture;

            Texture2D    _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;

            Texture2D    _CameraNormalsTexture;
            SamplerState sampler_CameraNormalsTexture;

            // Screen params: x = width, y = height, z = 1+1/width, w = 1+1/height
            float4 _ScreenParams;

            // Properties
            float  _EdgeThickness;
            float  _DepthThreshold;
            float  _NormalThreshold;
            float4 _OutlineColor;

            float  SampleDepth(float2 uv)
            {
                return _CameraDepthTexture.Sample(sampler_CameraDepthTexture, uv).r;
            }

            float3 SampleNormal(float2 uv)
            {
                float3 n = _CameraNormalsTexture.Sample(sampler_CameraNormalsTexture, uv).xyz;
                return normalize(n * 2.0 - 1.0);
            }

            float4 SampleColor(float2 uv)
            {
                return _BlitTexture.Sample(sampler_BlitTexture, uv);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                uv.y = 1.0 - uv.y;

                // convert thickness from pixels to UV
                float2 pixel   = 1.0 / _ScreenParams.xy;
                float2 offset  = pixel * _EdgeThickness;

                float2 uvUp    = uv + float2(0,    offset.y);
                float2 uvDown  = uv - float2(0,    offset.y);
                float2 uvRight = uv + float2(offset.x, 0);
                float2 uvLeft  = uv - float2(offset.x, 0);

                // ----- Depth edges -----
                float d0 = SampleDepth(uv);
                float dU = SampleDepth(uvUp);
                float dD = SampleDepth(uvDown);
                float dR = SampleDepth(uvRight);
                float dL = SampleDepth(uvLeft);

                float depthDiff = max(
                    max(abs(dU - d0), abs(dD - d0)),
                    max(abs(dR - d0), abs(dL - d0))
                );

                float depthEdge = step(_DepthThreshold, depthDiff);

                // ----- Normal edges -----
                float3 n0 = SampleNormal(uv);
                float3 nU = SampleNormal(uvUp);
                float3 nD = SampleNormal(uvDown);
                float3 nR = SampleNormal(uvRight);
                float3 nL = SampleNormal(uvLeft);

                float normalDiff = max(
                    max(length(nU - n0), length(nD - n0)),
                    max(length(nR - n0), length(nL - n0))
                );

                float normalEdge = step(_NormalThreshold, normalDiff);

                float edgeMask = max(depthEdge, normalEdge);

                float4 col = SampleColor(uv);
                col.rgb = lerp(col.rgb, _OutlineColor.rgb, edgeMask);

                return col;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
