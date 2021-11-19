using System;
using System.Collections.Generic;
using System.IO;
using DuloGames.UI;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "AddressablesCollecter", menuName = "AddressablesCollecter")]

public class AddressablesCollecter : ScriptableObject
{
    public AddressableAssetSettings settings;

    [Header("原始资源")]
    public UIUnitDatabase unitDatabase;
    public UIItemDatabase itemDatabase;
    public RuntimeAnimatorController[] SharedAnimators;
    public AudioSoundsManager audioManager;
    public List<GameObject> smallAnimals;
    public List<GameObject> VFXPrefabs;

    public List<GameObject> ManagerPrefabs;
    public List<GameObject> UIComponents;

    public List<SceneAsset> SceneAssets;

    public List<GameObject> PreObjects;

    public bool collectUnitAssets = true;
    public bool collectItemAssets = true;
    public bool collectAnimationAssets = true;
    public bool collectAudioAssets = true;
    public bool collectUIAssets = true;
    public bool collectVFXAssets = true;
    public bool collectSmallAnimalsAssets = true;
    public bool collectSceneAssets = false;

    [Header("收集后的资源")]
    public List<PrefabDependencyInfo> unitPrefabInfos;
    public List<PrefabDependencyInfo> itemPrefabInfos;
    public List<LargeAnimatorInfo> AnimatorInfos;
    public List<SceneDependencyInfo> SceneInfos;
    public List<Object> UIResources;
    public List<Object> VFXResources;
    public List<AudioClip> SFXResources;
    public List<Object> SmallAnimalResources;
    public BgmInfo BgmInfo;
    public List<PrefabDependencyInfo> otherPrefabInfos = new List<PrefabDependencyInfo>();

    [ContextMenu("Init")]
    public void Init()
    {
        if (gameManager.GM) 
        {
            unitDatabase = gameManager.GM.unitDataBase;
            itemDatabase = gameManager.GM.itemDataBase;
            audioManager = gameManager.GM.audioMangerPrefab.GetComponent<AudioSoundsManager>();
        }

        AnimatorInfos = new List<LargeAnimatorInfo>();
        SceneAssets = new List<SceneAsset>();

        foreach (RuntimeAnimatorController animatoritem in SharedAnimators)
        {
            LargeAnimatorInfo animatorInfo = new LargeAnimatorInfo();
            animatorInfo.AnimatorController = animatoritem;
            AnimatorInfos.Add(animatorInfo);
        }

        foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
        {
            if (true)
            {
                string path = editorScene.path;
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                SceneAssets.Add(sceneAsset);
            }
        }

        BgmInfo = new BgmInfo();
        BgmInfo.AudioClips = new List<AudioClip>();
        BgmInfo.BgmDatabase = audioManager;
    }

    [ContextMenu("收集资源和资源引用")]
    public void CollectAssetsAndDependencies()
    {
        int i = 0;
        //收集单位数据库的Prefab依赖资源
        if (collectUnitAssets)
        {
            unitPrefabInfos = new List<PrefabDependencyInfo>();
            i = 0;
            foreach (UnitInfo info in unitDatabase.indexes)
            {
                EditorUtility.DisplayProgressBar($"正在收集依赖：Unit/{info.unitName}", $"Unit/{info.unitName}", i / (float)unitDatabase.indexes.Length);
                i++;
                PrefabDependencyInfo prefabInfo = new PrefabDependencyInfo
                {
                    Meshs = new List<Mesh>(),
                    Materials = new List<Material>(),
                    Texture2Ds = new List<Texture2D>(),
                    Animators = new List<RuntimeAnimatorController>(),
                    Animations = new List<AnimationClip>(),
                    Audios = new List<AudioClip>()
                };

                GameObject prefab = info.Prefab;
                if (prefab == null) continue;
                prefabInfo.info = info;
                prefabInfo.Prefab = prefab;

                MeshRenderer[] meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
                if (meshRenderers == null) continue;

                foreach (SkinnedMeshRenderer meshRenderer in prefab.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (meshRenderer.sharedMesh)
                    {
                        prefabInfo.Meshs.AddCheckContains(meshRenderer.sharedMesh);
                        //if (!unitAssetsCollection.ContainsKey(meshRenderer.sharedMesh)) unitAssetsCollection.Add(meshRenderer.sharedMesh, new List<Object>());
                        //unitAssetsCollection[meshRenderer.sharedMesh].AddCheckContains(prefab);
                    }
                    Material[] materials = meshRenderer.sharedMaterials;
                    if (materials == null) continue;
                    foreach (Material material in materials)
                    {
                        if (material == null) continue;
                        prefabInfo.Materials.AddCheckContains(material);

                        Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { material });
                        if (dependencies == null) continue;
                        foreach (Object dependency in dependencies)
                        {
                            if (dependency == null) continue;
                            if (dependency is Texture2D texture) prefabInfo.Texture2Ds.AddCheckContains(texture);
                        }
                    }
                }

                foreach (MeshFilter meshRenderer in prefab.GetComponentsInChildren<MeshFilter>())
                {
                    if (meshRenderer.sharedMesh) prefabInfo.Meshs.AddCheckContains(meshRenderer.sharedMesh);
                }

                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    Material[] materials = meshRenderer.sharedMaterials;
                    if (materials == null) continue;
                    foreach (Material material in materials)
                    {
                        if (material == null) continue;
                        prefabInfo.Materials.AddCheckContains(material);

                        Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { material });
                        if (dependencies == null) continue;
                        foreach (Object dependency in dependencies)
                        {
                            if (dependency == null) continue;
                            if (dependency is Texture2D texture) prefabInfo.Texture2Ds.AddCheckContains(texture);
                        }
                    }
                }

