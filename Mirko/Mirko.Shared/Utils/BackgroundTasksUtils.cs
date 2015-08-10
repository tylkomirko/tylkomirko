using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Mirko.Utils
{
    public static class BackgroundTasksUtils
    {
        public static async Task RegisterTask(string taskEntryPoint, string taskName,
                                              IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var statusOperation = await BackgroundExecutionManager.RequestAccessAsync();
            if (statusOperation == BackgroundAccessStatus.Denied || statusOperation == BackgroundAccessStatus.Unspecified)
                return;

            //
            // Check for existing registrations of this background task.
            //

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {

                if (cur.Value.Name == taskName)
                {
                    // 
                    // The task is already registered.
                    // 
                    return;
                }
            }

            //
            // Register the background task.
            //

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
                builder.AddCondition(condition);

            BackgroundTaskRegistration task = builder.Register();
        }

        public static void UnregisterTask(string taskName)
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                    cur.Value.Unregister(false);
            }

            BackgroundExecutionManager.RemoveAccess();
        }
    }
}
