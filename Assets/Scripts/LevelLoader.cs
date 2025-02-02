﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public GameObject loadingScreen;
    public GameObject progressScreen;
    [SerializeField]Image progressBar;
    //[SerializeField]Image progressBarframe;
    public GameObject Rocket;
    public GameObject Target;
    public BGMManager BGMManager;
    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(1));
    }
    IEnumerator LoadLevel(int sceneIndex)
    {
        yield return null;
        //progressBar.enabled = true;
        //progressBarframe.enabled = true;
        transition.SetTrigger("Start");
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        op.allowSceneActivation = false;

        loadingScreen.SetActive(false);
        progressScreen.SetActive(true);
        float timer = 0.0f; 
        while (!op.isDone)
        {
            yield return null; 
            timer += Time.deltaTime;
            if(op.progress < 0.9f)
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                //Debug.Log(op.progress);
                Rocket.transform.position = (Target.transform.position - Rocket.transform.position) * op.progress + Rocket.transform.position;
                //Rocket.transform.position = new Vector3(Mathf.Lerp(Rocket.transform.position.x, Target.transform.position.x, timer), Target.transform.position.y, 0);
                if (progressBar.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                //Rocket.transform.position = Target.transform.position;
                Rocket.transform.position = (Target.transform.position - Rocket.transform.position) * op.progress + Rocket.transform.position;
                if (progressBar.fillAmount >= 0.99f)
                {
                    op.allowSceneActivation = true; 
                    yield break;
                }
            }
        }

        BGMManager.Transition();
    }
}