using Mirko.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Mirko.Controls
{
    /// <summary>
    /// Represents the method that will handle a LayoutChangeStarted event.
    /// </summary>
    /// <param name="sender">The object where the handler is attached.</param>
    /// <param name="e">Event data for the event.</param>
    public delegate void LayoutChangeStartedEventHandler(object sender, LayoutChangeEventArgs e);

    /// <summary>
    /// Represents the method that will handle a LayoutChangeCompletedEvent event.
    /// </summary>
    /// <param name="sender">The object where the handler is attached.</param>
    /// <param name="e">Event data for the event.</param>
    public delegate void LayoutChangeCompletedEventHandler(object sender, LayoutChangeEventArgs e);

    /// <summary>
    /// Defines constants that specify how InputAwarePanel will animate when adjusting for InputPane.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum InputAwarePanelAnimationMode
    {
        /// <summary>
        /// No animation will occur and panel will snap into position immediately.
        /// </summary>
        None,
        /// <summary>
        /// Animation will occur on compositor thread and may be smoother than other animation modes.
        /// </summary>
        Independent,
        /// <summary>
        /// Animation will occur on UI thread and behave more like native OS but may not be as smooth.
        /// </summary>
        Dependent
    }

    /// <summary>
    /// Represents a panel that will resize to available visible area when input panel is deployed.
    /// </summary>
    [ContentProperty(Name = "Child")]
    [TemplatePart(Name = RootGridName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartShiftCompensatorName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartInputPaneSpacerName, Type = typeof(FrameworkElement))]
    public sealed class InputAwarePanel : Control
    {
        #region DependencyProperties
        /// <summary>
        /// Identifies the ChildProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child", typeof(UIElement),
            typeof(InputAwarePanel), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the AnimationMode dependency property.
        /// </summary>
        public static readonly DependencyProperty AnimationModeProperty =
            DependencyProperty.Register("AnimationMode", typeof(InputAwarePanelAnimationMode),
            typeof(InputAwarePanel), new PropertyMetadata(InputAwarePanelAnimationMode.None, OnAnimationModeChanged));
        #endregion

        #region Fields
        private const double AnimationStartOffset = 100;
        private const string RootGridName = "RootGrid";
        private const string PartShiftCompensatorName = "part_ShiftCompensator";
        private const string PartInputPaneSpacerName = "part_InputPaneSpacer";

        private Dictionary<Type, Action<FrameworkElement>> allowedTextControls;
        private FrameworkElement rootGrid;
        private FrameworkElement partShiftCompensator;
        private FrameworkElement partInputPaneSpacer;
        private bool isLayoutChangingDeferred;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a InputAwarePanel begins to adjust its layout for the InputPane.
        /// </summary>
        public event LayoutChangeStartedEventHandler LayoutChangeStarted;

        /// <summary>
        /// Occurs when a InputAwarePanel completes its adjustments for the InputPane.
        /// </summary>
        public event LayoutChangeCompletedEventHandler LayoutChangeCompleted;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the InputAwarePanel class.
        /// </summary>
        public InputAwarePanel()
        {
            this.DefaultStyleKey = typeof(InputAwarePanel);
            this.Loaded += InputAwarePanel_Loaded;
            this.Unloaded += InputAwarePanel_Unloaded;
            allowedTextControls = new Dictionary<Type, Action<FrameworkElement>>
            {
                {typeof(TextBox), RegisterTextBoxEvents},
                {typeof(RichEditBox), RegisterNothing},
                {typeof(PasswordBox), RegisterPasswordBoxEvents}
            };
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the child of the InputAwarePanel.
        /// </summary>
        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        /// <summary>
        /// Gets or sets the animation mode of the InputAwarePanel.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always)]
        public InputAwarePanelAnimationMode AnimationMode
        {
            get { return (InputAwarePanelAnimationMode)GetValue(AnimationModeProperty); }
            set { SetValue(AnimationModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the InputAwarePanel should update its layout when the InputPane is shown or hidden.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsLayoutChangingDeferred
        {
            get { return isLayoutChangingDeferred; }
            set
            {
                isLayoutChangingDeferred = value;
                if (!isLayoutChangingDeferred)
                {
                    var inputPane = InputPane.GetForCurrentView();
                    UpdatePanelLayout(inputPane.OccludedRect.Height);
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Finds the part_InputPaneSpacer FrameworkElement in the control template and adjusts its size to make room for the input pane.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            rootGrid = GetTemplateChild(RootGridName) as FrameworkElement;
            partShiftCompensator = GetTemplateChild(PartShiftCompensatorName) as FrameworkElement;
            partInputPaneSpacer = GetTemplateChild(PartInputPaneSpacerName) as FrameworkElement;
            var inputPane = InputPane.GetForCurrentView();
            UpdatePanelLayout(inputPane.OccludedRect.Height);
        }

        private void OnLayoutChangeStarted(LayoutChangeEventArgs e)
        {
            LayoutChangeStartedEventHandler handler = LayoutChangeStarted;

            if (handler != null)
                handler(this, e);
        }

        private void OnLayoutChangeCompleted(LayoutChangeEventArgs e)
        {
            LayoutChangeCompletedEventHandler handler = LayoutChangeCompleted;

            if (handler != null)
                handler(this, e);
        }

        private void Reset()
        {
            if (partInputPaneSpacer != null)
                partInputPaneSpacer.Height = 0;
            if (partShiftCompensator != null)
                partShiftCompensator.Height = 0;

            if (rootGrid != null)
            {
                var compositeTransform = rootGrid.RenderTransform as CompositeTransform;
                if (compositeTransform != null)
                    compositeTransform.TranslateY = 0;
            }
        }

        private void UpdatePanelLayout(double occludedHeight)
        {
            if (!IsLayoutChangingDeferred)
            {
                CreateResizeAnimation(occludedHeight, AnimationMode).Begin();
                OnLayoutChangeStarted(new LayoutChangeEventArgs(occludedHeight == 0));
            }
        }

        private void ScrollTextBoxIntoView()
        {
            var focusedElement = FocusManager.GetFocusedElement() as FrameworkElement;

            // Check if focusedElement is a type of control we need to scroll into view
            if (focusedElement != null && allowedTextControls.ContainsKey(focusedElement.GetType()))
            {
                // Hook up lost focus event so we can scroll new in-focus control
                focusedElement.LostFocus += FocusedElement_LostFocus;

                // Find a parent ScrollViewer to scroll our focusedElement into view
                var containerSv = focusedElement.GetAntecedent<ScrollViewer>();
                if (containerSv != null)
                {
                    double? verticalScrollOffset = null;
                    double? horizontalScrollOffset = null;
                    double focusedElementHeight = focusedElement.ActualHeight + 9.5;
                    double containerSvHeight = containerSv.ActualHeight;
                    
                    // Get the ScrollViewer's content so we can calculate distance of 
                    // focusedElement from top so we can scroll that much.
                    var containerPanel = containerSv.Content as FrameworkElement;
                    GeneralTransform transform = focusedElement.TransformToVisual(containerPanel);
                    Point offset = transform.TransformPoint(new Point(0, 0));

                    // Register events for when we need to scroll into view again.
                    // For example: When user types.
                    if (allowedTextControls.ContainsKey(focusedElement.GetType()))
                        allowedTextControls[focusedElement.GetType()](focusedElement);

                    // Special treatment for TextBox since we can actually calculate the
                    // position of the caret. If not a TextBox or focusedElement is smaller
                    // than visible area, then we just want to make sure it's in view.
                    var textBox = focusedElement as TextBox;
                    if (textBox == null || focusedElementHeight < containerSvHeight)
                    {
                        if (offset.Y + focusedElementHeight > containerSv.VerticalOffset + containerSvHeight)
                            verticalScrollOffset = offset.Y + Math.Min(focusedElementHeight, containerSvHeight)
                                - containerSvHeight;
                        else if (offset.Y < containerSv.VerticalOffset)
                            verticalScrollOffset = offset.Y - 9.5;
                    }
                    else
                    {
                        var partScrollViewer = textBox.GetDescendant<ScrollViewer>();
                        Point innerOffset = partScrollViewer.TransformToVisual(textBox).TransformPoint(new Point(0, 0));
                        Rect caretRect = new Rect(0, 0, 0, textBox.MinHeight);

                        // Using try catch since this will throw exception when there is no text in the
                        // TextBox or if the caret is at the end of all text and there's only one line of
                        // text.
                        try { caretRect = textBox.GetRectFromCharacterIndex(textBox.SelectionStart, false); }
                        catch { }

                        double top = caretRect.Y + innerOffset.Y;
                        double bottom = caretRect.Y + caretRect.Height + 9.5 + innerOffset.Y;

                        // Ugly code but checks if bottom of caret is below or if top of caret is above
                        // visible area of the parent ScrollViewer then scroll it into view.
                        if (offset.Y + bottom > containerSv.VerticalOffset + containerSvHeight + partScrollViewer.VerticalOffset)
                            verticalScrollOffset = offset.Y + bottom - containerSvHeight - partScrollViewer.VerticalOffset;
                        else if (offset.Y + top < containerSv.VerticalOffset + partScrollViewer.VerticalOffset)
                            verticalScrollOffset = offset.Y + top - partScrollViewer.VerticalOffset;
                    }

                    containerSv.ChangeView(horizontalScrollOffset, verticalScrollOffset, null);
                }
            }
        }

        private void RegisterNothing(FrameworkElement element)
        { }

        private void RegisterTextBoxEvents(FrameworkElement element)
        {
            var textBox = element as TextBox;
            if (textBox != null)
            {
                textBox.TextChanged -= TextBox_TextChanged;
                textBox.TextChanged += TextBox_TextChanged;
                textBox.SelectionChanged -= TextBox_SelectionChanged;
                textBox.SelectionChanged += TextBox_SelectionChanged;
            }
        }

        private void RegisterPasswordBoxEvents(FrameworkElement element)
        {
            var passwordBox = element as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private Storyboard CreateResizeAnimation(double occludedHeight, InputAwarePanelAnimationMode animationMode)
        {
            if (animationMode == InputAwarePanelAnimationMode.Independent)
                return CreateShiftAnimation(occludedHeight);
            else
                return CreateInPlaceAnimation(occludedHeight, animationMode);
        }

        private Storyboard CreateInPlaceAnimation(double occludedHeight, InputAwarePanelAnimationMode animationMode)
        {
            var sb = new Storyboard();
            if (partInputPaneSpacer != null)
            {
                var easing = new CubicEase() { EasingMode = occludedHeight == 0 ? EasingMode.EaseIn : EasingMode.EaseOut };
                var duration = new Duration(TimeSpan.FromSeconds(animationMode == InputAwarePanelAnimationMode.None ? 0 : 0.2));

                var heightAnimation = new DoubleAnimation();
                heightAnimation.EnableDependentAnimation = true;
                heightAnimation.From = partInputPaneSpacer.ActualHeight +
                    (occludedHeight <= partInputPaneSpacer.ActualHeight ? 0 : AnimationStartOffset);
                heightAnimation.To = Math.Max(occludedHeight, 0);
                heightAnimation.EasingFunction = easing;
                heightAnimation.Duration = duration;
                Storyboard.SetTarget(heightAnimation, partInputPaneSpacer);
                Storyboard.SetTargetProperty(heightAnimation, "Height");
                sb.Children.Add(heightAnimation);
                sb.Completed += Storyboard_Completed;
            }
            return sb;
        }

        private Storyboard CreateShiftAnimation(double occludedHeight, bool isAnimated = true)
        {
            var sb = new Storyboard();
            if (rootGrid != null)
            {
                var easing = new CubicEase() { EasingMode = occludedHeight == 0 ? EasingMode.EaseIn : EasingMode.EaseOut };
                var duration = new Duration(TimeSpan.FromSeconds(0.2));

                var translateAnimation = new DoubleAnimation();
                translateAnimation.From = (rootGrid.RenderTransform as CompositeTransform).TranslateY;
                translateAnimation.To = -(Math.Max(occludedHeight, 0));
                translateAnimation.EasingFunction = easing;
                translateAnimation.Duration = duration;
                Storyboard.SetTarget(translateAnimation, rootGrid);
                Storyboard.SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
                sb.Children.Add(translateAnimation);
                sb.Completed += ShiftAnimation_Completed;

                partShiftCompensator.Height = Math.Max(occludedHeight, 0);

                sb.Completed += Storyboard_Completed;
            }
            return sb;
        }
        #endregion

        #region EventHandlers
        private void InputAwarePanel_Loaded(object sender, RoutedEventArgs e)
        {
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;
        }

        private void InputAwarePanel_Unloaded(object sender, RoutedEventArgs e)
        {
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing -= InputPane_Showing;
            inputPane.Hiding -= InputPane_Hiding;
        }

        private static void OnAnimationModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var inputAwarePanel = dependencyObject as InputAwarePanel;
            inputAwarePanel.Reset();
            inputAwarePanel.CreateInPlaceAnimation(0, InputAwarePanelAnimationMode.None).Begin();
            var inputPane = InputPane.GetForCurrentView();
            inputAwarePanel.UpdatePanelLayout(inputPane.OccludedRect.Height);
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            UpdatePanelLayout(args.OccludedRect.Height);
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = true;
            UpdatePanelLayout(args.OccludedRect.Height);
        }

        private void Storyboard_Completed(object sender, object e)
        {
            var sb = sender as Storyboard;
            if (sb != null)
                sb.Completed -= Storyboard_Completed;

            ScrollTextBoxIntoView();
            OnLayoutChangeCompleted(new LayoutChangeEventArgs(
                partShiftCompensator.Height == 0 && partInputPaneSpacer.Height == 0));
        }

        private void ShiftAnimation_Completed(object sender, object e)
        {
            var sb = sender as Storyboard;

            var occludedHeight = InputPane.GetForCurrentView().OccludedRect.Height;
            sb.Completed -= ShiftAnimation_Completed;
        }

        private void FocusedElement_LostFocus(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.LostFocus -= FocusedElement_LostFocus;
                ScrollTextBoxIntoView();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ScrollTextBoxIntoView();
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ScrollTextBoxIntoView();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ScrollTextBoxIntoView();
        }
        #endregion
    }

    /// <summary>
    /// Provides data for the InpuAwarePanel LayoutChangeStarted and LayoutChangeCompletd events.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LayoutChangeEventArgs : EventArgs
    {
        private bool isDefaultLayout = true;

        /// <summary>
        /// Initializes a new instance of the LayoutChangeEventArgs class.
        /// </summary>
        /// <param name="isDefaultLayout"></param>
        public LayoutChangeEventArgs(bool isDefaultLayout)
        {
            this.isDefaultLayout = isDefaultLayout;
        }

        /// <summary>
        /// Identifies whether the layout is changing to the default state or to a state that's compensated for the InputPane.
        /// Value is true if the end result of the layout change results in no compensation for the InputPane.
        /// </summary>
        public bool IsDefaultLayout { get { return isDefaultLayout; } }
    }
}
