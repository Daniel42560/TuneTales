using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public Transform LaunchPoint;

    public void FireProjectile()
    {
        Instantiate(ProjectilePrefab, LaunchPoint.position, ProjectilePrefab.transform.rotation);
    }
}
