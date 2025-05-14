using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Runtime.CompilerServices;

public class SceneInfo : MonoBehaviour
{
    private static SceneInfo instance;
    public EffectMesh EffectiveMesh;

    private float cooldownTime = 1f; // 冷却时间1秒
    private float lastToggleTime;

    public static SceneInfo Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("SceneInfo is not initialized!");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            // 检查是否超过冷却时间
            if (Time.time - lastToggleTime >= cooldownTime)
            {
                // 切换网格的显示状态
                EffectiveMesh.HideMesh = !EffectiveMesh.HideMesh;
            }
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            // 检查是否超过冷却时间
            if (Time.time - lastToggleTime >= cooldownTime)
            {
                // 更新场景信息
                UpdateSceneInfo();
                // 更新上次操作时间
                lastToggleTime = Time.time;
            }
        }

    }

    private void UpdateSceneInfo()
    {
        string sceneInfoText = "SceneInfo: ";
        MRUKRoom sceneInfo = MRUK.Instance.GetCurrentRoom();
        if (sceneInfo != null)
        {
            List <MRUKAnchor> anchors = sceneInfo.Anchors;
            if (anchors != null && anchors.Count > 0)
            {
                foreach (MRUKAnchor anchor in anchors)
                {
                    if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR ||
                    anchor.Label == MRUKAnchor.SceneLabels.CEILING ||
                    anchor.Label == MRUKAnchor.SceneLabels.WALL_FACE ||
                    anchor.Label == MRUKAnchor.SceneLabels.GLOBAL_MESH)
                    {
                        continue;
                    }
                    else
                    {
                        sceneInfoText += $"Object Name: {anchor.Label}, Position: {anchor.GetAnchorCenter()}\n";
                    }
                }
                UIController.Instance.UpdateSceneText(sceneInfoText);
            }
            else
            {
                UIController.Instance.AddLogText("SceneInfo: No anchors found in the current scene.");
            }
        }
        else
        {
            UIController.Instance.AddLogText("SceneInfo: No scene information available.");
        }
    }

}