                Animator[] animators = prefab.GetComponentsInChildren<Animator>();
                if (animators == null) continue;
                foreach (Animator animator in animators)
                {
                    if (animator.runtimeAnimatorController == null) continue;
                    if (animator.runtimeAnimatorController.name.Contains("Champion") ||
                        animator.runtimeAnimatorController.name.Contains("Audience")) continue;
                    prefabInfo.Animators.AddCheckContains(animator.runtimeAnimatorController);

                    Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { animator });
                    if (dependencies == null) continue;
                    foreach (Object dependency in dependencies)
                    {
                        if (dependency == null) continue;
                        if (dependency is AnimationClip animationClip) prefabInfo.Animations.AddCheckContains(animationClip);
                    }
                }

                UnitAudioManager audioManager = prefab.GetComponent<UnitAudioManager>();
                if (audioManager && audioManager.voiceType == VoiceType.special)
                {
                    foreach (var v in audioManager.voiceSet.affirmVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.negativeVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.deathVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.attackVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.critAttackVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.hurtVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.overweightVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.rampageVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.fleeVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.roarVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.greetingVoice) prefabInfo.Audios.AddCheckContains(v);
                    foreach (var v in audioManager.voiceSet.doorlockedVoice) prefabInfo.Audios.AddCheckContains(v);
                }
                unitPrefabInfos.Add(prefabInfo);
            }
        }

        //收集物品数据库的Prefab依赖资源
        if (collectItemAssets)
        {
            itemPrefabInfos = new List<PrefabDependencyInfo>();
            i = 0;
            foreach (UIItemInfo info in itemDatabase.items)
            {
                EditorUtility.DisplayProgressBar($"正在收集依赖：Item/{info.Name}", $"Item/{info.Name}", i / (float)itemDatabase.items.Length);
                i++;
                PrefabDependencyInfo prefabInfo = new PrefabDependencyInfo
                {
                    Meshs = new List<Mesh>(),
                    Materials = new List<Material>(),
                    Texture2Ds = new List<Texture2D>(),
                    Animators = new List<RuntimeAnimatorController>(),
                    Animations = new List<AnimationClip>(),
                    Audios = new List<AudioClip>()
                };

                GameObject prefab = info.ItemPrefab as GameObject;
                if (prefab == null) continue;
                prefabInfo.info = info;
                prefabInfo.Prefab = prefab;

                foreach (SkinnedMeshRenderer meshRenderer in prefab.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (meshRenderer.sharedMesh) prefabInfo.Meshs.AddCheckContains(meshRenderer.sharedMesh);
                    Material[] materials = meshRenderer.sharedMaterials;
                    if (materials == null) continue;
                    foreach (Material material in materials)
                    {
                        if (material == null) continue;
                        prefabInfo.Materials.AddCheckContains(material);

                        Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { material });
                        if (dependencies == null) continue;
                        foreach (Object dependency in dependencies)
                        {
                            if (dependency == null) continue;
                            if (dependency is Texture2D texture) prefabInfo.Texture2Ds.AddCheckContains(texture);
                        }
                    }
                }

                foreach (MeshFilter meshRenderer in prefab.GetComponentsInChildren<MeshFilter>())
                {
                    if (meshRenderer.sharedMesh) prefabInfo.Meshs.AddCheckContains(meshRenderer.sharedMesh);
                }

                MeshRenderer[] meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
                if (meshRenderers == null) continue;
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    Material[] materials = meshRenderer.sharedMaterials;
                    if (materials == null) continue;
                    foreach (Material material in materials)
                    {
                        if (material == null) continue;
                        prefabInfo.Materials.AddCheckContains(material);

                        Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { material });
                        if (dependencies == null) continue;
                        foreach (Object dependency in dependencies)
                        {
                            if (dependency == null) continue;
                            if (dependency is Texture2D texture) prefabInfo.Texture2Ds.AddCheckContains(texture);
                        }
                    }
                }

                Animator[] animators = prefab.GetComponentsInChildren<Animator>();
                if (animators == null) continue;
                foreach (Animator animator in animators)
                {
                    if (animator.runtimeAnimatorController == null) continue;
                    if (animator.runtimeAnimatorController.name.Contains("Champion") ||
                        animator.runtimeAnimatorController.name.Contains("Audience")) continue;
                    prefabInfo.Animators.AddCheckContains(animator.runtimeAnimatorController);

                    Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { animator });
                    if (dependencies == null) continue;
                    foreach (Object dependency in dependencies)
                    {
                        if (dependency == null) continue;
                        if (dependency is AnimationClip animationClip) prefabInfo.Animations.AddCheckContains(animationClip);
                    }
                }

                itemPrefabInfos.Add(prefabInfo);
            }
        }

        //收集动画控制器资源
        if (collectAnimationAssets)
        {
            foreach (LargeAnimatorInfo animatorInfo in AnimatorInfos)
            {
                animatorInfo.Animations = new List<AnimationClip>();

                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { animatorInfo.AnimatorController });
                if (dependencies == null) return;

                i = 0;
                foreach (Object dependency in dependencies)
                {
                    EditorUtility.DisplayProgressBar($"正在收集资源引用：Animator/{animatorInfo.AnimatorController.name}", $"Animator/{animatorInfo.AnimatorController.name}", i / (float)dependencies.Length);
                    i++;

                    if (dependency is AnimationClip animationClip) animatorInfo.Animations.AddCheckContains(animationClip);
                }
            }
        }

        //收集音效资源
        if (collectAudioAssets)
        {
            //收集BGM资源
            //foreach (BGMset sfxSet in BgmInfo.BgmDatabase.bgmSet)
            //{
            //    i = 0;
            //    if(sfxSet.clipReffs != null && sfxSet.clipReffs.Count > 0)
            //        foreach (AssetReference audioClip in sfxSet.clipReffs)
            //        {
            //            var handle = audioClip.LoadAssetAsync<AudioClip>();
            //            AudioClip audio = handle.WaitForCompletion();
            //            EditorUtility.DisplayProgressBar($"正在收集背景音乐：{audio.name}", audio.name, i / (float)sfxSet.clipReffs.Count);
            //            i++;
            //            BgmInfo.AudioClips.AddCheckContains(audio);
            //        }
            //}

            //收集SFX资源
            Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { audioManager });
            i = 0;
            foreach (Object dependency in dependencies)
            {
                EditorUtility.DisplayProgressBar($"正在收集音效资源引用：", $"UI/{dependency.name}", i / (float)dependencies.Length);
                i++;

                if (dependency is AudioClip clip) SFXResources.AddCheckContains(clip);
            }
        }

        //收集特效资源
        if (collectVFXAssets)
        {
            foreach (GameObject prefab in VFXPrefabs)
            {
                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { prefab });
                if (dependencies == null) return;

                i = 0;
                foreach (Object dependency in dependencies)
                {
                    if (dependency)
                    {
                        EditorUtility.DisplayProgressBar($"正在收集资源引用：特效/{prefab.name}", $"UI/{dependency.name}", i / (float)dependencies.Length);
                        i++;

                        if (dependency is Texture || dependency is Material || dependency is Mesh || dependency is RuntimeAnimatorController || dependency is AnimationClip || dependency is AudioClip) VFXResources.AddCheckContains(dependency);
                    }
                }
            }
        }

        //收集UI资源
        if (collectUIAssets) 
        {
            foreach (GameObject UIObject in UIComponents)
            {
                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { UIObject });
                if (dependencies == null) return;

                i = 0;
                foreach (Object dependency in dependencies)
                {
                    EditorUtility.DisplayProgressBar($"正在收集资源引用：UI/{UIObject.name}", $"UI/{dependency.name}", i / (float)dependencies.Length);
                    i++;

                    if (dependency is Sprite sprite) UIResources.AddCheckContains(sprite);
                    if (dependency is AnimationClip clip) UIResources.AddCheckContains(clip);
                }
            }
        }

        //收集小动物资源
        if (collectSmallAnimalsAssets)
        {
            foreach (GameObject animal in smallAnimals)
            {
                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { animal });
                if (dependencies == null) return;

                i = 0;
                foreach (Object dependency in dependencies)
                {
                    EditorUtility.DisplayProgressBar($"正在收集资源引用：UI/{animal.name}", $"UI/{dependency.name}", i / (float)dependencies.Length);
                    i++;

                    if (dependency is Texture || dependency is Material || dependency is Mesh || dependency is RuntimeAnimatorController || dependency is AnimationClip) SmallAnimalResources.AddCheckContains(dependency);
                }
            }
        }

        //收集场景资源
        if (collectSceneAssets) 
        {
            foreach (SceneAsset scene in SceneAssets)
            {
                SceneDependencyInfo sceneInfo = new SceneDependencyInfo();
                sceneInfo.scene = scene;
                sceneInfo.meshes = new List<Mesh>();
                sceneInfo.textures = new List<Texture>();
                sceneInfo.materials = new List<Material>();
                sceneInfo.animationclips = new List<AnimationClip>();
                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { scene });
                if (dependencies == null) return;

                i = 0;
                foreach (Object dependency in dependencies)
                {
                    if (dependency)
                    {
                        EditorUtility.DisplayProgressBar($"正在收集资源引用：场景{scene.name}", $"{dependency.name}", i / (float)dependencies.Length);
                        i++;
                        if (dependency is AnimationClip go) sceneInfo.animationclips.AddCheckContains(go);
                        if (dependency is Mesh mesh) sceneInfo.meshes.AddCheckContains(mesh);
                        if (dependency is Texture texture) sceneInfo.textures.AddCheckContains(texture);
                        if (dependency is Material mat) sceneInfo.materials.AddCheckContains(mat);
                    }
                }
                SceneInfos.Add(sceneInfo);
            }
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.UnloadUnusedAssetsImmediate();
    }

    [ContextMenu("设置全部资源Address")]
    public void SetAssetAddresses()
    {
        SetUnitsAddress();
        SetItemsAddress();
        SetAnimationsAddress();
        SetUIAddress();
        SetSFXAddress();
        SetVFXAddress();
        SetSAAddress();
        SetScenesAddress();
        SetMusicsAddress();
        SetManagersAddress();
        SetGameCoreAddress();

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
    }

    [ContextMenu("设置单位Address")]
    public void SetUnitsAddress() 
    {
        int i = 0;
        foreach (PrefabDependencyInfo prefabInfo in unitPrefabInfos)
        {
            EditorUtility.DisplayProgressBar($"设置Address", $"{prefabInfo.info.name}", i / (float)unitPrefabInfos.Count);
            i++;
            SetUnitAddressName(prefabInfo);
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置物品Address")]
    public void SetItemsAddress()
    {
        int i = 0;
        foreach (PrefabDependencyInfo prefabInfo in itemPrefabInfos)
        {
            EditorUtility.DisplayProgressBar($"设置Address", $"{prefabInfo.info.name}", i / (float)itemPrefabInfos.Count);
            i++;
            SetItemAddressName(prefabInfo);
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置动画Address")]
    public void SetAnimationsAddress()
    {
        //找到动画的Group
        AddressableAssetGroup aniGroup = settings.FindGroup("commonanimation");
        if (aniGroup == null)
            aniGroup = settings.CreateGroup("commonanimation", false, false, false, null, typeof(BundledAssetGroupSchema));
        aniGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        int i = 0;
        foreach (LargeAnimatorInfo animatorInfo in AnimatorInfos)
        {
            EditorUtility.DisplayProgressBar($"设置Address", $"{animatorInfo.AnimatorController.name}", i / (float)AnimatorInfos.Count);
            i++;

            settings.AddLabel(animatorInfo.AnimatorController.name, true);
            //设置动画控制器的Address
            string assetPath = AssetDatabase.GetAssetPath(animatorInfo.AnimatorController);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, aniGroup, false, false);
            else if (assetEntry.parentGroup != aniGroup)
                settings.MoveEntry(assetEntry, aniGroup, false, false);
            assetEntry.address = "animator/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
            assetEntry.labels.Add(animatorInfo.AnimatorController.name);

            //设置所有动画片段的Address
            foreach (AnimationClip animation in animatorInfo.Animations)
            {
                assetPath = AssetDatabase.GetAssetPath(animation);
                assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                assetEntry = settings.FindAssetEntry(assetGuid);
                if (assetEntry == null)
                    assetEntry = settings.CreateOrMoveEntry(assetGuid, aniGroup, false, false);
                else if (assetEntry.parentGroup != aniGroup)
                    settings.MoveEntry(assetEntry, aniGroup, false, false);
                assetEntry.address = assetPath;
                assetEntry.labels.Add(animatorInfo.AnimatorController.name);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置UIAddress")]
    public void SetUIAddress()
    {
        //找到Group
        AddressableAssetGroup aniGroup = settings.FindGroup("UIresource");
        if (aniGroup == null)
            aniGroup = settings.CreateGroup("UIresource", false, false, false, null, typeof(BundledAssetGroupSchema));
        aniGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        int i = 0;
        foreach (Object obj in UIResources)
        {
            EditorUtility.DisplayProgressBar($"设置UIAddress", $"{obj.name}", i / (float)AnimatorInfos.Count);
            i++;

            //设置Address
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
            {
                assetEntry = settings.CreateOrMoveEntry(assetGuid, aniGroup, false, false);
                //assetEntry.address = "UI/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置SFXAddress")]
    public void SetSFXAddress()
    {
        //找到Group
        AddressableAssetGroup group = settings.FindGroup("SFXClips");
        if (group == null)
            group = settings.CreateGroup("SFXClips", false, false, false, null, typeof(BundledAssetGroupSchema));
        group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        int i = 0;
        foreach (Object obj in SFXResources)
        {
            EditorUtility.DisplayProgressBar($"设置SFXAddress", $"{obj.name}", i / (float)AnimatorInfos.Count);
            i++;

            //设置Address
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
            {
                assetEntry = settings.CreateOrMoveEntry(assetGuid, group, false, false);
                //assetEntry.address = "UI/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
            }
            else if (assetEntry.parentGroup != group)
            {
                settings.MoveEntry(assetEntry, group, false, false);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置VFXAddress")]
    public void SetVFXAddress()
    {
        //找到Group
        AddressableAssetGroup group = settings.FindGroup("VFXresource");
        if (group == null)
            group = settings.CreateGroup("VFXresource", false, false, false, null, typeof(BundledAssetGroupSchema));
        group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        int i = 0;
        foreach (Object obj in VFXResources)
        {
            EditorUtility.DisplayProgressBar($"设置VFXAddress", $"{obj.name}", i / (float)AnimatorInfos.Count);
            i++;

            //设置Address
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
            {
                assetEntry = settings.CreateOrMoveEntry(assetGuid, group, false, false);
                //assetEntry.address = "UI/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置小动物Address")]
    public void SetSAAddress()
    {
        //找到Group
        AddressableAssetGroup group = settings.FindGroup("smallAnimals");
        if (group == null)
            group = settings.CreateGroup("smallAnimals", false, false, false, null, typeof(BundledAssetGroupSchema));
        group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        int i = 0;
        foreach (Object obj in SmallAnimalResources)
        {
            EditorUtility.DisplayProgressBar($"设置小动物Address", $"{obj.name}", i / (float)AnimatorInfos.Count);
            i++;

            //设置Address
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
            {
                assetEntry = settings.CreateOrMoveEntry(assetGuid, group, false, false);
                //assetEntry.address = "UI/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
            }
            else if (assetEntry.parentGroup != group)
            {
                settings.MoveEntry(assetEntry, group, false, false);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置场景Address")]
    public void SetScenesAddress()
    {
        //找到场景的Group
        AddressableAssetGroup sceneGroup = settings.FindGroup("scene");
        if (sceneGroup == null)
            sceneGroup = settings.CreateGroup("scene", false, false, false, null, typeof(BundledAssetGroupSchema));
        sceneGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        int i = 0;
        foreach (SceneAsset sceneAsset in SceneAssets)
        {
            EditorUtility.DisplayProgressBar($"设置Address", $"{sceneAsset.name}", i / (float)SceneAssets.Count);
            i++;
            //设置场景的Address
            string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, sceneGroup, false, false);
            else if (assetEntry.parentGroup != sceneGroup)
                settings.MoveEntry(assetEntry, sceneGroup, false, false);
            assetEntry.address = "scene/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
        }        
        //找到场景资源的Group
        sceneGroup = settings.FindGroup("sceneresource");
        if (sceneGroup == null)
            sceneGroup = settings.CreateGroup("sceneresource", false, false, false, null, typeof(BundledAssetGroupSchema));
        sceneGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        foreach (var collection in SceneInfos)
        {
            settings.AddLabel(collection.scene.name, true);

            //设置单位资源的Address

            List<Object> allObjects = new List<Object>();
            allObjects.AddRange(collection.meshes);
            allObjects.AddRange(collection.materials);
            allObjects.AddRange(collection.textures);
            allObjects.AddRange(collection.animationclips);


            foreach (Object obj in allObjects)
            {
                if (obj == null) continue;

                string objName = obj.name;

                if (objName.Contains("[") || objName.Contains("]"))
                {
                    objName = objName.Replace("[", "");
                    objName = objName.Replace("]", "");
                    obj.name = objName;
                }

                string assetPath = AssetDatabase.GetAssetPath(obj);
                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
                if (assetEntry == null)
                {
                    assetEntry = settings.CreateOrMoveEntry(assetGuid, sceneGroup, false, false);
                    if (assetPath.Contains("[")) assetPath = assetPath.Replace("[", "");
                    if (assetPath.Contains("]")) assetPath = assetPath.Replace("]", "");
                    assetEntry.address = assetPath;
                    assetEntry.labels.Add(collection.scene.name);
                }
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置音乐Address")]
    public void SetMusicsAddress()
    {
        int i = 0;
        foreach (AudioClip audioClip in BgmInfo.AudioClips)
        {
            EditorUtility.DisplayProgressBar($"设置Address", $"{audioClip.name}", i / (float)BgmInfo.AudioClips.Count);
            i++;
            //找到BGM的Group
            AddressableAssetGroup sceneGroup = settings.FindGroup("music");
            if (sceneGroup == null)
                sceneGroup = settings.CreateGroup("music", false, false, false, null, typeof(BundledAssetGroupSchema));
            sceneGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

            //设置所有BGM的Address
            string assetPath = AssetDatabase.GetAssetPath(audioClip);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, sceneGroup, false, false);
            else if (assetEntry.parentGroup != sceneGroup)
                settings.MoveEntry(assetEntry, sceneGroup, false, false);
            assetEntry.address = "music/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
        }
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu("设置管理器Address")]
    public void SetManagersAddress()
    {
        //找到GameCore的Group
        AddressableAssetGroup sceneGroup = settings.FindGroup("GameCore");
        if (sceneGroup == null)
            sceneGroup = settings.CreateGroup("GameCore", false, false, false, null, typeof(BundledAssetGroupSchema));
        sceneGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        foreach (GameObject managers in ManagerPrefabs)
        {
            //设置所有Manager的Address
            string assetPath = AssetDatabase.GetAssetPath(managers);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, sceneGroup, false, false);
            else if (assetEntry.parentGroup != sceneGroup)
                settings.MoveEntry(assetEntry, sceneGroup, false, false);
            assetEntry.address = "manager/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
        }
    }

    [ContextMenu("设置GameCoreAddress")]
    public void SetGameCoreAddress()
    {
        //找到GameCore的Group
        AddressableAssetGroup sceneGroup = settings.FindGroup("GameCore");
        if (sceneGroup == null)
            sceneGroup = settings.CreateGroup("GameCore", false, false, false, null, typeof(BundledAssetGroupSchema));
        sceneGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        foreach (GameObject component in UIComponents)
        {

            //设置所有UI的Address
            string assetPath = AssetDatabase.GetAssetPath(component);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, sceneGroup, false, false);
            else if (assetEntry.parentGroup != sceneGroup)
                settings.MoveEntry(assetEntry, sceneGroup, false, false);
            assetEntry.address = "UI/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
        }
    }

    //设置一个单位和其相关资源的地址
    public void SetUnitAddressName(PrefabDependencyInfo prefabInfo)
    {
        settings.AddLabel(prefabInfo.info.name,true);
        ScriptableObject unitinfo = prefabInfo.info;
        //找到unitInfo所在的Group
        AddressableAssetGroup dataGroup = settings.FindGroup("unitdata");
        if (dataGroup == null)
            dataGroup = settings.CreateGroup("unitdata", false, false, false, null, typeof(BundledAssetGroupSchema));
        dataGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        //找到unitPrefab所在的Group
        AddressableAssetGroup prefabGroup = settings.FindGroup("unitprefab");
        if (prefabGroup == null)
            prefabGroup = settings.CreateGroup("unitprefab", false, false, false, null, typeof(BundledAssetGroupSchema));
        prefabGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        //找到unitPortrait所在的Group
        AddressableAssetGroup iconGroup = settings.FindGroup("uniticon");
        if (iconGroup == null)
            iconGroup = settings.CreateGroup("uniticon", false, false, false, null, typeof(BundledAssetGroupSchema));
        iconGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        //找到unit资源所在的Group
        AddressableAssetGroup resourceGroup = settings.FindGroup("unitresource");
        if (resourceGroup == null)
            resourceGroup = settings.CreateGroup("unitresource", false, false, false, null, typeof(BundledAssetGroupSchema));
        resourceGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        //设置单位info的Address
        string assetPath = AssetDatabase.GetAssetPath(unitinfo);
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
        if (assetEntry == null)
            assetEntry = settings.CreateOrMoveEntry(assetGuid, dataGroup, false, false);
        else if (assetEntry.parentGroup != dataGroup)
            settings.MoveEntry(assetEntry, dataGroup, false, false);
        assetEntry.address = "unitdata/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);

        //设置单位prefab的Address
        assetPath = AssetDatabase.GetAssetPath(prefabInfo.Prefab);
        assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        assetEntry = settings.FindAssetEntry(assetGuid);
        if (assetEntry == null)
            assetEntry = settings.CreateOrMoveEntry(assetGuid, prefabGroup, false, false);
        else if (assetEntry.parentGroup != prefabGroup)
            settings.MoveEntry(assetEntry, prefabGroup, false, false);
        assetEntry.address = "unitprefab/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);

        //设置单位头像的Address
        UnitAttribute unit = prefabInfo.Prefab.GetComponent<UnitAttribute>();
        if (!unit) { Debug.Log(unitinfo.name); return; }
        if (unit.portrait)
        {
            assetPath = AssetDatabase.GetAssetPath(unit.portrait);
            assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, iconGroup, false, false);
            else if (assetEntry.parentGroup != iconGroup)
                settings.MoveEntry(assetEntry, iconGroup, false, false);
            assetEntry.address = "uniticon/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
        }

        //设置单位资源的Address

        List<Object> allObjects = new List<Object>();
        allObjects.AddRange(prefabInfo.Meshs);
        allObjects.AddRange(prefabInfo.Materials);
        allObjects.AddRange(prefabInfo.Texture2Ds);
        allObjects.AddRange(prefabInfo.Animators);
        allObjects.AddRange(prefabInfo.Animations);
        allObjects.AddRange(prefabInfo.Audios);

        foreach (Object obj in allObjects)
        {
            if (obj == null) continue;
            assetPath = AssetDatabase.GetAssetPath(obj);
            assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, resourceGroup, false, false);
            else if (assetEntry.parentGroup != resourceGroup)
                settings.MoveEntry(assetEntry, resourceGroup, false, false);
            if (assetPath.Contains("[")) assetPath = assetPath.Replace("[", "");
            if (assetPath.Contains("]")) assetPath = assetPath.Replace("]", "");
            assetEntry.address = assetPath;
            assetEntry.labels.Add(unitinfo.name);
        }
    }

    //设置一个物品和其相关资源的地址
    public void SetItemAddressName(PrefabDependencyInfo prefabInfo)
    {
        settings.AddLabel(prefabInfo.info.name,true);
        UIItemInfo item = prefabInfo.info as UIItemInfo;
        if (item == null) return;
        //找到itemInfo所在的Group
        AddressableAssetGroup dataGroup = settings.FindGroup("itemdata");
        if (dataGroup == null)
            dataGroup = settings.CreateGroup("itemdata", false, false, false, null, typeof(BundledAssetGroupSchema));
        dataGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        //找到itemPrefab所在的Group
        AddressableAssetGroup prefabGroup = settings.FindGroup("itemprefab");
        if (prefabGroup == null)
            prefabGroup = settings.CreateGroup("itemprefab", false, false, false, null, typeof(BundledAssetGroupSchema));
        prefabGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        //找到itemPortrait所在的Group
        AddressableAssetGroup iconGroup = settings.FindGroup("itemicon");
        if (iconGroup == null)
            iconGroup = settings.CreateGroup("itemicon", false, false, false, null, typeof(BundledAssetGroupSchema));
        iconGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        //找到item资源所在的Group
        AddressableAssetGroup resourceGroup = settings.FindGroup("itemresource");
        if (resourceGroup == null)
            resourceGroup = settings.CreateGroup("itemresource", false, false, false, null, typeof(BundledAssetGroupSchema));
        resourceGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

        //设置itemInfo的Address
        string assetPath = AssetDatabase.GetAssetPath(item);
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
        if (assetEntry == null)
            assetEntry = settings.CreateOrMoveEntry(assetGuid, dataGroup, false, false);
        else if (assetEntry.parentGroup != dataGroup)
            settings.MoveEntry(assetEntry, dataGroup, false, false);
        assetEntry.address = "itemdata/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);

        //设置ItemPrefab的Address
        if (prefabInfo.Prefab && prefabInfo.Prefab.name != "Itembag")
        {
            assetPath = AssetDatabase.GetAssetPath(prefabInfo.Prefab);
            assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, prefabGroup, false, false);
            else if (assetEntry.parentGroup != prefabGroup)
                settings.MoveEntry(assetEntry, prefabGroup, false, false);
            assetEntry.address = "itemprefab/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
        }

        //设置ItemIcon的Address
        if (item.Icon)
        {
            assetPath = AssetDatabase.GetAssetPath(item.Icon);
            assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, iconGroup, false, false);
            else if (assetEntry.parentGroup != iconGroup)
                settings.MoveEntry(assetEntry, iconGroup, false, false);
            assetEntry.address = "itemicon/" + Path.GetFileNameWithoutExtension(assetEntry.AssetPath);
        }

        //设置物品资源的Address

        List<Object> allObjects = new List<Object>();
        allObjects.AddRange(prefabInfo.Meshs);
        allObjects.AddRange(prefabInfo.Materials);
        allObjects.AddRange(prefabInfo.Texture2Ds);
        allObjects.AddRange(prefabInfo.Animators);
        allObjects.AddRange(prefabInfo.Animations);
        allObjects.AddRange(prefabInfo.Audios);

        foreach (Object obj in allObjects)
        {
            if (obj == null) continue;
            assetPath = AssetDatabase.GetAssetPath(obj);
            assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, resourceGroup, false, false);
            else if (assetEntry.parentGroup != resourceGroup)
                settings.MoveEntry(assetEntry, resourceGroup, false, false);
            if (assetPath.Contains("[")) assetPath = assetPath.Replace("[", "");
            if (assetPath.Contains("]")) assetPath = assetPath.Replace("]", "");
            assetEntry.address = assetPath;
            assetEntry.labels.Add(item.name);
        }
    }

    [ContextMenu("单独收集预制件引用")]
    public void CollectGameObjectDependencies()
    {
        if (PreObjects != null)
        {
            otherPrefabInfos = new List<PrefabDependencyInfo>();

            int i = 0;
            foreach (GameObject prefab in PreObjects)
            {
                EditorUtility.DisplayProgressBar($"正在收集依赖：GameObject/{prefab.name}", $"GameObject/{prefab.name}", i / (float)PreObjects.Count);
                i++;

                PrefabDependencyInfo prefabInfo = new PrefabDependencyInfo
                {
                    Prefab = prefab,
                    Materials = new List<Material>(),
                    Meshs = new List<Mesh>(),
                    Texture2Ds = new List<Texture2D>(),
                    Animators = new List<RuntimeAnimatorController>(),
                    Animations = new List<AnimationClip>(),
                    Audios = new List<AudioClip>()
                };

                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { prefab });
                if (dependencies == null) return;

                i = 0;
                foreach (Object dependency in dependencies)
                {
                    EditorUtility.DisplayProgressBar($"正在收集资源引用：预制件/{prefab.name}", $"UI/{dependency.name}", i / (float)dependencies.Length);
                    i++;

                    if (dependency is Texture2D texture) prefabInfo.Texture2Ds.Add(texture);
                    if (dependency is Mesh mesh) prefabInfo.Meshs.Add(mesh);
                    if (dependency is RuntimeAnimatorController controller) prefabInfo.Animators.Add(controller);
                    if (dependency is AnimationClip animation) prefabInfo.Animations.Add(animation);
                    if (dependency is AudioClip audioc) prefabInfo.Audios.Add(audioc);
                    if (dependency is Material material) prefabInfo.Materials.Add(material);
                }

                otherPrefabInfos.Add(prefabInfo);
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.UnloadUnusedAssetsImmediate();
        }
    }
    public string prefabsGroupName;
    [ContextMenu("单独设置预制件Address")]
    public void SetPrefabsAddress()
    {
        foreach (PrefabDependencyInfo prefabInfo in otherPrefabInfos)
        {
            //找到Prefab所在的Group
            AddressableAssetGroup prefabGroup = settings.FindGroup(prefabsGroupName);
            if (prefabGroup == null)
                prefabGroup = settings.CreateGroup(prefabsGroupName, false, false, false, null, typeof(BundledAssetGroupSchema));
            prefabGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

            //设置单位prefab的Address
            string assetPath = AssetDatabase.GetAssetPath(prefabInfo.Prefab);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null)
                assetEntry = settings.CreateOrMoveEntry(assetGuid, prefabGroup, false, false);
            else if (assetEntry.parentGroup != prefabGroup)
                settings.MoveEntry(assetEntry, prefabGroup, false, false);

            //设置资源的Address

            List<Object> allObjects = new List<Object>();
            allObjects.AddRange(prefabInfo.Meshs);
            allObjects.AddRange(prefabInfo.Materials);
            allObjects.AddRange(prefabInfo.Texture2Ds);
            allObjects.AddRange(prefabInfo.Animators);
            allObjects.AddRange(prefabInfo.Animations);
            allObjects.AddRange(prefabInfo.Audios);

            foreach (Object obj in allObjects)
            {
                if (obj == null) continue;
                assetPath = AssetDatabase.GetAssetPath(obj);
                assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                assetEntry = settings.FindAssetEntry(assetGuid);
                if (assetEntry == null)
                    assetEntry = settings.CreateOrMoveEntry(assetGuid, prefabGroup, false, false);
                else if (assetEntry.parentGroup != prefabGroup)
                    settings.MoveEntry(assetEntry, prefabGroup, false, false);
                if (assetPath.Contains("[")) assetPath.Replace("[", "");
                if (assetPath.Contains("]")) assetPath.Replace("]", "");
                assetEntry.address = assetPath;
            }
        }
    }

    [ContextMenu("资源收集清空")]
    public void ClearDependencies()
    {
        unitPrefabInfos.Clear();
        itemPrefabInfos.Clear();
        foreach (LargeAnimatorInfo animatorInfo in AnimatorInfos) animatorInfo.Animations.Clear();
        BgmInfo.AudioClips.Clear();
        otherPrefabInfos.Clear();
    }
}
#region Utility Class
[Serializable]
public class LargeAnimatorInfo
{
    public RuntimeAnimatorController AnimatorController;
    public List<AnimationClip> Animations;
}

[Serializable]
public class BgmInfo
{
    public AudioSoundsManager BgmDatabase;
    public List<AudioClip> AudioClips;
}

[Serializable]
public class PrefabDependencyInfo
{
    public ScriptableObject info;
    public GameObject Prefab;
    public List<Mesh> Meshs;
    public List<Material> Materials;
    public List<Texture2D> Texture2Ds;
    public List<RuntimeAnimatorController> Animators;
    public List<AnimationClip> Animations;
    public List<AudioClip> Audios;
}

[Serializable]
public class SceneDependencyInfo
{
    public SceneAsset scene;
    public List<Mesh> meshes;
    public List<Texture> textures;
    public List<Material> materials;
    public List<AnimationClip> animationclips;
}
#endregion