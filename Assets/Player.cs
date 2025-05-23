using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // External tunables.
    static public float m_fMaxSpeed = 0.10f;
    public float m_fSlowSpeed = m_fMaxSpeed * 0.66f;
    public float m_fIncSpeed = 0.0025f;
    public float m_fMagnitudeFast = 0.6f;
    public float m_fMagnitudeSlow = 0.06f;
    public float m_fFastRotateSpeed = 0.2f;
    public float m_fFastRotateMax = 10.0f;
    public float m_fDiveTime = 0.3f;
    public float m_fDiveRecoveryTime = 0.5f;
    public float m_fDiveDistance = 3.0f;

    // Internal variables.
    public Vector3 m_vDiveStartPos;
    public Vector3 m_vDiveEndPos;
    public float m_fAngle;
    public float m_fSpeed;
    public float m_fTargetSpeed;
    public float m_fTargetAngle;
    public eState m_nState;
    public float m_fDiveStartTime;

    public enum eState : int
    {
        kMoveSlow,
        kMoveFast,
        kDiving,
        kRecovering,
        kNumStates
    }

    private Color[] stateColors = new Color[(int)eState.kNumStates]
    {
        new Color(0,     0,   0),
        new Color(255, 255, 255),
        new Color(0,     0, 255),
        new Color(0,   255,   0),
    };

    public bool IsDiving()
    {
        return (m_nState == eState.kDiving);
    }

    void CheckForDive()
    {
        if (Input.GetMouseButton(0) && (m_nState != eState.kDiving && m_nState != eState.kRecovering))
        {
            // Start the dive operation
            m_nState = eState.kDiving;
            m_fSpeed = 0.0f;

            // Store starting parameters.
            m_vDiveStartPos = transform.position;
            m_vDiveEndPos = m_vDiveStartPos - (transform.right * m_fDiveDistance);
            m_fDiveStartTime = Time.time;
        }
    }

    void Start()
    {
        // Initialize variables.
        m_fAngle = 0;
        m_fSpeed = 0;
        m_nState = eState.kMoveSlow;
    }

    void UpdateDirectionAndSpeed()
    {
        // Get relative positions between the mouse and player
        Vector3 vScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 vScreenSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 vOffset = new Vector2(transform.position.x - vScreenPos.x, transform.position.y - vScreenPos.y);

        // Find the target angle being requested.
        m_fTargetAngle = Mathf.Atan2(vOffset.y, vOffset.x) * Mathf.Rad2Deg;

        // Calculate how far away from the player the mouse is.
        float fMouseMagnitude = vOffset.magnitude / vScreenSize.magnitude;

        // Based on distance, calculate the speed the player is requesting.
        if (fMouseMagnitude > m_fMagnitudeFast)
        {
            m_fTargetSpeed = m_fMaxSpeed;
        }
        else if (fMouseMagnitude > m_fMagnitudeSlow)
        {
            m_fTargetSpeed = m_fSlowSpeed;
        }
        else
        {
            m_fTargetSpeed = 0.0f;
        }
    }

    void FixedUpdate()
    {
        GetComponent<Renderer>().material.color = stateColors[(int)m_nState];
    }

    void Update()
    {
        // sees if player can dive
        CheckForDive();

        // updates direction and speed
        UpdateDirectionAndSpeed();

        // follows mouse
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; 

        Vector3 directionToMouse = (mousePosition - transform.position).normalized;

        // mouse rotation
        float targetAngle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg + 180;

        //faces mouse
        m_fAngle = Mathf.LerpAngle(m_fAngle, targetAngle, m_fFastRotateSpeed * 10f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, m_fAngle));

        
        if (m_nState == eState.kMoveSlow)
        {
            transform.position = Vector3.MoveTowards(transform.position, mousePosition, m_fSlowSpeed * 60.0f * Time.deltaTime);
        }
        else if (m_nState == eState.kMoveFast)
        {
            transform.position = Vector3.MoveTowards(transform.position, mousePosition, m_fMaxSpeed * 60.0f * Time.deltaTime);
        }
        else if (m_nState == eState.kDiving)
        {
            // dives
            float diveProgress = (Time.time - m_fDiveStartTime) / m_fDiveTime;
            transform.position = Vector3.Lerp(m_vDiveStartPos, m_vDiveEndPos, diveProgress);

            // Check if the dive is finished
            if (diveProgress >= 1.0f)
            {
                // if dive finished, recovers
                m_nState = eState.kRecovering;
                m_fSpeed = 0.0f;
                m_fDiveStartTime = Time.time;
            }
        }
        else if (m_nState == eState.kRecovering)
        {
            // recovery time for dive
            if (Time.time - m_fDiveStartTime >= m_fDiveRecoveryTime)
            {
                //slower movement
                m_nState = eState.kMoveSlow;
            }
        }

        // keeps player in bounds
        Vector3 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, screenMin.x, screenMax.x),
            Mathf.Clamp(transform.position.y, screenMin.y, screenMax.y),
            transform.position.z
        );
    }








}
