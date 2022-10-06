using UnityEngine;
using UnityEngine.XR.MagicLeap;
using MagicLeap.Core;
using System.Collections.Generic;

public class PersistentContentExample : MonoBehaviour
{
    [Tooltip("The object that will be created when pressing the bumper, and will persist between reboots.")]
    public GameObject PersistentObject;

    //Track the objects we already created to avoid duplicates
    private Dictionary<string, GameObject> _persistentObjectsById = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //PCFs are only valid when building for Lumin
#if PLATFORM_LUMIN

        //Ask the Magic Leap to start looking for Persistent Coordinate Frames.
        //The result will let us know if the service could start.
        MLResult result = MLPersistentCoordinateFrames.Start();

        //If our request was not successful...
        if (!result.IsOk)
        {
            //Inform the user about the error in the debug log.
            Debug.LogError("Error: Failed starting MLPersistentCoordinateFrames, disabling script. Reason:" + result);

            //Since we need the service to start successfully, we disable the script if it doesn't.
            enabled = false;

            //Return to prevent further initialization.
            return;
        }

        //Handle Localization status changes.
        MLPersistentCoordinateFrames.OnLocalized += HandleOnLocalized;

        //Handle controller button down events.
        MLInput.OnControllerButtonDown += MLInputOnOnControllerButtonDown;

#endif
    }

    //The Magic Leap Controller and PCFs can only be used when building for Lumin
#if PLATFORM_LUMIN

    //Called when the user pressed a button down on the Magic Leap controller
    private void MLInputOnOnControllerButtonDown(byte controllerId, MLInput.Controller.Button button)
    {
        //Check to see if the button that was pressed was the Bumper.
        if (button == MLInput.Controller.Button.Bumper)
        {
            //If it was the bumper, get the controller using the controllerId
            var controller = MLInput.GetController(controllerId);

            //Create a new object with the controller's position and rotation
            var persistentObject = Instantiate(PersistentObject, controller.Position, controller.Orientation);

            //Find the closest PCF relative to the controller's position.
            MLPersistentCoordinateFrames.FindClosestPCF(controller.Position, out MLPersistentCoordinateFrames.PCF pcf);

            //Create a new Transform Binding.
            var persistentBinding = new TransformBinding(persistentObject.GetInstanceID().ToString(), "exampleItem");

            //Bind the newly created transform to it. 
            persistentBinding.Bind(pcf, persistentObject.transform);

            //Track the created object to avoid duplicates.
            _persistentObjectsById.Add(persistentObject.GetInstanceID().ToString(),persistentObject);

            //Debug which PCF the new object was bound to.
            Debug.Log("Transform bound to PCF : " + pcf.CFUID);
        }

    }

    //Called when the Magic Leap's localization status changes.
    private void HandleOnLocalized(bool localized)
    {
        //Read the saved files from storage
        TransformBinding.storage.LoadFromFile();

        //Cache the reference to the Transform Bindings
        List<TransformBinding> allBindings = TransformBinding.storage.Bindings;

        //Debug that the bindings are being restored
        Debug.Log("Getting saved bindings..." );

        foreach (TransformBinding storedBinding in allBindings)
        {
            // Try to find the PCF with the stored CFUID.
            MLResult result = MLPersistentCoordinateFrames.FindPCFByCFUID(storedBinding.PCF.CFUID, out MLPersistentCoordinateFrames.PCF pcf);

            //If the current map contains the PCF and the PCF is being tracked and we have not created the object already...
            if (pcf != null && MLResult.IsOK(pcf.CurrentResultCode) && _persistentObjectsById.ContainsKey(storedBinding.Id) == false)
            {
                //Create a the persistent content
                GameObject gameObj = Instantiate(PersistentObject, Vector3.zero, Quaternion.identity);

                //Bind the new object to the Transform binding. Setting the "regain" condition true, will position the transform at the saved position.
                storedBinding.Bind(pcf, gameObj.transform, true);

                //Track the created object to avoid duplicates.
                _persistentObjectsById.Add(storedBinding.Id, gameObj);

                //Debug that a binding was restored.
                Debug.Log("Restored bound transform at PCF : " + pcf.CFUID);

            }
        }
    }
#endif

}