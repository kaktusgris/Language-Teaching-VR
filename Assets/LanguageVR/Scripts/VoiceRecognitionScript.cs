using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
namespace NTNU.CarloMarton.VRLanguage
{
    public class VoiceRecognitionScript : MonoBehaviour
    {
        private static VR_VoiceRecognition.VoiceRecognition voiceRec = new VR_VoiceRecognition.VoiceRecognition();
        private static bool voiceRec_busy = false;
        private bool voiceRec_Result = false;
        private static IDisposable speechCoroutine;
        private static CancellationTokenSource cts;
        private static CancellationToken cts_token;


        //-------------------------------------------------
        // Singleton instance of the VoiceRecognitionScript. Only one can exist at a time.
        //-------------------------------------------------
        private static VoiceRecognitionScript _instance;
        public static VoiceRecognitionScript instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<VoiceRecognitionScript>();
                }
                return _instance;
            }
        }


        static void InitiateSpeechRecognition(int duration_in_seconds, string inputWord)
        {
            if (!voiceRec_busy)
            {
                //_instance.StartCoroutine(_instance.WaitForSpeech(duration_in_seconds, inputWord));
                _instance.InitiateSpeechRecognition(duration_in_seconds, inputWord, true);
            }

        }
   
        private void InitiateSpeechRecognition(int duration_in_seconds, string inputWord, bool spawnItem)
        {
            try{
                speechCoroutine = Observable.FromCoroutine<bool>((observer) => WaitForSpeech(observer, duration_in_seconds, inputWord)).Subscribe(x =>
                    {
                        voiceRec_Result = x;
                        if (voiceRec_Result && spawnItem)
                        {
                            SpawnInteractableObject(inputWord);
                        }
                        //Dispose CancellationTokenSource after it has completed
                        cts.Dispose();
                        voiceRec_busy = false;
                       
                    });
               }
               finally
               {
                    speechCoroutine.Dispose();  
               }   
        }

        void SpawnInteractableObject(string objectName)
        {
            Vector3 headPosition = ViveManager.Instance.head.transform.position;
            Vector3 buttonPosition = transform.position;

            float spawnX = (headPosition.x + buttonPosition.x) / 2f;
            float spawnY = (headPosition.y + buttonPosition.y) / 2f;
            float spawnZ = (headPosition.z + buttonPosition.z) / 2f;

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);

            Debug.LogFormat("Instantiated {0} at {1}", objectName, spawnPosition);
            GameObject interactableObject = PhotonNetwork.Instantiate("InteractableObjects/" + objectName, spawnPosition, Quaternion.identity);
        }

        static void CancelSpeechRecognition()
        {
            if (voiceRec_busy)
            {
                cts.Cancel();
                cts.Dispose();
                //speechCoroutine.Dispose();
                voiceRec_busy = false;
            }
        }

        IEnumerator WaitForSpeech(IObserver<bool> observer, int duration_in_seconds, string inputWord)
        {
            Debug.Log("Starting up");
            voiceRec_busy = true;

            cts = new CancellationTokenSource();
            CancellationToken cts_token = cts.Token;

            Task<bool> t = Task.Run(() => voiceRec.StartSpeechRecognition(duration_in_seconds, inputWord), cts_token);

            while (!(t.IsCompleted || t.IsCanceled))
            {
                yield return null;
            }


            if (t.Status != TaskStatus.RanToCompletion)
            {

                yield break;
            }
            else
            {
                bool result = t.Result;
                //yield return result;
                voiceRec_busy = false;
                observer.OnNext(result);
                observer.OnCompleted();
            }

        }
    }
}
