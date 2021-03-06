﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    float moveSpeedTemp;
    Vector2 velocity, knockbackVelocity, dodgeVelocity;
    public Rigidbody2D rigidbody2d;
    private float animTimer = 0f;
    public float animSpeed = 0.3f;
    public float animBobHeight = 0.2f;
    bool moving = false;
    bool isHitByEnemy = false;
    bool isDodging = false;
    private float hitByEnemyAnimTimer = 0f;
    public int health = 3;
    Vector3 knockbackDir;
    private float dodgeTimer = 0f;
    public float dodgeDuration = 0.2f;
    public float dodgeSpeed = 30.0f;
    Vector2 direction;
    Vector2 dodgeDirection;
    bool standingOnExit = false;
    bool standingOnTreasure = false;
    TreasureController treasureReference;

    [SerializeField]
    List<HelmetController> helmets;
    [SerializeField]
    List<TorsoController> torsos;

    private void Awake()
    {
        //GetComponentsInChildren<TorsoController>(true)
        helmets = GetComponentsInChildren<HelmetController>(true).ToList();
        torsos = GetComponentsInChildren<TorsoController>(true).ToList();

        if (GameManager.Instance == null)
        {
            return;
        }
        var weapons = GetComponentsInChildren<AbsWeaponController>();

        if (weapons.Length > 0)
        {
            foreach (var weapon in weapons)
            {
                if (GameManager.Instance.equippedWeapon == weapon.GetItemId())
                {
                    weapon.gameObject.SetActive(true);
                    GetComponent<MainWeaponController>().equippedWeapon = weapon;
                    //break;
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = gameObject.GetComponent<Rigidbody2D>();
        health = GameManager.Instance.maxHealth;
        if (GameManager.Instance.inDungeon) transform.position = GameManager.Instance.dungeonController.GetFloorStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (isHitByEnemy) rigidbody2d.velocity = knockbackVelocity;
        else if (isDodging) rigidbody2d.velocity = dodgeVelocity;
        else rigidbody2d.velocity = velocity;
        MoveAnim();
        if (isHitByEnemy) HitByEnemyAnim();
        if (isDodging) DodgeAnim();
        // GameManager.Instance.dungeonController.checkRoom(transform.position);
    }

    public void SetMovement(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
        velocity = direction * moveSpeed;
    }

    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && !isDodging && direction != Vector2.zero)
        {
            dodgeDirection = direction;
            moveSpeedTemp = moveSpeed;
            isDodging = true;
            dodgeVelocity = dodgeDirection * moveSpeed;
        };
    }

    public void DodgeAnim()
    {
        if (dodgeTimer < dodgeDuration)
        {
            dodgeVelocity = dodgeDirection * Mathf.Lerp(moveSpeedTemp * 4, dodgeSpeed, Mathf.Sin(Mathf.PI * dodgeTimer / dodgeDuration));
            {
                transform.localRotation = Quaternion.Slerp(
                    Quaternion.Euler(0, 0, 350),
                    Quaternion.Euler(0, 0, 10),
                    Mathf.Sin(Mathf.PI * dodgeTimer / dodgeDuration));
            }
            dodgeTimer += Time.deltaTime;
        }
        else
        {
            transform.localRotation = Quaternion.identity;
            moveSpeed = moveSpeedTemp;
            dodgeTimer = 0f;
            isDodging = false;
        }
    }

    public void DungeonButton()
    {
        Debug.Log("test!");
    }

    private bool isMoving()
    {
        return (velocity.x != 0 || velocity.y != 0);
    }

    private void MoveAnim()
    {
        if (isMoving())
        {
            animTimer += animSpeed;
            transform.localScale = new Vector3(0.75F, (float)(0.75 - animBobHeight + Math.Abs(animBobHeight * Math.Sin(animTimer / 10))), 0.0F);
        }
    }

    public void HitByEnemy(Vector3 knockbackDir, int damage)
    {
        if (!isHitByEnemy)
        {
            isHitByEnemy = true;
            health -= damage;
            if (health <= 0)
            {
                GameManager.Instance.value = 0;
                GameManager.Instance.transitionToCampScene();
            }
            this.knockbackDir = knockbackDir;
            GameManager.Instance.hearts.UpdateHearts();
        }
    }
    public void HitByEnemyAnim()
    {
        knockbackVelocity = Vector2.Lerp(knockbackDir, Vector2.zero, (float)Math.Sin(hitByEnemyAnimTimer * Math.PI));
        hitByEnemyAnimTimer += Time.deltaTime;
        if (hitByEnemyAnimTimer > 0.2) { isHitByEnemy = false; hitByEnemyAnimTimer = 0; };
    }

    public void UpdateArmor()
    {
        foreach(var helmet in helmets)
        {
            helmet.UpdateLook();
        }
        foreach (var torso in torsos)
        {
            torso.UpdateLook();
        }
    }
    public void MoveToNextFloor(InputAction.CallbackContext context)
    {
        if (context.performed && standingOnExit)
        {
            Debug.Log("Moving on");
            bool isDone = GameManager.Instance.dungeonController.MoveToNextFloor();
            if (!isDone) transform.position = GameManager.Instance.dungeonController.GetFloorStart();
        }
        else if (context.performed && standingOnTreasure)
        {
            treasureReference.OpenTreasure();

        }
    }

    // OnTriggerEnter2D is inconsistent, so we use ontriggerstay
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Exit")
        {
            standingOnExit = true;
        }
        else if (other.gameObject.tag == "Treasure")
        {
            standingOnTreasure = true;
            treasureReference = other.gameObject.GetComponent<TreasureController>();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Exit")
        {
            standingOnExit = false;
        }
        else if (other.gameObject.tag == "Treasure")
        {
            standingOnTreasure = false;
        }
    }
}
