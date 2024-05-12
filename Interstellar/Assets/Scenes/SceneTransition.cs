using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string sceneName;

    // Lodas the scene when the player enters the trigger
    void OnTriggerEnter(Collider entity) {
        if (entity.CompareTag("Player")) {
            Debug.Log("Player has entered the trigger, loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}
