using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Mirko.Utils
{
    public static class VisualTreeExtensions
    {
        /// <summary>
        /// Gets children, children's children, etc. from 
        /// the visual tree that match the specified type
        /// </summary>
        public static List<T> GetDescendants<T>(this DependencyObject parent)
            where T : UIElement
        {
            List<T> children = new List<T>();

            int count = VisualTreeHelper.GetChildrenCount(parent);

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                    if (child is T)
                    {
                        children.Add((T)child);
                    }

                    children.AddRange(child.GetDescendants<T>());
                }
                return children;
            }
            else
            {
                return new List<T> { };
            }
        }

        /// <summary>
        /// Gets children, children's children, etc. from 
        /// the visual tree that match the specified type and elementName
        /// </summary>
        public static List<T> GetDescendants<T>(this DependencyObject parent, string elementName)
            where T : UIElement
        {
            List<T> children = new List<T>();

            int count = VisualTreeHelper.GetChildrenCount(parent);

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                    if (child is T && (child is FrameworkElement)
                        && (child as FrameworkElement).Name == elementName)
                    {
                        children.Add((T)child);
                    }

                    children.AddRange(child.GetDescendants<T>(elementName));
                }
                return children;
            }
            else
            {
                return new List<T> { };
            }
        }

        /// <summary>
        /// Gets the first child, child's child, etc. 
        /// from the visual tree that matches the specified type
        /// </summary>
        public static T GetDescendant<T>(this DependencyObject parent)
            where T : UIElement
        {
            List<T> descendants = parent.GetDescendants<T>();

            if (descendants.Count > 0)
            {
                return descendants[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the first child, child's child, etc. from 
        /// the visual tree that matches the specified type and elementName
        /// </summary>
        public static T GetDescendant<T>(this DependencyObject parent, string elementName)
            where T : UIElement
        {
            List<T> descendants = parent.GetDescendants<T>(elementName);

            if (descendants.Count > 0)
            {
                return descendants[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the first parent, parent's parent, etc. from the 
        /// visual tree that matches the specified type
        /// </summary>
        public static T GetAntecedent<T>(this DependencyObject root)
            where T : UIElement
        {
            if (root == null)
            {
                return null;
            }
            if (root is T)
            {
                return (T)root;
            }
            else
            {
                DependencyObject parent = VisualTreeHelper.GetParent(root);
                if (parent == null)
                {
                    return null;
                }
                else
                {
                    return parent.GetAntecedent<T>();
                }
            }
        }

        /// <summary>
        /// Gets the first parent, parent's parent, etc. from the 
        /// visual tree that matches the specified type and elementName
        /// </summary>
        public static T GetAntecedent<T>(this DependencyObject root, string elementName)
            where T : UIElement
        {
            if (root == null)
            {
                return null;
            }
            if (root is T && (root is FrameworkElement)
                && (root as FrameworkElement).Name == elementName)
            {
                return (T)root;
            }
            else
            {
                DependencyObject parent = VisualTreeHelper.GetParent(root);
                if (parent == null)
                {
                    return null;
                }
                else
                {
                    return parent.GetAntecedent<T>(elementName);
                }
            }
        }
    }
}