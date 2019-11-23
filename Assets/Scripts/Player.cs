﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RotateControl))]
public class Player : MonoBehaviour
{
    public List<TrailRenderer> TrailRenderers;

    public Material BoatMat;
    public Material MonsterMat;

    private RotateControl _control;
    public RotateControl Control
    {
        get
        {
            if(_control == null)
            {
                _control = GetComponent<RotateControl>();
            }

            return _control;
        }
    }

    public KeyCode KeyCode;

    private Point _point;

    public Color defaultColor;

    private void BecomeMonster()
    {
        SetMaterials(MonsterMat);
        if (_control != null) _control.Boost = 100;
    }

    private void BecomeBoat()
    {
        SetMaterials(BoatMat);
        if (_control != null) _control.Boost = 0;
    }

    private void SetMaterials(Material material)
    {
        //temp hack
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            if(!(renderer is TrailRenderer))
            {
                renderer.sharedMaterial = material;
            } 
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Point point = collision.gameObject.GetComponent<Point>();     
        if (point != null)
        {
            _point = point;
            _point.Hide();
            GameManager.Instance.PlayerGotPoint(this);
        }
        else
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                if (player._point != null)
                {
                    Respawn();
                }

                if (_point == null)
                {
                    Control.ChangeDirection();
                }
                else
                {
                    GameManager.Instance.PlayerKilledOther(this);
                }
            }
            else
            {
                Control.ChangeDirection();
            }    
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WhirlPool pool = other.GetComponent<WhirlPool>();
        if (pool != null)
        {       
           transform.position = pool.GetLinkPosition();
            ResetTrail();
        }
    }

    public void ClearPoint()
    {
        if(_point != null)
        {
            _point.Destroy();
            _point = null;
        }
    }

    private void OnDestroy()
    {
        if(_point != null)
        {
            _point.Destroy();
        }   
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode))
        {
            Control.Rotate = !Control.Rotate;
        }

        if(_point == null)
        {
            BecomeBoat();       
        }
        else
        {
            BecomeMonster();
        }


        //Try wrap
        Vector3 wrappedPos = transform.position;
        if (LevelBounds.Hit(ref wrappedPos))
        {
            transform.position = wrappedPos;
            ResetTrail();
        }
    }

    public void Respawn()
    {
        transform.position = GameManager.Instance.GetRespawnPoint();
        ResetTrail();
        GameManager.Instance.IncreaseSpeed();
    }

    private void ResetTrail()
    {
        foreach (TrailRenderer renderer in TrailRenderers)
        {
            renderer.Clear();
        }
    }
}
