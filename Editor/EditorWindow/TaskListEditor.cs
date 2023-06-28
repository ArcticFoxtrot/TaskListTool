using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.IO;
using System.Collections.Generic;

namespace TaskList
{
    public class TaskListEditor : EditorWindow
    {
        public const string AssetPath = "Assets/TaskList/Editor/EditorWindow/";
        public const string DataPath = "Assets/TaskList/Editor/Data/";
        private const string _addTaskButtonName = "addTaskButton";
        private const string _scrollViewName = "taskListScrollView";
        private const string _taskNameInputName = "taskNameInput";
        private const string _savedTasksObjectFieldName = "savedTasksObjectField";
        private const string _loadTasksButtonName = "loadTasksButton";
        private const string _saveProgressButtonName = "saveProgressButton";
        private const string _taskProgressBarName = "taskProgressBar";
        private const string _searchBarName = "searchBar";
        private const string _notificationTextName = "notificationText";
        private VisualElement _container;
        private TextField _taskNameInput;
        private Button _addTaskButton;
        private ObjectField _savedTasksObjectField;
        private Button _loadTasksButton;
        private Button _saveProgressButton;
        private ProgressBar _taskProgressBar;
        private ToolbarSearchField _searchBar;
        private Label _notificationLabel;

        TaskListScriptableObject _taskListObject;


        private ScrollView _taskListScrollView;
        [MenuItem("Tools/Task List Editor")]
        public static void OpenTaskListEditor()
        {
            var window = GetWindow<TaskListEditor>();
        }


        public void CreateGUI()
        {
            _container = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{AssetPath}TaskListEditor.uxml");
            _container.Add(visualTree.Instantiate());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{AssetPath}TaskListEditor.uss");
            _container.styleSheets.Add(styleSheet);

            SetupSavedTasksObject();
            SetupLoadTasksButton();
            SetupTaskNameInput();
            SetupSubmitButton();
            SetupScrollView();
            SetupSaveProgressButton();
            SetupTaskProgressBar();
            SetupSearchBar();
            SetupNotificationLabel();
        }

        private void SetupSavedTasksObject()
        {
            _savedTasksObjectField = _container.Q<ObjectField>(_savedTasksObjectFieldName);

            if (_savedTasksObjectField == null)
            {
                throw new System.Exception($"No saved tasks object field found with name {_savedTasksObjectFieldName}");
            }

            _savedTasksObjectField.objectType = typeof(TaskListScriptableObject);
        }

        private void SetupScrollView()
        {
            _taskListScrollView = _container.Q<ScrollView>(_scrollViewName);

            if (_taskListScrollView == null)
            {
                throw new System.Exception($"No button found with name {_scrollViewName}");
            }
        }

        private void SetupSubmitButton()
        {
            _addTaskButton = _container.Q<Button>(_addTaskButtonName);

            if (_addTaskButton == null)
            {
                throw new System.Exception($"No button found with name {_addTaskButtonName}");
            }

            _addTaskButton.clicked += AddTask;
        }

        private void SetupLoadTasksButton()
        {
            _addTaskButton = _container.Q<Button>(_loadTasksButtonName);

            if (_addTaskButton == null)
            {
                throw new System.Exception($"No button found with name {_loadTasksButtonName}");
            }

            _addTaskButton.clicked += OnLoadTasksClicked;
        }

        private void SetupTaskNameInput()
        {
            _taskNameInput = _container.Q<TextField>(_taskNameInputName);
            if (_taskNameInput == null)
            {
                throw new System.Exception($"No task name input field with name {_taskNameInputName} found");
            }

            _taskNameInput.RegisterCallback<KeyDownEvent>(AddTask);
        }

        private void SetupSaveProgressButton()
        {
            _saveProgressButton = _container.Q<Button>(_saveProgressButtonName);
            if (_saveProgressButton == null)
            {
                throw new System.Exception($"No button found with name {_saveProgressButtonName}");
            }

            _saveProgressButton.clicked += SaveProgress;
        }

        private void SetupTaskProgressBar()
        {
            _taskProgressBar = _container.Q<ProgressBar>(_taskProgressBarName);
            if (_taskProgressBar == null)
            {
                throw new System.Exception($"No progressbar found with name {_taskProgressBarName}");
            }
        }

