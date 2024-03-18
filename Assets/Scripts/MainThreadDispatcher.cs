using System.Collections.Generic;
using UnityEngine;
using System;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();
    private static MainThreadDispatcher instance = null;

    public static MainThreadDispatcher Instance
{
    get
    {
        if (instance == null)
        {
            // 查找场景中是否已存在MainThreadDispatcher实例
            instance = FindObjectOfType<MainThreadDispatcher>();
            if (instance == null)
            {
                // 如果不存在，则创建一个新的GameObject
                GameObject dispatcherObject = new GameObject("MainThreadDispatcher");
                // 并将MainThreadDispatcher脚本添加为组件
                instance = dispatcherObject.AddComponent<MainThreadDispatcher>();
                // 确保它在场景加载时不被销毁
                DontDestroyOnLoad(dispatcherObject);
            }
        }
        return instance;
    }
}

    private void Update()
    {
        while (actions.Count > 0)
        {
            Action action = null;

            lock (actions)
            {
                if (actions.Count > 0)
                {
                    action = actions.Dequeue();
                }
            }
            action?.Invoke();
        }
    }

    public static void Enqueue(Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }
}
