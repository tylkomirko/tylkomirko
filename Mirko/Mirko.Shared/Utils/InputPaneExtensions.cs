using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace Mirko.Utils
{
    public static class InputPaneExtensions
    {
        public static Task HideAsync(this InputPane pane)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (pane == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                TypedEventHandler<InputPane, InputPaneVisibilityEventArgs> onComplete = null;
                onComplete = (s, e) =>
                {
                    pane.Hiding -= onComplete;
                    tcs.SetResult(true);
                };
                pane.Hiding += onComplete;
                pane.TryHide();
            }
            return tcs.Task;
        }
    }
}
