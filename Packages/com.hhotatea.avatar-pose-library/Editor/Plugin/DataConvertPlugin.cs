using System;
using System.Linq;
using System.Threading.Tasks;
using com.hhotatea.avatar_pose_library.logic;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using VRC.SDK3.Avatars.Components;
using com.hhotatea.avatar_pose_library.component;
using com.hhotatea.avatar_pose_library.editor;
using com.hhotatea.avatar_pose_library.model;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

[assembly: ExportsPlugin(typeof(DataConvertPlugin))]
namespace com.hhotatea.avatar_pose_library.editor
{
    public class DataConvertPlugin : Plugin<DataConvertPlugin>
    {
        public override string DisplayName => "AvatarPoseLibrary";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("AvatarPose: Data Converting...", ctx =>
                {
                    var settings = ctx.AvatarRootObject.GetComponentsInChildren<AvatarPoseLibrary>();

                    // ターゲット未指定のデータを統合して処理
                    var combinedData = AvatarPoseData.Combine(
                        settings.Select(e => e.data)
                            .ToArray());

                    foreach (var d in combinedData)
                    {
                        var go = new GameObject(d.Guid);
                        var root = settings.FirstOrDefault(e => e.data.name == d.name);
                        go.transform.SetParent(root?.transform ?? ctx.AvatarRootObject.transform);
    
                        BuildRuntimeAnimator(go, d);
                        BuildRuntimeMenu(go, d);
                        BuildRuntimeParameter(go, d);
                    }
                });
        }

        /// <summary>
        /// アニメーターの設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="data"></param>
        void BuildRuntimeAnimator(GameObject obj,AvatarPoseData data)
        {
            var result = new GameObject();

            var locomotionLayer = AnimatorBuilder.BuildLocomotionAnimator(data);
            var paramLayer = AnimatorBuilder.BuildFxAnimator(data);
            var trackingLayer = AnimatorBuilder.BuildTrackingAnimator(data);
            
            var ma_base = result.AddComponent<ModularAvatarMergeAnimator>();
            ma_base.layerPriority = 1;
            ma_base.animator = locomotionLayer;
            ma_base.pathMode = MergeAnimatorPathMode.Absolute;
            ma_base.matchAvatarWriteDefaults = true;
            ma_base.layerType = VRCAvatarDescriptor.AnimLayerType.Base;
            
            var ma_action = result.AddComponent<ModularAvatarMergeAnimator>();
            ma_action.layerPriority = 1;
            ma_action.animator = locomotionLayer;
            ma_action.pathMode = MergeAnimatorPathMode.Absolute;
            ma_action.matchAvatarWriteDefaults = true;
            ma_action.layerType = VRCAvatarDescriptor.AnimLayerType.Action;
            
            var ma_fx = result.AddComponent<ModularAvatarMergeAnimator>();
            ma_fx.layerPriority = 1;
            ma_fx.animator = paramLayer;
            ma_fx.pathMode = MergeAnimatorPathMode.Absolute;
            ma_fx.matchAvatarWriteDefaults = true;
            ma_fx.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            
            var ma_tracking = result.AddComponent<ModularAvatarMergeAnimator>();
            ma_tracking.layerPriority = 1;
            ma_tracking.animator = trackingLayer;
            ma_tracking.pathMode = MergeAnimatorPathMode.Absolute;
            ma_tracking.matchAvatarWriteDefaults = true;
            ma_tracking.layerType = VRCAvatarDescriptor.AnimLayerType.Base;
            
            result.transform.SetParent(obj.transform);
        }

        /// <summary>
        /// メニューの設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="data"></param>
        void BuildRuntimeMenu(GameObject obj,AvatarPoseData data)
        {
            var result = MenuBuilder.BuildPoseMenu(data);
            foreach (var installer in result.GetComponentsInChildren<ModularAvatarMenuInstaller>())
            {
                Debug.Log(installer.gameObject.name);
                installer.transform.SetParent(obj.transform);
            }

            if (result.GetComponent<ModularAvatarMenuInstaller>() == null)
            {
                result.AddComponent<ModularAvatarMenuInstaller>();
            }
            result.transform.SetParent(obj.transform);
        }

        /// <summary>
        /// パラメーターの設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="data"></param>
        void BuildRuntimeParameter(GameObject obj,AvatarPoseData data)
        {
            var result = ParameterBuilder.BuildPoseParameter(data);

            result.transform.SetParent(obj.transform);
        }
    }
}