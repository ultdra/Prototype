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
            FadeController.Instance.FadeIn(0.25f, () =>
            {
                // Load the next scene after fade completes
                SceneManager.LoadScene(m_SceneToLoad);
                
                //Improvement can be done with a additive scene load and handling
                FadeController.Instance.FadeOut(0.25f);
            });
        }
    }
}