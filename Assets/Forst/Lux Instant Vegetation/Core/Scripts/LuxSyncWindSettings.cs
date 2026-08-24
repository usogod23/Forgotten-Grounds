using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;


namespace LuxInstantVegetation
{

    public enum _DebugModes
    {
        WindMask,
        BranchPhase,
        FlutterPhase,
        FlutterMask
    }

    [System.Serializable]
    public class MainBending
    {
        [Space(4)]
        [Tooltip("Strength of the Main Bending along the wind direction.")]
        public float _MainStrength = 1.0f;

        [Range(0.0f, 2.0f)]
        [Tooltip("Controls the stiffness of the trunk. Smaller values will make it stiffer.")]
        public float _MainPower = 1.2f;
        
        [Tooltip("Values > 0.0 will add some more main bending to outer vertices.")]
        public float _ScaleAlongXZ = 0.1f;
        
        [Range(0.0f, 4.0f)]
        [Tooltip("Speed of the main bending animation.")]
        public float _Speed = 1.0f;
    }

    [System.Serializable]
    public class BakedWindMask
    {
        [Tooltip("If checked the shader will pick the wind mask from the vertex color channel defined below.")]
        public bool _EnableBakedWindMask = false;
        
        [Range(0, 3)]
        [Tooltip("The vertex color channel which stores the wind mask. 0 means red, 1 means green, 2 means blue and 3 means alpha.")]
        public int _MaskVCChannel = 0;
    }

    [System.Serializable]
    public class ProceduralWindMask
    {
        [Tooltip("Radius of the procedural mask which masks out branch bending (especially useful to mask out the trunk).")]
        public float _ProceduralWindMask = 1.0f;

        [Range(0.01f, 2.0f)]
        [Tooltip("Falloff of the mask.")]
        public float _WindMaskFalloff = 0.75f;

        [Tooltip("If checked the wind mask is calculated using a hemispherical falloff. By default it uses a cylindrical falloff oriented along the up axis.")]
        public bool _UseHemisphere = false;

        [Tooltip("Lets you scale or skew the cylindrical or hemispherical mask.")]
        public Vector3 _MaskScale = Vector3.one;
    }

    [System.Serializable]
    public class BakedPhase
    {
        [Tooltip("If checked the shader will grab the phase from the vertex color channel defined below.")]
        public bool _EnableBakedPhase = false;
        [Range(0, 3)]
        [Tooltip("The vertex color channel which stores the phase. 0 means red, 1 means green, 2 means blue and 3 means alpha.")]
        public int _PhaseChannel = 0;
        [Tooltip("Lets you scale the phase to add more variation.")]
        public float _PhaseScale = 1.0f;
    }

    [System.Serializable]
    public class ProceduralPhase
    {
        [Tooltip("Tiling or frequency of the procedural phase in object space.")]
        public float _ProceduralTiling = 0.05f;
        [Tooltip("Final multiplier on the procedural branch wind.")]
        [Range(0.0f, 1.0f)]
        public float _ProceduralStrength = 1.0f;
    }

    [System.Serializable]
    public class BranchBending
    {
        [Space(4)]

        [Tooltip("If checked the shader will calculate secondary branch bending.")]
        public bool _EnableBranchWind = false;

        [Range(0.0f, 1.0f)]
        [Tooltip("Lets you mask out wind on the side pointing away from the wind ('wind shadow').")]
        public float _DirectionalMask = 0.5f;

        [Tooltip("Strength of the rotation around the y axis. If Rolling = 0.0 the shader will skip some computations.")]
        public float _Rolling = 1.0f;

        [Tooltip("Strength of the movement up and down.")]
        public float _Displacement = 0.3f;

        [Tooltip("Speed of the branch bending animation.")]
        public float _BranchWindSpeed = 0.1f;

        [Space(4)]
        public BakedWindMask _BakedWindMask = new BakedWindMask();
        public ProceduralWindMask _ProceduralWindMask = new ProceduralWindMask();

        [Space(4)]
        public BakedPhase _BakedPhase = new BakedPhase();
        public ProceduralPhase _ProceduralPhase = new ProceduralPhase();
        
    }


