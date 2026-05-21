// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.Vault.Explorer
{
    /// <summary>
    /// Provides theme-aware colors following Microsoft Fluent design guidelines.
    /// Adapts colors based on whether the system is using dark or light mode.
    /// </summary>
    internal static class ThemeHelper
    {
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
        /// Applies theme-aware colors to all LinkLabels and PropertyGrids in a control tree.
        /// </summary>
        public static void ApplyTo(Control control)
        {
            foreach (Control child in control.Controls)
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

                // Recurse into containers (ToolStripContainer panels, SplitContainers, etc.)
                if (child.HasChildren)
                {
                    ApplyTo(child);
                }

                // ToolStripContainer has special panels
                if (child is ToolStripContainer tsc)
                {
                    ApplyTo(tsc.ContentPanel);
                    ApplyTo(tsc.TopToolStripPanel);
                    ApplyTo(tsc.BottomToolStripPanel);
                }

                // SplitContainer panels
                if (child is SplitContainer sc)
                {
                    ApplyTo(sc.Panel1);
                    ApplyTo(sc.Panel2);
                }
            }
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
