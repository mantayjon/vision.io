// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Microsoft.MixedReality.SampleQRCodes
{
    public class QRCodesVisualizer : MonoBehaviour
    {
        public GameObject interactiveCity;
        public List<string> validQrCodes;

        private SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        private bool clearExisting = false;

        struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private Queue<ActionData> pendingActions = new Queue<ActionData>();

        // Use this for initialization
        void Start()
        {
            Debug.Log("QRCodesVisualizer start");
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
            if (interactiveCity == null)
            {
                throw new System.Exception("Prefab not assigned");
            }
        }
        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            if (!status)
            {
                clearExisting = true;
            }
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeAdded");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeRemoved");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added)
                    {
                        string qrCodeData = action.qrCode.Data;
                        destroyDuplicatePrefabs();
                        if (validQrCodes.Contains(qrCodeData)){
                            GameObject qrCodeObject = Instantiate(interactiveCity, new Vector3(0, 0, 0), Quaternion.identity);
                            qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                            // qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;

                            qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                        }

                    }
                    else if (action.type == ActionData.Type.Updated)
                    {
                        destroyDuplicatePrefabs();
                        string qrCodeData = action.qrCode.Data;
                        if (validQrCodes.Contains(qrCodeData))
                        {
                            if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                            {

                                GameObject qrCodeObject = Instantiate(interactiveCity, new Vector3(0, 0, 0), Quaternion.identity);
                                qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                                // qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                                qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                            }
                        }
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                        }
                    }
                }
            }
            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList.Clear();

            }
        }

        void destroyDuplicatePrefabs()
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("CityTemplate");
            if (objectsWithTag.Length > 1)
            {
                foreach (GameObject obj in objectsWithTag)
                {
                    Destroy(obj);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }
    }
}
