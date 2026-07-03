Shader "UI/Blur/Standard2DBlur"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1, 1, 1, 0.5)
        _BlurSize ("Blur Size", Range(0, 10)) = 2.0

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        // Memastikan shader UI ini dirender PALING AKHIR setelah semua Sprite 2D selesai digambar
        Tags
        {
            "Queue"="Transparent+100"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        // Memfoto layar yang sudah berisi Sprite 2D
        GrabPass { "_GrabTexture" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 grabPos  : TEXCOORD1;
            };

            sampler2D _GrabTexture;
            float _BlurSize;
            fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.texcoord = v.texcoord;
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texelSize = _BlurSize / _ScreenParams.xy;
                half4 color = 0;

                // Proses Gaussian Blur sederhana
                color += tex2Dproj(_GrabTexture, i.grabPos + float4(-texelSize.x, -texelSize.y, 0, 0)) * 0.111;
                color += tex2Dproj(_GrabTexture, i.grabPos + float4(0, -texelSize.y, 0, 0)) * 0.111;
                color += tex2Dproj(_GrabTexture, i.grabPos + float4(texelSize.x, -texelSize.y, 0, 0)) * 0.111;

                color += tex2Dproj(_GrabTexture, i.grabPos + float4(-texelSize.x, 0, 0, 0)) * 0.111;
                color += tex2Dproj(_GrabTexture, i.grabPos + float4(0, 0, 0, 0)) * 0.111;
                color += tex2Dproj(_GrabTexture, i.grabPos + float4(texelSize.x, 0, 0, 0)) * 0.111;

                color += tex2Dproj(_GrabTexture, i.grabPos + float4(-texelSize.x, texelSize.y, 0, 0)) * 0.111;
                color += tex2Dproj(_GrabTexture, i.grabPos + float4(0, texelSize.y, 0, 0)) * 0.111;
                color += tex2Dproj(_GrabTexture, i.grabPos + float4(texelSize.x, texelSize.y, 0, 0)) * 0.111;

                // Menggabungkan dengan warna putih (Tint Color)
                color.rgb = lerp(color.rgb, i.color.rgb, i.color.a);
                color.a = 1;

                return color;
            }
            ENDCG
        }
    }
}