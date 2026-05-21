// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.Vault.Explorer
{
    /// <summary>
    /// Provides theme-aware colors following Microsoft Fluent design guidelines.
    /// Uses Windows dark mode APIs to fix ListView group headers and other OS-rendered elements.
    /// </summary>
    internal static class ThemeHelper
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string? pszSubIdList);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        /// <summary>
        /// Determines if the current system theme is dark based on the window background luminance.
        /// </summary>
        public static bool IsDarkMode
        {
            get
            {
                var bg = SystemColors.Window;
                // Luminance formula: if background is dark, we're in dark mode
                double luminance = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0;
                return luminance < 0.5;
            }
        }

        /// <summary>
        /// Microsoft Fluent link color — readable in both light and dark modes.
        /// Light mode: #005FB8 (Fluent blue), Dark mode: #60CDFF (Fluent accent light)
        /// </summary>
        public static Color LinkColor => IsDarkMode
            ? Color.FromArgb(96, 205, 255)   // Fluent accent light
            : Color.FromArgb(0, 95, 184);    // Fluent blue

        /// <summary>
        /// Active/hover link color.
        /// </summary>
        public static Color ActiveLinkColor => IsDarkMode
            ? Color.FromArgb(152, 224, 255)
            : Color.FromArgb(0, 62, 146);

        /// <summary>
        /// Visited link color.
        /// </summary>
        public static Color VisitedLinkColor => LinkColor;

        /// <summary>
        /// Error/expired color readable in both modes.
        /// </summary>
        public static Color ErrorColor => IsDarkMode
            ? Color.FromArgb(255, 153, 164)   // Soft red for dark
            : Color.FromArgb(196, 43, 28);    // Fluent red for light

        /// <summary>
        /// Warning/about-to-expire color readable in both modes.
        /// </summary>
        public static Color WarningColor => IsDarkMode
            ? Color.FromArgb(252, 225, 0)     // Bright yellow for dark
            : Color.FromArgb(157, 93, 0);     // Dark orange for light

        /// <summary>
        /// Applies theme-aware colors to all controls in a form's control tree.
        /// </summary>
        public static void ApplyTo(Control control)
        {
            ApplyToRecursive(control);
        }

        private static void ApplyToRecursive(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is LinkLabel link)
                {
                    link.LinkColor = LinkColor;
                    link.ActiveLinkColor = ActiveLinkColor;
                    link.VisitedLinkColor = VisitedLinkColor;
                }

                if (child is PropertyGrid grid)
                {
                    ApplyToPropertyGrid(grid);
                }

                // Apply dark mode Explorer theme to ListViews — fixes group header colors
                if (child is ListView lv && lv.IsHandleCreated && IsDarkMode)
                {
                    SetWindowTheme(lv.Handle, "DarkMode_Explorer", null);
                }

                // Apply dark mode Explorer theme to TreeViews (used inside PropertyGrid)
                if (child is TreeView tv && tv.IsHandleCreated && IsDarkMode)
                {
                    SetWindowTheme(tv.Handle, "DarkMode_Explorer", null);
                }

                // ToolStripContainer has panels accessible only via properties
                if (child is ToolStripContainer tsc)
                {
                    ApplyToRecursive(tsc.ContentPanel);
                    ApplyToRecursive(tsc.TopToolStripPanel);
                    ApplyToRecursive(tsc.BottomToolStripPanel);
                    ApplyToRecursive(tsc.LeftToolStripPanel);
                    ApplyToRecursive(tsc.RightToolStripPanel);
                }

                // SplitContainer panels
                if (child is SplitContainer sc)
                {
                    ApplyToRecursive(sc.Panel1);
                    ApplyToRecursive(sc.Panel2);
                }

                // TabControl pages
                if (child is TabControl tc)
                {
                    foreach (TabPage page in tc.TabPages)
                        ApplyToRecursive(page);
                }

                // Recurse into all children
                if (child.HasChildren)
                {
                    ApplyToRecursive(child);
                }
            }
        }

        /// <summary>
        /// Applies dark mode Explorer theme to a native window handle.
        /// Fixes ListView group headers, scrollbars, and other OS-rendered elements.
        /// </summary>
        public static void ApplyDarkModeToHandle(IntPtr handle)
        {
            SetWindowTheme(handle, "DarkMode_Explorer", null);
        }

        /// <summary>
        /// Sets the immersive dark mode attribute on a window so the OS renders
        /// all common controls (including ListView group headers) in dark mode colors.
        /// </summary>
        public static void SetDarkModeForWindow(IntPtr handle, bool darkMode)
        {
            int value = darkMode ? 1 : 0;
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        }

        /// <summary>
        /// Applies dark/light mode colors to a PropertyGrid so category headers and values are readable.
        /// </summary>
        public static void ApplyToPropertyGrid(PropertyGrid grid)
        {
            if (IsDarkMode)
            {
                grid.CategoryForeColor = Color.FromArgb(96, 205, 255);      // Fluent accent for categories
                grid.CategorySplitterColor = Color.FromArgb(50, 50, 50);
                grid.LineColor = Color.FromArgb(60, 60, 60);
                grid.ViewForeColor = SystemColors.WindowText;
                grid.ViewBackColor = SystemColors.Window;
                grid.HelpForeColor = SystemColors.WindowText;
                grid.HelpBackColor = SystemColors.Window;
            }
            else
            {
                grid.CategoryForeColor = Color.FromArgb(0, 95, 184);        // Fluent blue for categories
                grid.CategorySplitterColor = SystemColors.Control;
                grid.LineColor = SystemColors.InactiveBorder;
                grid.ViewForeColor = SystemColors.WindowText;
                grid.ViewBackColor = SystemColors.Window;
            }
        }
    }
}
