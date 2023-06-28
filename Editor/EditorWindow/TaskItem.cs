using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TaskList
{
    public class TaskItem : VisualElement
    {
        private const string _taskItemAssetName = "TaskItem.uxml";
        private const string _taskStatusToggleName = "taskStatusToggle";
        private const string _taskNameLabelName = "taskNameLabel";
        private Toggle _taskToggle;
        private Label _taskNameLabel;

        public TaskItem(string taskName)
        {
            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TaskListEditor.AssetPath + _taskItemAssetName);
            this.Add(original.Instantiate());

            _taskToggle = this.Query<Toggle>(_taskStatusToggleName);
            if (_taskToggle == null)
            {
                throw new System.Exception($"Unable to locate toggle with name {_taskStatusToggleName}");
            }

            _taskNameLabel = this.Query<Label>(_taskNameLabelName);
            if (_taskNameLabel == null)
            {
                throw new System.Exception($"Unable to locate toggle with name {_taskNameLabelName}");
            }

            _taskNameLabel.text = taskName;
        }

        public Toggle GetTaskToggle() => _taskToggle;
        public Label GetTaskLabel() => _taskNameLabel;
    }
}

