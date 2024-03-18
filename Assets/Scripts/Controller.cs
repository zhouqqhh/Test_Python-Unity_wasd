using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public Animator animator;
    public GameObject character; // 正确地声明character作为类的字段
    public GameObject loadedCharacter;
    public float speedForward = 2.0f;
    public float speedBackward = 1.0f;
    public float speedLeft = 1.0f;
    public float speedRight = 1.0f;

    private Vector3 deltaNewPos = Vector3.zero;
    private int newAnim = 0;
    private string currentMoveDirection = "";
    private Queue<System.Action> actionsToExecuteOnMainThread = new Queue<System.Action>();

    void Update()
    {
        while (actionsToExecuteOnMainThread.Count > 0)
        {
            System.Action action = actionsToExecuteOnMainThread.Dequeue();
            action?.Invoke(); // 安全地调用操作，如果action不为null
        }

        if (character != null && animator != null)
        {
        if(character != null)
        {
            character.transform.Translate(Time.deltaTime * deltaNewPos);
        }
        animator.SetInteger("WASD", newAnim);
    }
    }

    public void MoveForward()
    {
        deltaNewPos = speedForward * Vector3.forward;
        newAnim = 1;
        currentMoveDirection = "forward";
    }

    public void MoveBackward()
    {
        deltaNewPos = speedBackward * Vector3.back;
        newAnim = 3;
        currentMoveDirection = "backward";
    }

    public void MoveLeft()
    {
        deltaNewPos = speedLeft * Vector3.left;
        newAnim = 2;
        currentMoveDirection = "left";
    }

    public void MoveRight()
    {
        deltaNewPos = speedRight * Vector3.right;
        newAnim = 4;
        currentMoveDirection = "right";
    }

    public void Idle()
    {
        deltaNewPos = Vector3.zero;
        newAnim = 0;
        currentMoveDirection = ""; // 清空当前的移动方向
    }

    public void LoadAgent(string[] parts) {
    if(parts.Length >= 2)
    {
        lock (actionsToExecuteOnMainThread)
        {
            actionsToExecuteOnMainThread.Enqueue(() =>
            {
                loadedCharacter = Resources.Load<GameObject>(parts[0]);
            });
        }
    }
}

    public void PlaceAgent(string[] parts)
    {
        if (parts.Length == 4 && parts[3].ToLower() == "placeagent")
        {
            if (float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
            {
                lock (actionsToExecuteOnMainThread)
                {
                    actionsToExecuteOnMainThread.Enqueue(() =>
                    {
                        Vector3 position = new Vector3(x, y, z);
                        character = Instantiate(loadedCharacter, position, Quaternion.identity);
                        
                    Animator charAnimator = character.GetComponent<Animator>();
                    if (charAnimator == null)
                    {
                        charAnimator = character.AddComponent<Animator>(); // 如果没有找到 Animator 组件，则添加一个
                    }

                    RuntimeAnimatorController animatorController = Resources.Load<RuntimeAnimatorController>("Characters/WASD_AnimatorController");
                    if (animatorController != null)
                    {
                        charAnimator.runtimeAnimatorController = animatorController;
                        animator = charAnimator; // 确保 Controller 脚本的 animator 变量指向这个 Animator 组件
                    }
                    else
                    {
                        Debug.LogError("Failed to load Animator Controller from Resources.");
                    }

                    });
                }
            }
            else
            {
                Debug.LogError("Can not resolve position.");
            }
        }
    }
    public void Command(string command)
    {
        string[] parts = command.Split(' ');

        // 检查命令是否为放置agent的命令
        if (parts.Length == 4 && parts[3].ToLower() == "placeagent")
        {
            MainThreadDispatcher.Enqueue(() => PlaceAgent(parts));
        }
        // 检查命令是否为放置agent的命令
        if (parts.Length == 2 && parts[1].ToLower() == "loadagent")
        {
            MainThreadDispatcher.Enqueue(() => LoadAgent(parts));
        }
        else if (parts.Length == 2)
        {
            string key = parts[0].ToLower();
            string action = parts[1].ToLower();

            if (action == "down")
            {
                switch (key)
                {
                    case "w":
                    case "up":
                        MoveForward();
                        break;
                    case "a":
                    case "left":
                        MoveLeft();
                        break;
                    case "s":
                    case "down":
                        MoveBackward();
                        break;
                    case "d":
                    case "right":
                        MoveRight();
                        break;
                    default:
                        Idle();
                        break;
                }
            }
            else if (action == "up")
            {
                // 如果是释放按键的命令
                switch (key)
                {
                    case "w":
                    case "up":
                        if (currentMoveDirection == "forward") Idle();
                        break;
                    case "a":
                    case "left":
                        if (currentMoveDirection == "left") Idle();
                        break;
                    case "s":
                    case "down":
                        if (currentMoveDirection == "backward") Idle();
                        break;
                    case "d":
                    case "right":
                        if (currentMoveDirection == "right") Idle();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
