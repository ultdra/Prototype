using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class TriggerBehaviour : MonoBehaviour
{
    [SerializeField]
    private string m_OtherColliderName = "";

    [SerializeField]
    private string m_SceneToLoad = "";


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag(m_OtherColliderName) && m_SceneToLoad != "")
        {
            // Load the next scene
            SceneManager.LoadScene(m_SceneToLoad);
        }
    }
}