    [System.Serializable]
    public class EdgeFlutter
    {
        [Space(4)]
        
        [Tooltip("If checked the shader will apply edge flutter.")]
        public bool _EnableFlutter;
        [Range(0, 3)]
        [Tooltip("Lets you choose the vertex color channel in which your flutter mask is stored. 0 means red, 1 means green, 2 means blue and 3 means alpha.")]
        public int _FlutterMaskVCChannel = 1;
        [Tooltip("If checked the shader will expect a TVE baked flutter mask in UV0.z.")]
        public bool _TVEFlutterMask = false;
        [Tooltip("Strength or amplitude of the edge flutter animation.")]
        public float _FlutterStrength = 0.1f;
        [Tooltip("Speed of the edge flutter animation.")]
        public float _FlutterSpeed = 1.0f;
        [Tooltip("Tiling or frequency of the edge flutter.")]
        public float _FlutterTiling = 0.1f;
    }

    [RequireComponent(typeof(Renderer))]
    public class LuxSyncWindSettings : MonoBehaviour
    {
        [Range(0, 3)]
        public int _MasterMaterial = 0;
        [Space(4)]

        public Material[] materials;
        public Shader[] shaders;

        [Space(4)]
        [Header ("Color Variation")]
        [Space(6)]
        public Color _ColorVariation = new Color(0.9f, 0.5f, 0.0f, 0.1f);

        [Space(4)]
        [Header ("Wind Settings")]
        [Space(6)]
        public MainBending _MainBending = new MainBending();
        public BranchBending _BranchBending = new BranchBending();
        public EdgeFlutter _EdgeFlutter = new EdgeFlutter();

        [Space(4)]
        [Header ("Debug")]
        [Space(6)]
        public _DebugModes _DebugMode = _DebugModes.WindMask;
        [Tooltip("If checked the script will assign the debug shader and set the chosen debug mode.")]
        public bool _Debug = false;
        private bool _DebugEnabled = false;
        [Space(8)]
        [Tooltip("If checked the script will automatically send all changes to the assigned materials.")]
        public bool _AutoSync = false;
        [Space(8)]
        
        private Shader _DebugShader;

        [HideInInspector, SerializeField]
        public bool _DontFetchValues = false;

    #if UNITY_EDITOR

        public void SyncShaders()
        {
            if (_DebugEnabled)
            {
                DisableDebug();
                _Debug = false;
            }
            shaders = new Shader[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                shaders[i] = materials[i].shader;
            }
        }

        public void GetMaterials()
        {
            var renderer = this.GetComponent<Renderer>();
            materials = renderer.sharedMaterials;
            shaders = new Shader[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                shaders[i] = materials[i].shader;
            }
        }

        void SetDebugMode()
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (_DebugMode == _DebugModes.WindMask)
                {
                    materials[i].SetFloat("_DebugWindMask", 1.0f);
                    materials[i].SetFloat("_DebugBranchPhase", 0.0f);
                    materials[i].SetFloat("_DebugFlutterPhase", 0.0f);
                    materials[i].SetFloat("_DebugFlutterMask", 0.0f);
                }

                else if (_DebugMode == _DebugModes.BranchPhase)
                {
                    materials[i].SetFloat("_DebugWindMask", 0.0f);
                    materials[i].SetFloat("_DebugBranchPhase", 1.0f);
                    materials[i].SetFloat("_DebugFlutterPhase", 0.0f);
                    materials[i].SetFloat("_DebugFlutterMask", 0.0f);
                }

                else if (_DebugMode == _DebugModes.FlutterPhase)
                {
                    materials[i].SetFloat("_DebugWindMask", 0.0f);
                    materials[i].SetFloat("_DebugBranchPhase", 0.0f);
                    materials[i].SetFloat("_DebugFlutterPhase", 1.0f);
                    materials[i].SetFloat("_DebugFlutterMask", 0.0f);
                }

