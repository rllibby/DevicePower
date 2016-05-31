/*
 *  Copyright © 2016 Russell Libby
 */

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace DevicePower.Theme
{
    /// <summary>
    /// Class for theme management.
    /// </summary>
    public static class ThemeManager
    {
        #region Private fields

        /// <summary>
        /// Handle the pressed items in the listbox's
        /// </summary>
        private const string PressedKey = "ListBoxItemPressedBackgroundThemeBrush";

        /// <summary>
        /// Brush keys to override with our new theme color.
        /// </summary>
        private static readonly string[] BrushKeys =
        {
            "PhoneHighContrastSelectedForegroundThemeBrush",
            "JumpListDefaultEnabledBackground",
            "ListPickerFlyoutPresenterSelectedItemForegroundThemeBrush",
            "ProgressBarBackgroundThemeBrush",
            "PhoneAccentBrush",
            "PhoneRadioCheckBoxPressedBrush",
            "TextSelectionHighlightColorThemeBrush",
            "ButtonPressedBackgroundThemeBrush",
            "CheckBoxPressedBackgroundThemeBrush",
            "ComboBoxHighlightedBorderThemeBrush",
            "ComboBoxItemSelectedForegroundThemeBrush",
            "ComboBoxPressedBackgroundThemeBrush",
            "HyperlinkPressedForegroundThemeBrush",
            "ListBoxItemSelectedBackgroundThemeBrush",
            "ListBoxItemSelectedPointerOverBackgroundThemeBrush",
            "ListViewItemCheckHintThemeBrush",
            "ListViewItemCheckSelectingThemeBrush",
            "ListViewItemDragBackgroundThemeBrush",
            "ListViewItemSelectedBackgroundThemeBrush",
            "ListViewItemSelectedPointerOverBackgroundThemeBrush",
            "ListViewItemSelectedPointerOverBorderThemeBrush",
            "ProgressBarForegroundThemeBrush",
            "ProgressBarIndeterminateForegroundThemeBrush",
            "SliderTrackDecreaseBackgroundThemeBrush",
            "SliderTrackDecreasePointerOverBackgroundThemeBrush",
            "SliderTrackDecreasePressedBackgroundThemeBrush",
            "ToggleSwitchCurtainBackgroundThemeBrush",
            "ToggleSwitchCurtainPointerOverBackgroundThemeBrush",
            "ToggleSwitchCurtainPressedBackgroundThemeBrush",
            "LoopingSelectorSelectionBackgroundThemeBrush",
            "ComboBoxItemSelectedBackgroundThemeBrush",
            "ComboBoxSelectedBackgroundThemeBrush",
            "IMECandidateSelectedBackgroundThemeBrush",
            "ListBoxItemSelectedBackgroundThemeBrush",
            "ListViewItemSelectedBackgroundThemeBrush",
            "SearchBoxButtonBackgroundThemeBrush",
            "SearchBoxHitHighlightForegroundThemeBrush",
            "TextSelectionHighlightColorThemeBrush"
        };

        #endregion

        #region Public methods

        /// <summary>
        /// Overrides the theme color used for the application.
        /// </summary>
        /// <param name="color">The color to override the default theme with.</param>
        public static void SetThemeColor(Color color)
        {
            foreach (var brushKey in BrushKeys)
            {
                if (Application.Current.Resources.ContainsKey(brushKey))
                {
                    var solidColorBrush = Application.Current.Resources[brushKey] as SolidColorBrush;
                    if (solidColorBrush != null) solidColorBrush.Color = color;
                }
            }

            if (Application.Current.Resources.ContainsKey("ListBoxItemSelectedBackgroundThemeBrush"))
            {
                Application.Current.Resources["ListBoxItemSelectedBackgroundThemeBrush"] = new SolidColorBrush(Colors.Black);
            }
        }

        #endregion
    }
}