        private void SetupSearchBar()
        {
            _searchBar = _container.Q<ToolbarSearchField>(_searchBarName);
            if (_searchBar == null)
            {
                throw new System.Exception($"No search bar found with name {_searchBarName}");
            }

            _searchBar.RegisterValueChangedCallback(OnSearchBarValueChanged);
        }

        private void SetupNotificationLabel()
        {
            _notificationLabel = _container.Q<Label>(_notificationTextName);
            if (_notificationLabel == null)
            {
                throw new System.Exception($"No label found with name  {_notificationTextName}");
            }

            if (_taskListObject == null)
            {
                UpdateNotifications("Please load up a task list object");
                return;
            }

            _notificationLabel.text = string.Empty;
        }

        private void OnSearchBarValueChanged(ChangeEvent<string> evt)
        {
            var currentValue = _searchBar.value;

            foreach (TaskItem item in _taskListScrollView.Children())
            {
                if (!string.IsNullOrEmpty(currentValue) && item.GetTaskLabel().text.ToLowerInvariant().ContainsInvariantCultureIgnoreCase(currentValue))
                {
                    item.AddToClassList("highlight");
                }
                else
                {
                    item.RemoveFromClassList("highlight");
                }
            }
        }

        private void SaveProgress()
        {
            if (_taskListObject != null)
            {
                var incompleteTasks = new List<string>();

                foreach (TaskItem item in _taskListScrollView.Children())
                {
                    if (!item.GetTaskToggle().value)
                    {
                        incompleteTasks.Add(item.GetTaskLabel().text);
                    }
                }
                _taskListObject.AddTasks(incompleteTasks);
                LoadTasks();
            }

            UpdateNotifications("Progress saved!");
        }

        private void UpdateProgress()
        {
            var allTasksCount = 0;
            var completeTasksCount = 0;

            foreach (TaskItem item in _taskListScrollView.Children())
            {
                allTasksCount++;
                if (item.GetTaskToggle().value)
                {
                    completeTasksCount++;
                }
            }


            var completion = 0f;

            if (allTasksCount > 0)
            {
                completion = (float)completeTasksCount / allTasksCount;
            }
            else
            {
                completion = 1f;
            }

            _taskProgressBar.value = completion;
            _taskProgressBar.title = $"{(Mathf.Round(completion * 1000) * 0.1f).ToString()}%";
        }

        private void AddTask(KeyDownEvent evt)
        {
            if (Event.current.Equals(Event.KeyboardEvent("Return")))
            {
                AddTask();
                UpdateProgress();
                _taskNameInput.Focus();
            }
        }

        private void AddTask()
        {
            if (TryGetValidatedTextInput(out string input))
            {
                SaveTask(input);
                AddTaskToView(input);
            }

            UpdateNotifications("Task added!");
            _taskNameInput.value = string.Empty;
        }

        private void AddTaskToView(string input)
        {
            TaskItem taskItem = new TaskItem(input);
            taskItem.GetTaskToggle().RegisterValueChangedCallback(OnToggleValueChanged);
            _taskListScrollView.Add(taskItem);
        }

        private void OnToggleValueChanged(ChangeEvent<bool> evt)
        {
            UpdateProgress();
            UpdateNotifications("Progress updated!");
        }

        private bool TryGetValidatedTextInput(out string input)
        {
            input = string.Empty;
            if (_taskNameInput.value.Length <= 0)
            {
                return false;
            }

            input = _taskNameInput.value;
            return true;
        }

        private void OnLoadTasksClicked()
        {
            LoadTasks();
            UpdateNotifications("Tasks loaded successfully!");
        }

        private void LoadTasks()
        {
            if (_savedTasksObjectField.value is TaskListScriptableObject taskListScriptableObject)
            {
                _taskListObject = taskListScriptableObject;
                _taskListScrollView.Clear();
                List<string> tasks = _taskListObject.GetTasks();
                foreach (var task in tasks)
                {
                    AddTaskToView(task);
                }

                UpdateProgress();
            }
            else
            {
                UpdateNotifications("Invalid object in task list object field");
                throw new InvalidOperationException($"Invalid object in task list object field");
            }

        }

        private void SaveTask(string task)
        {
            _taskListObject.AddTask(task);
        }

        private void UpdateNotifications(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (_notificationLabel == null)
            {
                return;
            }

            _notificationLabel.text = text;
        }

    }


}
