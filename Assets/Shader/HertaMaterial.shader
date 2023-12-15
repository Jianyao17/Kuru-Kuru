Shader "Custom/HertaMaterial"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 5.0

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
            };

            struct MeshProperties 
            {
                float4x4 mat; 
                float4 sprite_UV;
            };

            float4 _Color;
            sampler2D _MainTex;
            StructuredBuffer<MeshProperties> _Properties;

            //UNITY_INSTANCING_BUFFER_START(Props)
            //  UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            //UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                //UNITY_SETUP_INSTANCE_ID(v);
                //UNITY_TRANSFER_INSTANCE_ID(v, o);

                float4 pos = mul(_Properties[instanceID].mat, v.vertex);
                o.vertex = UnityObjectToClipPos(pos);
                // Apply Sprite Sheet Animation UV
                o.uv = (v.uv * _Properties[instanceID].sprite_UV.xy) + _Properties[instanceID].sprite_UV.zw;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //UNITY_SETUP_INSTANCE_ID(i);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
