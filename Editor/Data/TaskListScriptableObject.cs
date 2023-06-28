using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaskList
{
    [CreateAssetMenu(fileName = "TaskListData", menuName = "TaskList/TaskListScriptableObject", order = 1)]
    public class TaskListScriptableObject : ScriptableObject
    {
        [SerializeField] private List<string> _tasks;

        public List<string> GetTasks() => _tasks;
        public void AddTasks(IEnumerable<string> tasks)
        {
            _tasks = new List<string>(tasks);
            SaveAsset();
        }

        public void AddTask(string task)
        {
            _tasks.Add(task);
            SaveAsset();
        }

        private void SaveAsset()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();
        }
    }
}

