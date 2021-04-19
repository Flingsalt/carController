using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//For controlling a car, inherits base class UnitController which allows for delegated AI object use, add AI object to the car controller

public class CarController : MonoBehaviour{
    //Front, Left, Right, Rear aka. FrontLeft(FL) etc.
    public WheelCollider wheelColliderFL;
    public WheelCollider wheelColliderFR;
    public WheelCollider wheelColliderRL;
    public WheelCollider wheelColliderRR;

    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;

    public Transform centerOfMass;

    public Light headlightLeftTop;
    public Light headlightLeftBottom;
    public Light headlightRightTop;
    public Light headlightRightBottom;

    bool m_headlightsToggle = true;

    private float centreHealth = 100000.0f;
    private float maxSpeed = 150.0f;


    public CarType cartype;
    
    public AIType ai;
    public Rigidbody rigidBody;
    private bool braking = false;

    public event Action<CarController> OnDeathEvent;
    //public event Action<CarController,float> OnCheckpointEvent;

    void Start(){
        rigidBody = GetComponent<Rigidbody>();        
        rigidBody.centerOfMass = centerOfMass.localPosition;
    }
    
    private void OnTriggerEnter(Collider other) {
        //OnCheckpointEvent?.Invoke(this, amount);

     
        
            if (other.CompareTag("SlowDown"))
            {
                Brake();
            }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SlowDown"))
        {
           
            StopBrake();
        }
    }

    public void ToggleHeadlights() {
        m_headlightsToggle = !m_headlightsToggle;
        headlightLeftTop.enabled = m_headlightsToggle;
        headlightLeftBottom.enabled = m_headlightsToggle;
        headlightRightTop.enabled = m_headlightsToggle;
        headlightRightBottom.enabled = m_headlightsToggle;
    }

    private void OnCollisionStay(Collision collision) {ApplyCollision(collision);}
    private void OnCollisionEnter(Collision collision) {ApplyCollision(collision);}

    void ApplyCollision(Collision collision) {
        float forceMagnitude = collision.impulse.magnitude;
        //print(forceMagnitude.ToString());
        centreHealth -= forceMagnitude;
        if (centreHealth <= 0.0f) OnDeathEvent?.Invoke(this);
    }
    
    public void Accelerate(float yinput) {
        if (braking) return;
        wheelColliderRL.motorTorque = yinput * cartype.GetTorqueModifier();// * Time.deltaTime;
        wheelColliderRR.motorTorque = yinput * cartype.GetTorqueModifier();// * Time.deltaTime;
        MaxSpeedLimitBounds(maxSpeed);
    }
    public void Turn(float xinput) {
        wheelColliderFL.steerAngle = xinput * cartype.GetSteeringResponsiveness() * 25.0f;// * Time.deltaTime;
        wheelColliderFR.steerAngle = xinput * cartype.GetSteeringResponsiveness() * 25.0f;// * Time.deltaTime;

        //wheelColliderFL.steerAngle = Mathf.LerpAngle(wheelColliderFL.steerAngle, xinput * cartype.GetMaxSteeringAngle(), Time.deltaTime * cartype.GetSteeringResponsiveness());
        //wheelColliderFR.steerAngle = Mathf.LerpAngle(wheelColliderFR.steerAngle, xinput * cartype.GetMaxSteeringAngle(), Time.deltaTime * cartype.GetSteeringResponsiveness());
    }

    public void Brake() {
        braking = true;
        wheelColliderRL.brakeTorque = 4000.0f;
        wheelColliderRR.brakeTorque = 4000.0f;
        wheelColliderRL.motorTorque = 0.0f;
        wheelColliderRR.motorTorque = 0.0f;
    }

    public void StopBrake() {
        braking = false;
        wheelColliderRL.brakeTorque = 0.0f;
        wheelColliderRR.brakeTorque = 0.0f;
    }
    public bool isBraking() {
        return braking;
    }
    
   

    void FixedUpdate(){

      
        ai.Control(this);
    }

    public bool isDead() {
        if (centreHealth <= 0.0f) return true;
        return false;
    }

    public float GetVelocityKMH() {
        return rigidBody.velocity.magnitude * 3.6f;
    }

    public void ModifySpeed(float _multiplier) {
        rigidBody.velocity*= _multiplier;
        MaxSpeedLimitBounds(maxSpeed);
    }
    public void MaxSpeedLimitBounds(float _bounds) {
        if (GetVelocityKMH() > maxSpeed) {
            Vector3 vec = rigidBody.velocity.normalized * maxSpeed;
            vec /= 3.6f;
            rigidBody.velocity = vec;
        }
    }

    public float GetLife() {
        return centreHealth;
    }
    public float GetLifeRelative() {
        return centreHealth / 100000.0f;//cartype.maxHealth;
    }

    private void Update() {
        var pos = Vector3.zero;
        var rot = Quaternion.identity;

        wheelColliderFL.GetWorldPose(out pos, out rot);
        wheelFL.position = pos;
        wheelFL.rotation = rot;

        wheelColliderFR.GetWorldPose(out pos, out rot);        
        wheelFR.position = pos;
        wheelFR.rotation = rot * Quaternion.Euler(0,180,0);//Rotate 180 on y

        if (braking) return;//only show front wheel spin/rotation during braking

        wheelColliderRL.GetWorldPose(out pos, out rot);
        wheelRL.position = pos;
        wheelRL.rotation = rot;

        wheelColliderRR.GetWorldPose(out pos, out rot);
        wheelRR.position = pos;
        wheelRR.rotation = rot * Quaternion.Euler(0, 180, 0);//Rotate 180 on y
    }
}















//print(collision.GetContact(0).thisCollider.name);
//Vector3.Dot(collision.contacts[0].normal, collision.relativeVelocity) * rigidBody.mass;
//Vector3.Magnitude(rigidBody.velocity);


//float vertInput = Input.GetAxis("Vertical");
//wheelColliderRL.motorTorque = vertInput * torqueModifier;
//wheelColliderRR.motorTorque = vertInput * torqueModifier;


//float horizInput = Input.GetAxis("Horizontal");
//wheelColliderFL.steerAngle = horizInput * steeringModifier;
//wheelColliderFR.steerAngle = horizInput * steeringModifier;
//Angular drag bromsar rotation, använd senare
//if (braking) rigidBody.angularDrag = 1.0f;
//else rigidBody.angularDrag = 0.0f;



//if (wheelFL == null) wheelFL = gameObject.AddComponent<Transform>();
//if (wheelFR == null) wheelFR = gameObject.AddComponent<Transform>();
//if (wheelRL == null) wheelRL = gameObject.AddComponent<Transform>();
//if (wheelRR == null) wheelRR = gameObject.AddComponent<Transform>();


//wheelFL.position = cartype.chassisType.axelPoints[i].pos;
//wheelFR.position = cartype.chassisType.axelPoints[i].pos;
//wheelRL.position = cartype.chassisType.axelPoints[i].pos;
//wheelRR.position = cartype.chassisType.axelPoints[i].pos;

//wheelFL.rotation = cartype.chassisType.axelPoints[i].rot;
//wheelFR.rotation = cartype.chassisType.axelPoints[i].rot;
//wheelRL.rotation = cartype.chassisType.axelPoints[i].rot;
//wheelRR.rotation = cartype.chassisType.axelPoints[i].rot;

//wheelFL.localScale = cartype.chassisType.axelPoints[i].sca;
//wheelFR.localScale = cartype.chassisType.axelPoints[i].sca;
//wheelRL.localScale = cartype.chassisType.axelPoints[i].sca;
//wheelRR.localScale = cartype.chassisType.axelPoints[i].sca;