                else
                {
                    materials[i].SetFloat("_DebugWindMask", 0.0f);
                    materials[i].SetFloat("_DebugBranchPhase", 0.0f);
                    materials[i].SetFloat("_DebugFlutterPhase", 0.0f);
                    materials[i].SetFloat("_DebugFlutterMask", 1.0f);
                }
            }
        }

        void EnableDebug()
        {
            if (_DebugEnabled)
            {
                return;
            }

            _DebugEnabled = true;

            if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("HD"))
            {
                _DebugShader = Shader.Find("Lux Instant Wind/HDRP/Debug");
            }
            else
            {
                _DebugShader = Shader.Find("Lux Instant Wind/URP/Debug");
            }
            
            shaders = new Shader[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                shaders[i] = materials[i].shader;
                materials[i].shader = _DebugShader;
            }
            SetDebugMode();
        }

        void DisableDebug()
        {
            _DebugEnabled = false;

            if (materials != null && shaders != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].shader = shaders[i];
                }
            }
        }

        void OnDisable()
        {
            DisableDebug();
        }

        public void GetValuesFromMaterialEdior()
        {
            GetValuesFromMaterial(_MasterMaterial);
        }
        
        void GetValuesFromMaterial(int index)
        {
            if (materials == null)
            {
                Debug.Log("No materials found.");
                return;
            }

            index = Math.Min(index, materials.Length - 1);

            if (materials[index] == null)
            {
                Debug.Log("Index material is null.");
                return;
            }
            
            else
            {
                var mat = materials[index];

                if (mat.HasProperty("_HueVariation"))
                {
                    _ColorVariation = mat.GetColor("_HueVariation");
                }
                
                if (mat.HasProperty("_MainBendingStrength"))
                {
                    _MainBending._MainStrength = mat.GetFloat("_MainBendingStrength");
                }
                if (mat.HasProperty("_MainBendingPower"))
                {
                    _MainBending._MainPower = mat.GetFloat("_MainBendingPower");
                }
                if (mat.HasProperty("_MainScaleXZ"))
                {
                    _MainBending._ScaleAlongXZ = mat.GetFloat("_MainScaleXZ");
                }
                if (mat.HasProperty("_MainBendingSpeed"))
                {
                    _MainBending._Speed = mat.GetFloat("_MainBendingSpeed");
                }

                // Branch Wind
                if (mat.HasProperty("_EnableBranchWind"))
                {
                    _BranchBending._EnableBranchWind = mat.GetFloat("_EnableBranchWind") == 1.0f ? true : false;
                }
                if (mat.HasProperty("_RollingDirectionalMask"))
                {
                    _BranchBending._DirectionalMask = mat.GetFloat("_RollingDirectionalMask");
                }
                if (mat.HasProperty("_Rolling"))
                {
                    _BranchBending._Rolling = mat.GetFloat("_Rolling");
                }
                if (mat.HasProperty("_RollingDisplacement"))
                {
                    _BranchBending._Displacement = mat.GetFloat("_RollingDisplacement");
                }
                if (mat.HasProperty("_BranchWindSpeed"))
                {
                    _BranchBending._BranchWindSpeed = mat.GetFloat("_BranchWindSpeed");
                }

                // Baked Wind Mask
                if (mat.HasProperty("_EnableBakedWindMask"))
                {
                    _BranchBending._BakedWindMask._EnableBakedWindMask = mat.GetFloat("_EnableBakedWindMask") == 1.0f ? true : false;
                }
                if (mat.HasProperty("_MaskVCChannel"))
                {
                    _BranchBending._BakedWindMask._MaskVCChannel = (int)mat.GetFloat("_MaskVCChannel");
                }
                
                // Procedural Wind Mask
                if (mat.HasProperty("_WindMask"))
                {
                    _BranchBending._ProceduralWindMask._ProceduralWindMask = mat.GetFloat("_WindMask");
                }
                if (mat.HasProperty("_WindMaskFalloff"))
                {
                    _BranchBending._ProceduralWindMask._WindMaskFalloff = mat.GetFloat("_WindMaskFalloff");
                }
                if (mat.HasProperty("_UseHemisphere"))
                {
                    _BranchBending._ProceduralWindMask._UseHemisphere = mat.GetFloat("_UseHemisphere") == 1.0f ? true : false;
                }
                if (mat.HasProperty("_MaskScale"))
                {
                    _BranchBending._ProceduralWindMask._MaskScale = mat.GetVector("_MaskScale");
                }

                // Baked Phase
                if (mat.HasProperty("_EnableBakedPhase"))
                {
                    _BranchBending._BakedPhase._EnableBakedPhase = mat.GetFloat("_EnableBakedPhase") == 1.0f ? true : false;
                }
                if (mat.HasProperty("_PhaseChannel"))
                {
                    _BranchBending._BakedPhase._PhaseChannel = (int)mat.GetFloat("_PhaseChannel");
                }
                if (mat.HasProperty("_PhaseScale"))
                {
                    _BranchBending._BakedPhase._PhaseScale = mat.GetFloat("_PhaseScale");
                }

                // Procedural Phase
                if (mat.HasProperty("_ProceduralTiling"))
                {
                    _BranchBending._ProceduralPhase._ProceduralTiling = mat.GetFloat("_ProceduralTiling");
                }
                if (mat.HasProperty("_RollingProceduralStrength"))
                {
                    _BranchBending._ProceduralPhase._ProceduralStrength = mat.GetFloat("_RollingProceduralStrength");
                }

                // Flutter
                if (mat.HasProperty("_EnableFlutter"))
                {
                    _EdgeFlutter._EnableFlutter = mat.GetFloat("_EnableFlutter") == 1.0f ? true : false;
                }
                if (mat.HasProperty("_FlutterMask"))
                {
                    _EdgeFlutter._FlutterMaskVCChannel = (int)mat.GetFloat("_FlutterMask");
                }
                if (mat.HasProperty("_TVEFlutterMask"))
                {
                    _EdgeFlutter._TVEFlutterMask = mat.GetFloat("_TVEFlutterMask") == 1.0f ? true : false;
                }
                if (mat.HasProperty("_FlutterStrength"))
                {
                    _EdgeFlutter._FlutterStrength = mat.GetFloat("_FlutterStrength");
                }
                if (mat.HasProperty("_FlutterSpeed"))
                {
                    _EdgeFlutter._FlutterSpeed = mat.GetFloat("_FlutterSpeed");
                }
                if (mat.HasProperty("_FlutterTiling"))
                {
                    _EdgeFlutter._FlutterTiling = mat.GetFloat("_FlutterTiling");
                }
            }
        }

        public void SyncMaterials()
        {
            
            Undo.RegisterCompleteObjectUndo(this, "SyncMaterials");

            foreach (Material mat in materials)
            {
                
                if (mat.HasProperty("_HueVariation"))
                {
                    mat.SetColor("_HueVariation", _ColorVariation);
                }

                if (mat.HasProperty("_MainBendingStrength"))
                {
                    mat.SetFloat("_MainBendingStrength",  _MainBending._MainStrength);
                }
                if (mat.HasProperty("_MainBendingPower"))
                {
                    mat.SetFloat("_MainBendingPower", _MainBending._MainPower);
                }
                if (mat.HasProperty("_MainScaleXZ"))
                {
                    mat.SetFloat("_MainScaleXZ", _MainBending._ScaleAlongXZ);
                }
                if (mat.HasProperty("_MainBendingSpeed"))
                {
                    mat.SetFloat("_MainBendingSpeed", _MainBending._Speed);
                }

                // Branch Wind
                if (mat.HasProperty("_EnableBranchWind"))
                {
                    mat.SetFloat("_EnableBranchWind", _BranchBending._EnableBranchWind ? 1.0f : 0.0f);
                }
                if (mat.HasProperty("_RollingDirectionalMask"))
                {
                    mat.SetFloat("_RollingDirectionalMask", _BranchBending._DirectionalMask);
                }
                if (mat.HasProperty("_Rolling"))
                {
                    mat.SetFloat("_Rolling", _BranchBending._Rolling);
                }
                if (mat.HasProperty("_RollingDisplacement"))
                {
                    mat.SetFloat("_RollingDisplacement", _BranchBending._Displacement);
                }
                if (mat.HasProperty("_BranchWindSpeed"))
                {
                    mat.SetFloat("_BranchWindSpeed", _BranchBending._BranchWindSpeed);
                }

                // Baked Wind Mask
                if (mat.HasProperty("_EnableBakedWindMask"))
                {
                    mat.SetFloat("_EnableBakedWindMask", _BranchBending._BakedWindMask._EnableBakedWindMask  ? 1.0f : 0.0f);
                }
                if (mat.HasProperty("_MaskVCChannel"))
                {
                    mat.SetFloat("_MaskVCChannel", _BranchBending._BakedWindMask._MaskVCChannel);
                }

                // Procedural Wind Mask
                if (mat.HasProperty("_WindMask"))
                {
                    mat.SetFloat("_WindMask", _BranchBending._ProceduralWindMask._ProceduralWindMask);
                }
                if (mat.HasProperty("_WindMaskFalloff"))
                {
                    mat.SetFloat("_WindMaskFalloff", _BranchBending._ProceduralWindMask._WindMaskFalloff);
                }
                if (mat.HasProperty("_UseHemisphere"))
                {
                    mat.SetFloat("_UseHemisphere", _BranchBending._ProceduralWindMask._UseHemisphere ? 1.0f : 0.0f);
                }
                if (mat.HasProperty("_MaskScale"))
                {
                    mat.SetVector("_MaskScale", _BranchBending._ProceduralWindMask._MaskScale);
                }

                // Baked Phase
                if (mat.HasProperty("_EnableBakedPhase"))
                {
                    mat.SetFloat("_EnableBakedPhase", _BranchBending._BakedPhase._EnableBakedPhase ? 1.0f : 0.0f);
                }
                if (mat.HasProperty("_PhaseChannel"))
                {
                    mat.SetFloat("_PhaseChannel", _BranchBending._BakedPhase._PhaseChannel);
                }
                if (mat.HasProperty("_PhaseScale"))
                {
                    mat.SetFloat("_PhaseScale", _BranchBending._BakedPhase._PhaseScale);
                }

                // Procedural Phase
                if (mat.HasProperty("_ProceduralTiling"))
                {
                    mat.SetFloat("_ProceduralTiling", _BranchBending._ProceduralPhase._ProceduralTiling);
                }
                if (mat.HasProperty("_RollingProceduralStrength"))
                {
                    mat.SetFloat("_RollingProceduralStrength", _BranchBending._ProceduralPhase._ProceduralStrength);
                }

                
                // Flutter
                if (mat.HasProperty("_EnableFlutter"))
                {
                    mat.SetFloat("_EnableFlutter", _EdgeFlutter._EnableFlutter ? 1.0f : 0.0f);
                }
                if (mat.HasProperty("_FlutterMask"))
                {
                    mat.SetFloat("_FlutterMask", _EdgeFlutter._FlutterMaskVCChannel);
                }
                if (mat.HasProperty("_TVEFlutterMask"))
                {
                    mat.SetFloat("_TVEFlutterMask", _EdgeFlutter._TVEFlutterMask ? 1.0f : 0.0f);
                }
                if (mat.HasProperty("_FlutterStrength"))
                {
                    mat.SetFloat("_FlutterStrength", _EdgeFlutter._FlutterStrength);
                }
                if (mat.HasProperty("_FlutterSpeed"))
                {
                    mat.SetFloat("_FlutterSpeed", _EdgeFlutter._FlutterSpeed);
                }
                if (mat.HasProperty("_FlutterTiling"))
                {
                    mat.SetFloat("_FlutterTiling", _EdgeFlutter._FlutterTiling);
                }
            }
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (_Debug)
                {
                    EnableDebug();
                }
                else if (_DebugEnabled)
                {
                    DisableDebug();
                }

                if (_AutoSync)
                {
                    SyncMaterials();
                }
            }
        }
    #endif
    }
}