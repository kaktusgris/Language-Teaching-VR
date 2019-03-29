using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
namespace NTNU.CarloMarton.VRLanguage
{
    public class VoiceRecognitionScript : MonoBehaviour, IObservable<VoiceRecognitionStatus>
    {
        private static VR_VoiceRecognition.VoiceRecognition voiceRec = new VR_VoiceRecognition.VoiceRecognition();
        private static bool voiceRec_busy = false;
        private bool voiceRec_Result = false;
        private Coroutine speechCoroutine;
        //private static IDisposable speechCoroutine;
        private static CancellationTokenSource cts;
        private static CancellationToken cts_token;
        private List<IObserver<VoiceRecognitionStatus>> observers = new List<IObserver<VoiceRecognitionStatus>>();
        private List<VoiceRecognitionStatus> status = new List<VoiceRecognitionStatus>();

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


        public static void InitiateSpeechRecognition(int duration_in_seconds, string inputWord)
        {
            if (!voiceRec_busy)
            {
                instance.speechCoroutine = instance.StartCoroutine(instance.WaitForSpeech(duration_in_seconds, inputWord));
            }

        }
   
        private void SpawnInteractableObject(string objectName)
        {
            Vector3 headPosition = ViveManager.Instance.head.transform.position;
            
            Vector3 playerDirection = ViveManager.Instance.head.transform.forward;
            Quaternion playerRotation = ViveManager.Instance.head.transform.rotation;
            float spawnDistance = 1;

            Vector3 spawnPos = headPosition + playerDirection * spawnDistance;

            Debug.LogFormat("Instantiated {0} at {1}", objectName, spawnPos);
            GameObject interactableObject = PhotonNetwork.Instantiate("InteractableObjects/" + objectName, spawnPos, Quaternion.identity);

            cts.Dispose();
        }

        public static void CancelSpeechRecognition()
        {
            if (voiceRec_busy)
            {
                cts.Cancel();
                cts.Dispose();
                _instance.StopCoroutine(_instance.speechCoroutine);
                instance.observers.Clear();
                Debug.Log("Coroutine successfully cancelled.");
                voiceRec_busy = false;
            }
        }

        IEnumerator WaitForSpeech(int duration_in_seconds, string inputWord)
        {
            Debug.Log("Speech recognition starting up. Searching for: " + inputWord);
            voiceRec_busy = true;

            cts = new CancellationTokenSource();
            CancellationToken cts_token = cts.Token;

            //Remove integers in string and white space
            string filteredInputWord = Regex.Replace(inputWord, @"[\d-]", string.Empty).Trim();

            Task<bool> t = Task.Run(() => voiceRec.StartSpeechRecognition(duration_in_seconds, filteredInputWord), cts_token);

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
                VoiceRecognitionStatus voiceRec_status = new VoiceRecognitionStatus(result);
                Debug.Log(voiceRec_status);

                foreach(var observer in observers)
                {
                    observer.OnNext(voiceRec_status);
                    observer.OnCompleted();
                } 

                yield return result;

                if (result)
                {
                    SpawnInteractableObject(inputWord);
                }
                voiceRec_busy = false;
            }

        }

        public IDisposable Subscribe(IObserver<VoiceRecognitionStatus> observer)
        {
            // Check whether observer is already registered. If not, add it
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                // Provide observer with existing data.
                foreach (var item in status)
                    observer.OnNext(item);
            }
            return new Unsubscriber<VoiceRecognitionStatus>(observers, observer);
        }

        internal class Unsubscriber<VoiceRecognitionStatus> : IDisposable
        {
            private List<IObserver<VoiceRecognitionStatus>> _observers;
            private IObserver<VoiceRecognitionStatus> _observer;

            internal Unsubscriber(List<IObserver<VoiceRecognitionStatus>> observers, IObserver<VoiceRecognitionStatus> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }

    public class VoiceRecognitionStatus
    {
        private bool voiceRec_status;

        internal VoiceRecognitionStatus(bool voiceRec_status)
        {
            this.voiceRec_status = voiceRec_status;
        }

        public bool Status
        {
            get { return voiceRec_status; }
        }
    }

}
