using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MakeModelInteractable : MonoBehaviour
{
    public Material solidTerrain, solidBuilding, passthroughBuilding, passthroughTerrain;
    public GameObject Terrain, virtBuildings;

    private bool isVirtualShader;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Called makeModelInteractable.Start");
        SetMaterialInChildren(solidBuilding, true);

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetChild(0).gameObject; // due to layers, every model is wrapped in their own obj
            
            child.AddComponent<CityBlockEvacuation>();

            var mc = child.GetComponent<MeshCollider>();
            if (mc == null)
            {
                mc = child.AddComponent<MeshCollider>();
            }
            mc.convex = true;
            mc.isTrigger = false;

            var renderer = child.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;

            /*
            child.tag = "Interaction";
            var colliderSurface = child.AddComponent<ColliderSurface>();
            colliderSurface.InjectCollider(mc);
            var rayInteractable = child.AddComponent<RayInteractable>();
            rayInteractable.InjectSurface(colliderSurface);*/
            
        }
        /*
        for (int i = 0; i < virtBuildings.transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetChild(0).gameObject;
            var obst = child.AddComponent<NavMeshObstacle>();
            obst.carving = true;
        }*/
        //Terrain.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private void SetMaterialInChildren(Material material, bool shadows)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetChild(0).gameObject; // due to layers, every model is wrapped in their own obj
            
            var renderer = child.GetComponent<Renderer>();
            renderer.material = material;
            renderer.receiveShadows = shadows;

        }
    }

    public void ToggleShader()
    {
        if (isVirtualShader)
        {
            SetMaterialInChildren(solidBuilding, true);
            var r = Terrain.GetComponent<Renderer>();
            r.material = solidTerrain;
            r.receiveShadows = true;
        }
        else
        {
            SetMaterialInChildren(passthroughBuilding, true);
            var r = Terrain.GetComponent<Renderer>();
            r.material = passthroughTerrain;
            r.receiveShadows = true;
        }
        isVirtualShader = !isVirtualShader;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
