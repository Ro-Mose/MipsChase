using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Player m_player;
    public enum eState : int
    {
        kIdle,
        kHopStart,
        kHop,
        kCaught,
        kNumStates
    }

    private Color[] stateColors = new Color[(int)eState.kNumStates]
   {
        new Color(255, 0,   0),
        new Color(0,   255, 0),
        new Color(0,   0,   255),
        new Color(255, 255, 255)
   };

    // External tunables.
    public float m_fHopTime = 0.2f;
    public float m_fHopSpeed = 6.5f;
    public float m_fScaredDistance = 3.0f;
    public int m_nMaxMoveAttempts = 50;

    // Internal variables.
    public eState m_nState;
    public float m_fHopStart;
    public Vector3 m_vHopStartPos;
    public Vector3 m_vHopEndPos;

    void Start()
    {
        // Setup the initial state and get the player GO.
        m_nState = eState.kIdle;
        m_player = GameObject.FindObjectOfType(typeof(Player)) as Player;
    }

    void FixedUpdate()
    {
        GetComponent<Renderer>().material.color = stateColors[(int)m_nState];
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Check if this is the player (in this situation it should be!)
        if (collision.gameObject == GameObject.Find("Player"))
        {
            // If the player is diving, it's a catch!
            if (m_player.IsDiving())
            {
                m_nState = eState.kCaught;
                transform.parent = m_player.transform;
                transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);
            }
        }
    }

    void Update()
    {
        // find distance from player
        float distanceToPlayer = Vector3.Distance(m_player.transform.position, transform.position);

        // if too close to player hops away
        if (m_nState == eState.kIdle && distanceToPlayer <= m_fScaredDistance)
        {
            m_nState = eState.kHopStart;  
            m_fHopStart = Time.time; 
            m_vHopStartPos = transform.position;  
        }
        else if (m_nState == eState.kHopStart)
        {
            
            if (Time.time - m_fHopStart >= 0.1f)
            {
                m_nState = eState.kHop;  
            }
        }
        else if (m_nState == eState.kHop)
        {
            //directions to move away from player
            Vector3 directionAwayFromPlayer = (transform.position - m_player.transform.position).normalized;
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            Vector3 finalDirection = directionAwayFromPlayer + randomDirection;

            finalDirection = finalDirection.normalized;

            transform.position += finalDirection * m_fHopSpeed * Time.deltaTime;

            // stands idle if player is far
            if (Vector3.Distance(m_vHopStartPos, transform.position) >= 1.5f)
            {
                m_nState = eState.kIdle;
            }
        }
        else if (m_nState == eState.kCaught)
        {
            // attaches to player after being caught
            transform.parent = m_player.transform;
            transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);  
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